using UnityEngine;

namespace Rimbara.Mission
{
    // Pure, presentation-facing score. One place defines the deck contract
    // "(trees + fires) / 7" and the mission 1-4 window so the HUD never has to
    // re-derive it from raw counters.
    public readonly struct RestorationScore
    {
        public const int MissionCount = 4;

        public readonly int Seedlings;
        public readonly int Trees;
        public readonly int Fires;
        public readonly int SeedlingGoal;
        public readonly int TreeGoal;
        public readonly int FireGoal;
        public readonly int Mission;

        public int CarriedSeedlings => Seedlings - Trees;
        public int Total => Trees + Fires;
        public int Target => TreeGoal + FireGoal;
        public float Fraction => Target <= 0 ? 0f : (float)Total / Target;
        public int Percent => Mathf.RoundToInt(Fraction * 100f);
        public bool Restored => Seedlings >= SeedlingGoal && Trees >= TreeGoal && Fires >= FireGoal;

        public RestorationScore(int seedlings, int trees, int fires, int seedlingGoal, int treeGoal, int fireGoal, int mission)
        {
            Seedlings = seedlings;
            Trees = trees;
            Fires = fires;
            SeedlingGoal = seedlingGoal;
            TreeGoal = treeGoal;
            FireGoal = fireGoal;
            Mission = Mathf.Clamp(mission, 1, MissionCount);
        }

        public static RestorationScore From(ProgressSnapshot snapshot, int mission)
        {
            return new RestorationScore(snapshot.Seedlings, snapshot.Trees, snapshot.Fires,
                snapshot.SeedlingGoal, snapshot.TreeGoal, snapshot.FireGoal, mission);
        }

        public static int MissionNumberFor(MissionStage stage)
        {
            return Mathf.Clamp((int)stage + 1, 1, MissionCount);
        }
    }
}
