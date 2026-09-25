using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BetterInfestations
{
    public class HiveData_MapComponent : MapComponent
    {
        public Dictionary<Pawn, Hive> pawnToHiveDict;
        public BoolGrid withinHiveGrid;

        public HiveData_MapComponent(Map map) : base(map)
        {
            pawnToHiveDict = new Dictionary<Pawn, Hive>();
            withinHiveGrid = new BoolGrid(map);
        }

        public void RebuildHiveGrid(float radius = 8)
        {
            withinHiveGrid.Clear();

            foreach (Thing h in map.listerThings.ThingsOfDef(ThingDefOf.BI_Hive))
            {
                if (h.DestroyedOrNull()) continue;
                foreach (IntVec3 c in GenRadial.RadialCellsAround(h.Position, radius, true))
                {
                    if (c.InBounds(map))
                    {
                        withinHiveGrid.Set(c, true);
                    }
                }
            }
        }

        public void AddPawnHiveData(Pawn pawn, Hive hive)
        {
            if (pawn.DestroyedOrNull() || hive.DestroyedOrNull()) return;

            //Log.Message($"Added pawn {pawn.ThingID} to pawnToHiveDict! Size is {pawnToHiveDict.Count}");
            pawnToHiveDict[pawn] = hive;
        }

        public void RemovePawnHiveData(Pawn pawn)
        {
            //Log.Message($"Removed pawn {pawn.ThingID} from pawnToHiveDict!");
            pawnToHiveDict.Remove(pawn);
        }

        public void RemovePawnHiveDataSweep()
        {
            List<Pawn> pawnsToRemove = new List<Pawn>();
            foreach (Pawn p in pawnToHiveDict.Keys)
            {
                if (p.DestroyedOrNull() || p.Dead)
                {
                    pawnsToRemove.Add(p);
                }
            }
            if ( pawnsToRemove != null )
            {
                foreach (Pawn p in pawnsToRemove)
                {
                    pawnToHiveDict.Remove(p);
                    //Log.Message($"Removed pawn {p} from dict! Size is {pawnToHiveDict.Count}");
                }
            }
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();

            if (pawnToHiveDict.NullOrEmpty())
            {
                foreach (Hive hive in map.listerThings.ThingsOfDef(ThingDefOf.BI_Hive))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        foreach (Pawn p in hive.TryGetComp<CompSpawnerPawns>()?.spawnedPawns[i])
                        {
                            AddPawnHiveData(p, hive);
                        }
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref pawnToHiveDict, "pawnToHiveDict", keyLookMode: LookMode.Reference, valueLookMode: LookMode.Reference);
            Scribe_Deep.Look(ref withinHiveGrid, "withinHiveGrid");

            // reinitialize if null after load
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (pawnToHiveDict.NullOrEmpty()) pawnToHiveDict = new Dictionary<Pawn, Hive>();
                if (withinHiveGrid == null) withinHiveGrid = new BoolGrid(map);
            }
        }
    }
}
