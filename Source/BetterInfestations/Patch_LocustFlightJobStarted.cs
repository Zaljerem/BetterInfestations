using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
    [HarmonyPatch(typeof(Pawn_FlightTracker), nameof(Pawn_FlightTracker.Notify_JobStarted))]
    static class Patch_LocustFlightJobStarted
    {
        static bool Prefix(Job job, Pawn ___pawn)
        {
            // REVIEW: in the supplied 1.7 assembly StartJob advances the first toils before this
            // callback. An immediately ended job can already be pooled (Job.Clear sets def=null).
            // See notes/locust-flight-review.md; do not disable Locust flight or change its race def.
            if (___pawn == null || PawnKindDefOf.Locust == null || ___pawn.kindDef != PawnKindDefOf.Locust)
                return true;

            // A replacement job receives its own notification during nested StartJob. Only the
            // still-current, defined job may update flight; skip the obsolete outer notification.
            return job?.def != null && ReferenceEquals(___pawn.CurJob, job);
        }
    }
}
