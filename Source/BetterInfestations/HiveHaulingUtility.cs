using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
    // Hive gathering is a local delivery, never a storage job. Keep all retries in this helper
    // instead of using JobDriver_HaulToCell's storage-enabled placement fallback.
    public static class HiveHaulingUtility
    {
        public const int DropRadius = 5;
        private const int MaxDropAttempts = 8;

        public static bool HasLiveHome(Pawn pawn, Hive hive)
        {
            return !pawn.DestroyedOrNull() && pawn.Spawned && !hive.DestroyedOrNull()
                && hive.Spawned && hive.Map == pawn.Map;
        }

        public static bool CanDropAt(Pawn pawn, Hive hive, Thing cargo, IntVec3 cell)
        {
            if (!HasLiveHome(pawn, hive) || cargo.DestroyedOrNull() || !cell.InBounds(pawn.Map)
                || cell.DistanceToSquared(hive.Position) > DropRadius * DropRadius) return false;

            // Slot groups include stockpiles and shelves. Also reject non-slot storage buildings.
            // Check again at delivery: a zone, shelf, fire or blocking stack can appear in transit.
            return cell.Standable(pawn.Map) && !cell.Fogged(pawn.Map) && !cell.IsForbidden(pawn)
                && !cell.ContainsStaticFire(pawn.Map) && cell.GetSlotGroup(pawn.Map) == null
                && !(cell.GetFirstBuilding(pawn.Map) is IHaulDestination)
                && GenPlace.HaulPlaceBlockerIn(cargo, cell, pawn.Map, true) == null
                && pawn.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.Some);
        }

        public static bool TryFindDropCell(Pawn pawn, Hive hive, Thing cargo, out IntVec3 cell)
        {
            cell = IntVec3.Invalid;
            return HasLiveHome(pawn, hive) && CellFinder.TryFindRandomCellNear(hive.Position,
                pawn.Map, DropRadius, c => CanDropAt(pawn, hive, cargo, c), out cell);
        }

        public static IEnumerable<Toil> MakeHaulToils(JobDriver driver)
        {
            Pawn pawn = driver.pawn;
            Job job = driver.job;
            Hive hive = job.GetTarget(TargetIndex.C).Thing as Hive;
            driver.FailOn(() => !HasLiveHome(pawn, hive));
            driver.FailOnDestroyedOrNull(TargetIndex.A);

            // A loaded job may already have its target in the carry tracker.
            Toil afterPickup = Toils_General.Label();
            yield return Toils_Jump.JumpIf(afterPickup, () => pawn.carryTracker.CarriedThing != null
                && pawn.carryTracker.CarriedThing == job.GetTarget(TargetIndex.A).Thing);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, subtractNumTakenFromJobCount: true);
            yield return afterPickup;

            Toil carry = Toils_Haul.CarryHauledThingToCell(TargetIndex.B, PathEndMode.OnCell);
            Toil chooseCell = Toils_General.Do(() =>
            {
                Thing cargo = pawn.carryTracker.CarriedThing;
                if (!CanDropAt(pawn, hive, cargo, job.GetTarget(TargetIndex.B).Cell)
                    && !TryRetarget(pawn, hive, cargo, job))
                    pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
            });
            yield return chooseCell;
            yield return carry;

            int attempts = 0;
            yield return Toils_General.Do(() =>
            {
                Thing cargo = pawn.carryTracker.CarriedThing;
                IntVec3 cell = job.GetTarget(TargetIndex.B).Cell;
                if (CanDropAt(pawn, hive, cargo, cell)
                    && pawn.carryTracker.TryDropCarriedThing(cell, ThingPlaceMode.Direct, out Thing placed))
                {
                    // Gathering can also carry downed pawns, which normally have no forbiddable
                    // comp. Only mark supported food/corpse stacks, including merged deliveries.
                    if (placed?.TryGetComp<CompForbiddable>() != null) placed.SetForbidden(true);
                    if (pawn.carryTracker.CarriedThing == null) return;
                }

                // Failed or partial placement gets a fresh hive cell, never StoreUtility's fallback.
                // Bound retries in case another mod repeatedly refuses otherwise-valid placement.
                if (++attempts < MaxDropAttempts && TryRetarget(pawn, hive, pawn.carryTracker.CarriedThing, job))
                    driver.JumpToToil(carry);
                else
                    pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
            });
        }

        private static bool TryRetarget(Pawn pawn, Hive hive, Thing cargo, Job job)
        {
            if (!TryFindDropCell(pawn, hive, cargo, out IntVec3 cell) || !pawn.Reserve(cell, job)) return false;
            job.SetTarget(TargetIndex.B, cell);
            return true;
        }
    }

    public class JobDriver_HaulToHive : JobDriver_HaulToCell
    {
        // Retain the standard pickup/destination reservations and load support, with hive-only toils.
        protected override IEnumerable<Toil> MakeNewToils() => HiveHaulingUtility.MakeHaulToils(this);

        public override string GetReport() => ReportStringProcessed(job.def.reportString).CapitalizeFirst();
    }
}
