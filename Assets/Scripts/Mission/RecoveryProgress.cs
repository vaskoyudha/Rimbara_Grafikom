using System;
using System.Collections.Generic;
using System.Linq;

namespace Rimbara.Mission
{
    public enum RecoveryStage { Dormant, Briefing, Planting, PlantingDebrief, Ecosystem, EcosystemDebrief, Complete }

    // Recovery has its own registered targets and inventory. It never advances
    // the legacy MVP sequence or mistakes Zone 1's 100% for whole-story victory.
    public sealed class RecoveryProgress
    {
        private readonly HashSet<string> seedTargets;
        private readonly HashSet<string> plantTargets;
        private readonly HashSet<string> wasteTargets;
        private readonly HashSet<string> habitatTargets;
        private readonly HashSet<string> seeds = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> dug = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> planted = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> watered = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> waste = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> habitats = new HashSet<string>(StringComparer.Ordinal);
        public RecoveryStage Stage { get; private set; }
        public int TreeGoal => plantTargets.Count;
        public int WasteGoal => wasteTargets.Count;
        public int HabitatGoal => habitatTargets.Count;
        public int Collected => seeds.Count;
        public int Carried => seeds.Count - planted.Count;
        public int Dug => dug.Count;
        public int Planted => planted.Count;
        public int Watered => watered.Count;
        public int Waste => waste.Count;
        public int Habitats => habitats.Count;
        public bool WaterClean => Waste == WasteGoal;
        public float Fraction => (float)(Planted + Watered + Waste + Habitats) / (2 * TreeGoal + WasteGoal + HabitatGoal);
        public event Action Changed;

        public RecoveryProgress(IEnumerable<string> seedIds, IEnumerable<string> plantIds, IEnumerable<string> wasteIds, IEnumerable<string> habitatIds)
        {
            seedTargets = Targets(seedIds);
            plantTargets = Targets(plantIds);
            wasteTargets = Targets(wasteIds);
            habitatTargets = Targets(habitatIds);
            if (seedTargets.Count != plantTargets.Count) throw new ArgumentException("Supply exactly one Recovery seedling per planting target. Final-story supply is separate.");
        }

        private static HashSet<string> Targets(IEnumerable<string> ids)
        {
            string[] values = ids?.ToArray() ?? throw new ArgumentNullException(nameof(ids));
            var set = new HashSet<string>(values, StringComparer.Ordinal);
            if (values.Length == 0 || values.Any(string.IsNullOrWhiteSpace) || set.Count != values.Length)
                throw new ArgumentException("Recovery requires distinct nonempty registered target IDs.");
            return set;
        }

        public bool Begin(bool disasterReady, bool running) => Transition(RecoveryStage.Dormant, RecoveryStage.Briefing, disasterReady && running);
        public bool AcceptBriefing(bool running) => Transition(RecoveryStage.Briefing, RecoveryStage.Planting, running);
        public bool AcceptPlantingDebrief(bool running) => Transition(RecoveryStage.PlantingDebrief, RecoveryStage.Ecosystem, running);
        public bool AcceptEcosystemDebrief(bool running) => Transition(RecoveryStage.EcosystemDebrief, RecoveryStage.Complete, running);

        private bool Transition(RecoveryStage from, RecoveryStage to, bool allowed)
        {
            if (!allowed || Stage != from) return false;
            Stage = to;
            Changed?.Invoke();
            return true;
        }

        public bool Collect(string id, bool running)
        {
            if (!running || Stage != RecoveryStage.Planting || !seedTargets.Contains(id ?? "") || !seeds.Add(id)) return false;
            Changed?.Invoke();
            return true;
        }

        public bool Dig(string id, bool running)
        {
            if (!running || Stage != RecoveryStage.Planting || !plantTargets.Contains(id ?? "") || !dug.Add(id)) return false;
            Changed?.Invoke();
            return true;
        }

        public bool Plant(string id, bool running)
        {
            if (!running || Stage != RecoveryStage.Planting || Carried < 1 || !dug.Contains(id ?? "") || !planted.Add(id)) return false;
            Changed?.Invoke();
            return true;
        }

        public bool Water(string id, bool running)
        {
            if (!running || Stage != RecoveryStage.Planting || !planted.Contains(id ?? "") || !watered.Add(id)) return false;
            if (Watered == TreeGoal) Stage = RecoveryStage.PlantingDebrief;
            Changed?.Invoke();
            return true;
        }

        public bool CleanWaste(string id, bool running)
        {
            if (!running || Stage != RecoveryStage.Ecosystem || !wasteTargets.Contains(id ?? "") || !waste.Add(id)) return false;
            CompleteEcosystemIfReady();
            Changed?.Invoke();
            return true;
        }

        public bool RestoreHabitat(string id, bool running)
        {
            if (!running || Stage != RecoveryStage.Ecosystem || !habitatTargets.Contains(id ?? "") || !habitats.Add(id)) return false;
            CompleteEcosystemIfReady();
            Changed?.Invoke();
            return true;
        }

        private void CompleteEcosystemIfReady()
        {
            if (WaterClean && Habitats == HabitatGoal) Stage = RecoveryStage.EcosystemDebrief;
        }

        public bool IsDug(string id) => dug.Contains(id);
        public bool IsPlanted(string id) => planted.Contains(id);
        public bool IsWatered(string id) => watered.Contains(id);
        public bool IsCollected(string id) => seeds.Contains(id);
        public bool IsWasteCleaned(string id) => waste.Contains(id);
        public bool IsHabitatRestored(string id) => habitats.Contains(id);

        public void Reset()
        {
            seeds.Clear(); dug.Clear(); planted.Clear(); watered.Clear(); waste.Clear(); habitats.Clear();
            Stage = RecoveryStage.Dormant;
            Changed?.Invoke();
        }
    }
}
