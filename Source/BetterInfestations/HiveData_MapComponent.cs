using System.Collections.Generic;
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
            // Only index living pawns and hives currently on this component's map.
            if (!IsValidAssociation(pawn, hive)) return;

            //Log.Message($"Added pawn {pawn.ThingID} to pawnToHiveDict! Size is {pawnToHiveDict.Count}");
            pawnToHiveDict[pawn] = hive;
        }

        public void RemovePawnHiveData(Pawn pawn)
        {
            // Dictionary.Remove rejects null keys, including unresolved save references.
            if (pawn == null) return;
            //Log.Message($"Removed pawn {pawn.ThingID} from pawnToHiveDict!");
            pawnToHiveDict.Remove(pawn);
        }

        public void RemovePawnHiveDataSweep()
        {
            List<Pawn> pawnsToRemove = new List<Pawn>();
            foreach (KeyValuePair<Pawn, Hive> entry in pawnToHiveDict)
            {
                // A surviving pawn must not retain a destroyed, despawned or off-map home.
                if (!IsValidAssociation(entry.Key, entry.Value))
                {
                    pawnsToRemove.Add(entry.Key);
                }
            }
            foreach (Pawn p in pawnsToRemove)
            {
                pawnToHiveDict.Remove(p);
            }
        }

        // Keep the same ownership rules for insertion, lookup and post-load cleanup.
        public bool IsValidAssociation(Pawn pawn, Hive hive)
        {
            return !pawn.DestroyedOrNull() && !pawn.Dead && pawn.Spawned && pawn.Map == map
                && !hive.DestroyedOrNull() && hive.Spawned && hive.Map == map;
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();

            // Group membership is authoritative. Rebuild even a partially populated saved index,
            // after references resolve, and tolerate missing components/collections in old saves.
            pawnToHiveDict.Clear();
            foreach (Thing thing in map.listerThings.ThingsOfDef(ThingDefOf.BI_Hive))
            {
                Hive hive = thing as Hive;
                CompSpawnerPawns comp = hive?.CompSpawnerPawns;
                if (comp == null) continue;
                foreach (HashSet<Pawn> group in comp.spawnedPawns)
                {
                    if (group == null) continue;
                    foreach (Pawn pawn in group)
                    {
                        AddPawnHiveData(pawn, hive);
                    }
                }
            }
            // The grid is derived data; never trust a stale serialized proximity cache.
            RebuildHiveGrid();
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
