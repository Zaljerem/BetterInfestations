using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
    [HarmonyPatch(typeof(JobDriver_HaulToCell), "MakeNewToils")]
    static class Patch_LegacyHiveHauling
    {
        static bool Prefix(JobDriver_HaulToCell __instance, ref IEnumerable<Toil> __result)
        {
            // Old saves still contain vanilla HaulToCell jobs issued by the gathering AI.
            // Limit the compatibility fix to non-player-forced, non-storage insect hive hauls.
            Job job = __instance.job;
            Pawn pawn = __instance.pawn;
            if (job.def != RimWorld.JobDefOf.HaulToCell || job.haulMode != HaulMode.ToCellNonStorage
                || job.playerForced || pawn.Faction != Faction.OfInsects) return true;

            Hive hive = job.GetTarget(TargetIndex.C).Thing as Hive ?? HiveUtility.GetHive(pawn);
            if (hive == null) return true;
            job.SetTarget(TargetIndex.C, hive);
            __result = HiveHaulingUtility.MakeHaulToils(__instance);
            return false;
        }
    }
}
