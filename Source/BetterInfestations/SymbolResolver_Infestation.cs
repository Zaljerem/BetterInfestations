using RimWorld;
using RimWorld.BaseGen;
using System;
using Unity.Burst.Intrinsics;
using UnityEngine;
using Verse;

namespace BetterInfestations
{
    public class SymbolResolver_Infestation : SymbolResolver
    {

        public static readonly SimpleCurve PointsFactorCurve = new SimpleCurve
    {
       new CurvePoint(0f, 0.7f),
       new CurvePoint(2000f, 0.55f),
       new CurvePoint(5000f, 0.45f)
    };

        public override void Resolve(ResolveParams rp)
        {
            if (BetterInfestationsMod.settings == null)
                return;

            Map map = BaseGen.globalSettings.map;

            //float baseThreat = StorytellerUtility.DefaultThreatPointsNow(map);
            float baseThreat = StorytellerUtility.DefaultSiteThreatPointsNow();

            Log.Message($"[BI] Base threat points: {baseThreat}");

            float curvedThreat = baseThreat * PointsFactorCurve.Evaluate(baseThreat);

            Log.Message($"[BI] Curved threat points: {curvedThreat}");

            float threatScale = Find.Storyteller.difficulty.threatScale;

            Log.Message($"[BI] Threat scale: {threatScale}");

            float finalThreat = curvedThreat * threatScale;

            Log.Message($"[BI] FINAL threat points: {finalThreat}");

            float threatPoints = finalThreat;            

            // Determine scale
            int hiveCount = CalculateHiveCount(threatPoints);
            int pawnsPerHive = CalculatePawnsPerHive(threatPoints, hiveCount);

            Log.Message($"[BI] Hives: {hiveCount}");
            Log.Message($"[BI] Pawns per hive: {pawnsPerHive}");

            if (!TryFindRootCell(map, out IntVec3 rootCell))
                return;

            // Spawn root hive
            Hive rootHive = SpawnHive(rootCell, map, pawnsPerHive, spawnQueen: true, threatPoints);          

            if (rootHive == null)
                return;

           
            // Spawn child hives around the root
            for (int i = 1; i < hiveCount; i++)
            {
                if (rootHive.GetComp<CompSpawnerHives>()
                    .TrySpawnChildHive(out Hive childHive))
                {
                    SpawnHiveContents(
                        childHive,
                        pawnsPerHive,
                        threatPoints: threatPoints,
                        spawnQueen: Rand.Chance(QueenChance(threatPoints)
                        )
                    );
                    
                }
            }

            HiveUtility.SpawnRandomCorpses(rootHive);

            if (finalThreat > 1000f)
            {
                Log.Message($"[BI] Threat points > 1000, bonus loot generated");
                HiveUtility.SpawnRandomItems(rootHive);
            }
        }

        #region Scaling

        private int CalculateHiveCount(float threatPoints)
        {
            
            int baseHives = Mathf.Clamp(
                Mathf.RoundToInt(threatPoints / 220f),
                1,
                BetterInfestationsMod.settings.maxHivesPerMap
            );

            return Rand.RangeInclusive(baseHives, baseHives + 1);
        }

        private int CalculatePawnsPerHive(float threatPoints, float hiveCount)
        {

            float pawnPointsPerHive = threatPoints / hiveCount;

            int pawnCount = Mathf.RoundToInt(pawnPointsPerHive / 40f);
            pawnCount = Mathf.Clamp(pawnCount, 3, 15);
            return pawnCount;

        }

        //private float QueenChance(float threatPoints)
        // {
        // One guaranteed queen at root
        // Extra queens only at high threat
        //    return Mathf.Clamp01(threatPoints / 1500f);
        // }

        private float QueenChance(float threatPoints)
        {
            const float baseChance = 0.02f; // 2% minimum
            const float maxChance = 0.65f;  // never guaranteed on child hives

            // Threat scaling tuned for site-level points
            float scaled = threatPoints / 1200f;

            return Mathf.Clamp(
                baseChance + scaled,
                baseChance,
                maxChance
            );
        }

        #endregion

        #region Placement

        private bool TryFindRootCell(Map map, out IntVec3 result)
        {
            // Vanilla infestation logic - too restrictive
            //if (Patches.Patch_InfestationCellFinder_TryFindCell.TryFindCell(out result, map))
             //return true;

            // Random standable, unfogged near center - better for a world site
            Predicate<IntVec3> validator = c =>
                c.Standable(map) &&
                !c.Fogged(map) &&
                c.GetRoom(map).TouchesMapEdge == false;

            Predicate<IntVec3> validator1 = cell =>
                       !cell.Fogged(map) && cell.Walkable(map);

            return RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(validator1, map, out result);
            
        }

        #endregion

        #region Hive Spawning

        private Hive SpawnHive(
            IntVec3 cell,
            Map map,
            int pawnsPerHive,
            bool spawnQueen,
            float threatPoints)
        {
            Hive hive = ThingMaker.MakeThing(ThingDefOf.BI_Hive) as Hive;
            hive.SetFaction(Faction.OfInsects);

            hive = GenSpawn.Spawn(hive, cell, map) as Hive;

            if (hive == null)
                return null;

            SpawnHiveContents(hive, pawnsPerHive, spawnQueen, threatPoints);
            return hive;
        }

        private void SpawnHiveContents(
            Hive hive,
            int pawnsPerHive,
            bool spawnQueen,
            float threatPoints)
        {
            // Glowpods
            TrySpawnFromComp<CompSpawner>(hive, ThingDefOf.GlowPod, 1);

            // Jelly
            TrySpawnFromComp<CompSpawnerJelly>(
                hive,
                ThingDefOf.InsectJelly,
                Rand.Range(6, 20)
            );

            // Regular insects
            for (int i = 0; i < pawnsPerHive; i++)
            {
                hive.CompSpawnerPawns.TrySpawnPawn(
                    0,
                    out Pawn _,
                    hive.CompSpawnerPawns.RandomWeightedPawnKindDef(threatPoints),
                    false
                );
            }

            // Queen
            if (spawnQueen)
            {
                hive.CompSpawnerPawns.TrySpawnPawn(
                    0,
                    out Pawn _,
                    PawnKindDefOf.BI_Queen,
                    false
                );
            }

            HiveUtility.SpawnRandomItems(hive);
        }

        private void TrySpawnFromComp<T>(
            Hive hive,
            ThingDef expected,
            int count) where T : ThingComp
        {
            T comp = hive.GetComp<T>();
            if (comp is CompSpawner spawner &&
                spawner.PropsSpawner.thingToSpawn == expected)
            {
                for (int i = 0; i < count; i++)
                    spawner.TryDoSpawn();
            }
        }      


        #endregion
    }
}
