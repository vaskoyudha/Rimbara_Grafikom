using System;
using System.Collections.Generic;

namespace Rimbara.Mission
{
    public enum DisasterStage { Briefing, Equipment, StopTheFlames, CleanTheWasteland, RecoveryReady }

    // First chapter of the approved five-mission story. Pure logic: no Unity,
    // HUD counters, automatic victory or bypass of the physical interactions.
    public sealed class DisasterProgress
    {
        private readonly HashSet<string> fires = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> debris = new HashSet<string>(StringComparer.Ordinal);
        public int FireGoal { get; }
        public int DebrisGoal { get; }
        public int Fires => fires.Count;
        public int Debris => debris.Count;
        public bool HasEquipment { get; private set; }
        public DisasterStage Stage { get; private set; } = DisasterStage.Briefing;
        public float ChapterFraction => (float)(Fires + Debris) / (FireGoal + DebrisGoal);
        public event Action Changed;

        public DisasterProgress(int fireGoal, int debrisGoal)
        {
            if (fireGoal < 1 || debrisGoal < 1)
                throw new ArgumentOutOfRangeException(nameof(fireGoal), "Supply fires and designated debris for the Disaster chapter.");
            FireGoal = fireGoal;
            DebrisGoal = debrisGoal;
        }

        public bool AcceptBriefing(bool sessionRunning)
        {
            if (!sessionRunning || Stage != DisasterStage.Briefing) return false;
            Stage = DisasterStage.Equipment;
            Changed?.Invoke();
            return true;
        }

        public bool CollectEquipment(bool sessionRunning)
        {
            if (!sessionRunning || Stage != DisasterStage.Equipment || HasEquipment) return false;
            HasEquipment = true;
            Stage = DisasterStage.StopTheFlames;
            Changed?.Invoke();
            return true;
        }

        public bool Extinguish(string id, bool sessionRunning)
        {
            if (!sessionRunning || Stage != DisasterStage.StopTheFlames || !HasEquipment
                || string.IsNullOrWhiteSpace(id) || Fires >= FireGoal || !fires.Add(id)) return false;
            if (Fires == FireGoal) Stage = DisasterStage.CleanTheWasteland;
            Changed?.Invoke();
            return true;
        }

        public bool ClearDebris(string id, bool sessionRunning)
        {
            if (!sessionRunning || Stage != DisasterStage.CleanTheWasteland
                || string.IsNullOrWhiteSpace(id) || Debris >= DebrisGoal || !debris.Add(id)) return false;
            if (Debris == DebrisGoal) Stage = DisasterStage.RecoveryReady;
            Changed?.Invoke();
            return true;
        }

        public void Reset()
        {
            fires.Clear();
            debris.Clear();
            HasEquipment = false;
            Stage = DisasterStage.Briefing;
            Changed?.Invoke();
        }
    }
}
