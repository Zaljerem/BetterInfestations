using RimWorld;
using RimWorld.BaseGen;
using System;
using Verse;
using Verse.Noise;

namespace BetterInfestations
{
    public class SymbolResolver_Infestation : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            if (BetterInfestationsMod.settings == null) return;

            //if (!Patches.Patch_InfestationCellFinder_TryFindCell.TryFindCell(out IntVec3 pos, BaseGen.globalSettings.map))
            //{
            //   return;
            // }
            //InfestationCellFinder.TryFindCell(out IntVec3 pos, BaseGen.globalSettings.map);
            //Patches.Patch_InfestationCellFinder_TryFindCell.TryFindCell(out IntVec3 pos, BaseGen.globalSettings.map);

            Predicate<IntVec3> validator1 = cell =>
                       !cell.Fogged(BaseGen.globalSettings.map) && cell.Walkable(BaseGen.globalSettings.map);

            RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(validator1, BaseGen.globalSettings.map, out IntVec3 pos);

            //CellFinder.TryFindRandomCell(BaseGen.globalSettings.map, validator1, out IntVec3 pos);   

            //CellFinderLoose.TryGetRandomCellWith(validator1, BaseGen.globalSettings.map, 1000, out IntVec3 pos);

            int num = BetterInfestationsMod.settings.maxHivesPerMap;
            Hive hive = (Hive)ThingMaker.MakeThing(ThingDefOf.BI_Hive);
            hive.SetFaction(Faction.OfInsects);
            hive = (Hive)GenSpawn.Spawn(hive, pos, BaseGen.globalSettings.map);
            if (hive != null)
            {
                CompSpawner compSpawner = hive.GetComp<CompSpawner>();
                if (compSpawner.PropsSpawner.thingToSpawn == RimWorld.ThingDefOf.GlowPod)
                {
                    compSpawner.TryDoSpawn();
                }
                CompSpawnerJelly compSpawnerJelly = hive.GetComp<CompSpawnerJelly>();
                if (compSpawnerJelly.PropsSpawner.thingToSpawn == RimWorld.ThingDefOf.InsectJelly)
                {
                    for (int i = 0; i < Rand.Range(6, 20); i++)
                    {
                        compSpawnerJelly.TryDoSpawn();
                    }
                }
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < Rand.Range(2, 8); j++)
                    {
                        hive.CompSpawnerPawns.TrySpawnPawn(i, out Pawn _, hive.CompSpawnerPawns.RandomPawnKindDef(), false);
                    }
                }
                hive.CompSpawnerPawns.TrySpawnPawn(0, out Pawn _, PawnKindDefOf.BI_Queen, false);
                //HiveUtility.SpawnRandomItems(hive, DefDatabase<ThingDef>.AllDefsListForReading);

                for (int i = 0; i < num - 1; i++)
                {
                    //Log.Message($"Hives: {num}");
                    if (hive.GetComp<CompSpawnerHives>().TrySpawnChildHive(out Hive newHive))
                    {
                        newHive.SetFaction(hive.Faction);
                        compSpawner = newHive.GetComp<CompSpawner>();
                        if (compSpawner.PropsSpawner.thingToSpawn == RimWorld.ThingDefOf.GlowPod)
                        {
                           // Log.Message("Try spawn glowpod");
                            compSpawner.TryDoSpawn();
                        }
                        compSpawnerJelly = newHive.GetComp<CompSpawnerJelly>();
                        if (compSpawnerJelly.PropsSpawner.thingToSpawn == RimWorld.ThingDefOf.InsectJelly)
                        {
                            for (int j = 0; j < Rand.Range(6, 20); j++)
                            {
                               // Log.Message("Try spawn jelly");
                                compSpawnerJelly.TryDoSpawn();
                            }
                        }
                        for (int j = 0; j < 2; j++)
                        {
                            for (int k = 0; k < Rand.Range(3, 8); k++)
                            {
                               // Log.Message("Try spawn pawns");
                                newHive.CompSpawnerPawns.TrySpawnPawn(j, out Pawn _, newHive.CompSpawnerPawns.RandomPawnKindDef(), false);
                            }
                        }
                        if (Rand.Range(1, 100) <= 50)
                        {
                           // Log.Message("Try spawn pawns");
                            newHive.CompSpawnerPawns.TrySpawnPawn(0, out Pawn _, PawnKindDefOf.BI_Queen, false);
                        }
                       // Log.Message("Try spawn items");
                        HiveUtility.SpawnRandomItems(newHive, DefDatabase<ThingDef>.AllDefsListForReading);
                    }
                    else
                    {
                       // Log.Message("TrySpawnChildHive failed");
                    }

                }
                //Log.Message("Exit child spawn");
                HiveUtility.SpawnRandomCorpses(hive);
            }
        }      

    }
}