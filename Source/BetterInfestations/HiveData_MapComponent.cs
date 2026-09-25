using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BetterInfestations
{
    public class HiveData_MapComponent : MapComponent
    {
        public Dictionary<Pawn, Hive> pawnToHiveDict = new Dictionary<Pawn, Hive>();

        public HiveData_MapComponent(Map map) : base(map)
        {
            pawnToHiveDict = new Dictionary<Pawn, Hive>();
        }

        public void AddPawnHiveData(Pawn pawn, Hive hive)
        {
            if (pawn.DestroyedOrNull() || hive.DestroyedOrNull()) return;

            Log.Message($"Added pawn {pawn.ThingID} to pawnToHiveDict!");
            pawnToHiveDict[pawn] = hive;
        }

        public void RemovePawnHiveData(Pawn pawn, Hive hive)
        {
            Log.Message($"Removed pawn {pawn.ThingID} from pawnToHiveDict!");
            pawnToHiveDict.Remove(pawn);
        }

        public void RegeneratePawnsFromHive(Pawn pawn, Hive hive)
        {
            // placeholder
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();

            if (pawnToHiveDict == null || pawnToHiveDict.NullOrEmpty())
            {
                foreach (Hive hive in map.listerThings.ThingsOfDef(ThingDefOf.BI_Hive))
                {
                    for (int i = 0; i < 4; i++)
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

            // reinitialize if null after load
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (pawnToHiveDict == null || pawnToHiveDict.NullOrEmpty())
                {
                    Log.Warning($"BetterInfestations: pawnToHiveDict not found after load!");
                    pawnToHiveDict = new Dictionary<Pawn, Hive>();
                }
            }
        }
    }
}
