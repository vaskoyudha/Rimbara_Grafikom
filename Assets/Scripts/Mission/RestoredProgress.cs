using System;

namespace Rimbara.Mission
{
    public enum RestoredStage { Dormant, TimeTransition, ElderBriefing, FinalPlanting, FinalWatering, Viewpoint, Ending, Complete }

    public sealed class RestoredProgress
    {
        private readonly string bedId;
        private readonly string viewpointId;
        public RestoredStage Stage { get; private set; }
        public bool HasSeedling { get; private set; }
        public bool Planted { get; private set; }
        public bool Watered { get; private set; }
        public bool ViewpointVisited { get; private set; }
        public bool EndingSkipped { get; private set; }
        public event Action Changed;
        public RestoredProgress(string plantingId, string panoramaId)
        {
            if (string.IsNullOrWhiteSpace(plantingId) || string.IsNullOrWhiteSpace(panoramaId) || plantingId == panoramaId)
                throw new ArgumentException("Final planting and viewpoint require distinct registered IDs.");
            bedId = plantingId; viewpointId = panoramaId;
        }
        private bool Advance(RestoredStage from, RestoredStage to, bool allowed)
        {
            if (!allowed || Stage != from) return false;
            Stage = to; Changed?.Invoke(); return true;
        }
        public bool Begin(bool recoveryComplete, bool running) => Advance(RestoredStage.Dormant, RestoredStage.TimeTransition, recoveryComplete && running);
        public bool CompleteTransition(bool directorAllowed) => Advance(RestoredStage.TimeTransition, RestoredStage.ElderBriefing, directorAllowed);
        public bool AcceptBriefing(bool running)
        {
            if (!running || Stage != RestoredStage.ElderBriefing || HasSeedling) return false;
            HasSeedling = true; Stage = RestoredStage.FinalPlanting; Changed?.Invoke(); return true;
        }
        public bool Plant(string id, bool running)
        {
            if (!running || Stage != RestoredStage.FinalPlanting || !HasSeedling || id != bedId) return false;
            HasSeedling = false; Planted = true; Stage = RestoredStage.FinalWatering; Changed?.Invoke(); return true;
        }
        public bool Water(string id, bool running)
        {
            if (!running || Stage != RestoredStage.FinalWatering || !Planted || id != bedId) return false;
            Watered = true; Stage = RestoredStage.Viewpoint; Changed?.Invoke(); return true;
        }
        public bool Visit(string id, bool running)
        {
            if (!running || Stage != RestoredStage.Viewpoint || !Watered || id != viewpointId) return false;
            ViewpointVisited = true; Stage = RestoredStage.Ending; Changed?.Invoke(); return true;
        }
        public bool FinishEnding(bool directorAllowed, bool skipped)
        {
            if (!directorAllowed || Stage != RestoredStage.Ending || !ViewpointVisited) return false;
            EndingSkipped = skipped; Stage = RestoredStage.Complete; Changed?.Invoke(); return true;
        }
        /// <summary>Restores saved final-chapter state from persistent storage.</summary>
        public void RestoreState(RestoredStage stage, bool hasSeedling, bool planted, bool watered, bool viewpointVisited, bool endingSkipped)
        {
            Stage = stage;
            HasSeedling = hasSeedling;
            Planted = planted;
            Watered = watered;
            ViewpointVisited = viewpointVisited;
            EndingSkipped = endingSkipped;
            Changed?.Invoke();
        }
        public void Reset()
        {
            Stage = RestoredStage.Dormant; HasSeedling = Planted = Watered = ViewpointVisited = EndingSkipped = false; Changed?.Invoke();
        }
    }
}
