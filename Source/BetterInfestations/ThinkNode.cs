using System.Runtime.CompilerServices;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
    public class ThinkNode_ConditionalHiveCanReproduce : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return (pawn.mindState.duty.focus.Thing as Hive)?.GetComp<CompSpawnerHives>().canSpawnHives ?? false;
        }
    }
    public class ThinkNode_PerTick : ThinkNode_Priority
    {
        private float ticks = -1f;
        //private float savedTick = -1f;

        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            if (Find.TickManager.TicksGame < GetLastTryTick(pawn) + ticks)
            {
                return ThinkResult.NoJob;
            }
            //Log.Message($"Pawn {pawn.ThingID} ticked on {Find.TickManager.TicksGame}, from {GetLastTryTick(pawn)}. Interval {ticks}, Delta {Find.TickManager.TicksGame - GetLastTryTick(pawn)}");
            SetLastTryTick(pawn, Find.TickManager.TicksGame);

            //if (Find.TickManager.TicksGame < savedTick)
            //{
            //    return ThinkResult.NoJob;
            //}
            //savedTick = Find.TickManager.TicksGame + ticks;
            //pawn.timetable.times.

            return base.TryIssueJobPackage(pawn, jobParams);
        }

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_PerTick obj = (ThinkNode_PerTick)base.DeepCopy(resolve);
            obj.ticks = ticks;
            //obj.savedTick = savedTick;
            return obj;
        }

        private int GetLastTryTick(Pawn pawn)
        {
            if (pawn.mindState.thinkData.TryGetValue(base.UniqueSaveKey, out var value))
            {
                return value;
            }
            return -99999;
        }

        private void SetLastTryTick(Pawn pawn, int val)
        {
            pawn.mindState.thinkData[base.UniqueSaveKey] = val;
        }
    }
}