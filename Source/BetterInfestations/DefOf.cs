using RimWorld;
using Verse;
using Verse.AI;

namespace BetterInfestations
{
    [DefOf]
    public static class ThingDefOf
    {
        public static ThingDef BI_Queen;
        public static ThingDef BI_Hive;
        public static ThingDef BI_TunnelHiveSpawner;

        public static ThingDef GlowPod;
        public static ThingDef InsectJelly;
    }
    [DefOf]
    public static class DutyDefOf
    {
        public static DutyDef BI_DefendAndExpandHive;
        public static DutyDef BI_DefendHiveAggressively;
        public static DutyDef BI_HiveHunters;
    }
    [DefOf]
    public static class JobDefOf
    {
        public static JobDef BI_Maintain;
        public static JobDef BI_Butcher;
        public static JobDef BI_GotoSpawnHive;
        public static JobDef BI_GotoPatrol;
        public static JobDef BI_ChewConduits;
    }
    [DefOf]
    public static class PawnKindDefOf
    {
        public static PawnKindDef BI_Queen;

        [MayRequire("zal.vfeinsectoid")]
        public static PawnKindDef VFEI_Insectoid_RoyalMegaspider;
        [MayRequire("zal.vfeinsectoid")]
        public static PawnKindDef VFEI_Insectoid_Gigalocust;
        [MayRequire("zal.vfeinsectoid")]
        public static PawnKindDef VFEI_Insectoid_Megapede;
        [MayRequire("zal.vaecaves")]
        public static PawnKindDef VAECaves_InsectoidHulk;
        [MayRequire("zal.vaecaves")]
        public static PawnKindDef VAECaves_GiantSpider;
        [MayRequire("zal.vaecaves")]
        public static PawnKindDef VAECaves_AncientGiantSpider;

        public static PawnKindDef AncientSoldier;
        public static PawnKindDef Drifter;
        public static PawnKindDef SpaceRefugee;

        [MayRequireOdyssey]
        public static PawnKindDef Locust;
        [MayRequireOdyssey]
        public static PawnKindDef Larva;
        [MayRequireOdyssey]
        public static PawnKindDef HiveQueen;
    }
    [DefOf]
    public static class SoundDefOf
    {
        public static SoundDef Hive_Spawn;
        public static SoundDef Tunnel;
        public static SoundDef DragSlider;
    }
}