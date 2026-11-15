using System;
using UnityEngine;

namespace Rimbara.Mission
{
    public enum MissionStage { FirstSteps, SeedsOfHope, StopTheFire, ANewBeginning, Complete }

    // Ordered state machine; counters are owned by GameState, not by the HUD.
    [DefaultExecutionOrder(-250)]
    public sealed class MissionSystem : MonoBehaviour
    {
        [SerializeField] private GameState state;
        [SerializeField] private Transform inspectionMarker;
        [SerializeField] private Transform volunteerPost;
        [SerializeField] private Transform plantingZone;
        [SerializeField] private Transform fireZone;
        [SerializeField] private DisasterChapter disasterChapter;
        public DisasterChapter DisasterChapter => disasterChapter;
        public void ConfigureDisaster(DisasterChapter chapter) { disasterChapter = chapter; }
        private MissionSequence sequence;
        public MissionStage Stage => sequence == null ? MissionStage.FirstSteps : sequence.Stage;
        public GameState State => state;
        public Transform ActiveDestination => Stage == MissionStage.FirstSteps ? inspectionMarker
            : Stage == MissionStage.SeedsOfHope ? plantingZone
            : Stage == MissionStage.StopTheFire ? fireZone : volunteerPost;
        public event Action<MissionStage> MissionChanged;
        public event Action Completed;
        public bool CanCollectOrPlant => disasterChapter == null && state.SessionRunning && Stage == MissionStage.SeedsOfHope;
        public bool CanExtinguish => disasterChapter != null ? disasterChapter.CanExtinguish : state.SessionRunning && Stage == MissionStage.StopTheFire;

        public void Configure(GameState gameState, Transform inspection, Transform post, Transform planting, Transform fire)
        {
            state = gameState;
            inspectionMarker = inspection;
            volunteerPost = post;
            plantingZone = planting;
            fireZone = fire;
        }

        private void Awake()
        {
            if (state == null) throw new InvalidOperationException("MissionSystem needs a GameState.");
            sequence = new MissionSequence(state.Progress);
            sequence.Changed += OnMissionChanged;
            sequence.Completed += OnCompleted;
        }

        public void ResetMissions()
        {
            sequence.Reset();
        }

        public bool ReachInspectionMarker()
        {
            if (disasterChapter != null) return false;
            return sequence.TryInspect(state.SessionRunning);
        }

        public bool ReturnToPost()
        {
            if (disasterChapter != null) return false;
            return sequence.TryReturn(state.SessionRunning);
        }

        private void OnMissionChanged(MissionStage stage) { MissionChanged?.Invoke(stage); }
        private void OnCompleted() { Completed?.Invoke(); }

        public string Title => Stage == MissionStage.FirstSteps ? "LANGKAH PERTAMA"
            : Stage == MissionStage.SeedsOfHope ? "BENIH HARAPAN"
            : Stage == MissionStage.StopTheFire ? "PADAMKAN API"
            : Stage == MissionStage.ANewBeginning ? "AWAL YANG BARU" : "ARUNIKA PULIH";

        public string Instruction
        {
            get
            {
                ProgressSnapshot progress = state.Snapshot;
                switch (Stage)
                {
                    case MissionStage.FirstSteps: return "Datangi penanda kuning dan periksa hutan yang terdampak.";
                    case MissionStage.SeedsOfHope:
                        return "Ambil " + progress.SeedlingGoal + " bibit lokal dan tanam " + progress.TreeGoal + " pohon. Tekan E di dekat objek.";
                    case MissionStage.StopTheFire:
                        return "Dekati " + progress.FireGoal + " titik api. Arahkan bidikan dan tahan klik kiri untuk menyemprot.";
                    case MissionStage.ANewBeginning: return "Semua api telah padam. Kembali ke posko relawan untuk menyelesaikan tugas.";
                    default: return "Kepedulianmu menjadi awal yang baru bagi Arunika.";
                }
            }
        }

        private void OnDestroy()
        {
            if (sequence == null) return;
            sequence.Changed -= OnMissionChanged;
            sequence.Completed -= OnCompleted;
            sequence.Dispose();
        }
    }
}
