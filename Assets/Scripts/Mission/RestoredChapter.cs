using System;
using Rimbara.Interaction;
using Rimbara.UI;
using UnityEngine;

namespace Rimbara.Mission
{
    [DefaultExecutionOrder(-160)]
    public sealed class RestoredChapter : MonoBehaviour
    {
        [SerializeField] private RecoveryChapter recovery;
        [SerializeField] private Transform elder;
        [SerializeField] private FinalPlantingPoint finalBed;
        [SerializeField] private RestoredViewpoint viewpoint;
        [SerializeField] private EndingDirector director;
        public RecoveryChapter Recovery => recovery;
        public RestoredProgress Progress { get; private set; }
        public Transform Elder => elder;
        public FinalPlantingPoint FinalBed => finalBed;
        public RestoredViewpoint Viewpoint => viewpoint;
        public EndingDirector Director => director;
        public bool WorldApplied => director != null && director.GetComponent<RestoredEnvironment>().Applied;
        public bool Active => Progress != null && Progress.Stage != RestoredStage.Dormant;
        public bool EnvironmentReady => Active && Progress.Stage >= RestoredStage.ElderBriefing;
        public bool Running => recovery.Running;
        public bool CanWater => Running && Progress.Stage == RestoredStage.FinalWatering;
        public event Action Changed;
        public void Configure(RecoveryChapter previous, Transform guide, FinalPlantingPoint tree, RestoredViewpoint panorama, EndingDirector cinematic)
        { recovery = previous; elder = guide; finalBed = tree; viewpoint = panorama; director = cinematic; }
        private void Awake()
        {
            if (recovery == null || elder == null || finalBed == null || viewpoint == null || director == null)
                throw new InvalidOperationException("Restored chapter requires its Recovery, elder, final tree, viewpoint and ending director.");
            Progress = new RestoredProgress(finalBed.ObjectId, viewpoint.ObjectId); Progress.Changed += Notify;
        }
        private void Notify() { Changed?.Invoke(); }
        public bool Begin()
        {
            if (!Progress.Begin(recovery.Progress.Stage == RecoveryStage.Complete, Running)) return false;
            director.BeginTransition(); return true;
        }
        public bool AcceptBriefing() => Progress.AcceptBriefing(Running);

        /// <summary>Applies restored environment visuals immediately when loading saved post-transition progress.</summary>
        public void SyncVisualsFromProgress()
        {
            if (Progress == null || director == null || Progress.Stage < RestoredStage.TimeTransition) return;
            RestoredEnvironment env = director.GetComponent<RestoredEnvironment>();
            if (env != null) env.ApplyFromSave();
        }
        public bool VisitViewpoint(string id)
        {
            if (!Progress.Visit(id, Running)) return false;
            director.BeginEnding(); return true;
        }
        public Transform Destination => Progress.Stage == RestoredStage.FinalPlanting || Progress.Stage == RestoredStage.FinalWatering
            ? finalBed.transform : Progress.Stage == RestoredStage.Viewpoint ? viewpoint.transform : elder;
        public string DestinationLabel => Progress.Stage == RestoredStage.FinalPlanting ? "Lahan / Bibit terakhir"
            : Progress.Stage == RestoredStage.FinalWatering ? "Siram bibit terakhir" : Progress.Stage == RestoredStage.Viewpoint ? "Titik pandang hutan" : "Tetua Desa";
        public string Instruction => Progress.Stage == RestoredStage.FinalPlanting ? "Bawa bibit dari Tetua ke lahan pemulihan desa. Tekan E untuk menanam."
            : Progress.Stage == RestoredStage.FinalWatering ? "Bidik bibit terakhir dan tahan KLIK KIRI untuk menyiram."
            : Progress.Stage == RestoredStage.Viewpoint ? "Datangi titik pandang. Tekan E untuk menyaksikan hutan dan desa yang pulih."
            : "Temui Tetua Desa di dekat kebun desa. Tekan E untuk berbicara.";
        private void OnDestroy() { if (Progress != null) Progress.Changed -= Notify; }
    }
}
