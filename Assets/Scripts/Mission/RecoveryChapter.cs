using System;
using System.Linq;
using Rimbara.Interaction;
using UnityEngine;

namespace Rimbara.Mission
{
    [DefaultExecutionOrder(-180)]
    public sealed class RecoveryChapter : MonoBehaviour
    {
        [SerializeField] private DisasterChapter disaster;
        [SerializeField] private Transform ranger;
        [SerializeField] private RecoverySeedling[] seedlings;
        [SerializeField] private RecoveryPlantingPoint[] beds;
        [SerializeField] private RecoveryTask[] tasks;
        public RecoveryProgress Progress { get; private set; }
        public RestoredChapter Restored => GetComponent<RestoredChapter>();
        public DisasterChapter Disaster => disaster;
        public Transform Ranger => ranger;
        public bool Active => Progress != null && Progress.Stage != RecoveryStage.Dormant;
        public bool CanPlant => Active && Running && Progress.Stage == RecoveryStage.Planting;
        public bool CanRestore => Active && Running && Progress.Stage == RecoveryStage.Ecosystem;
        public bool Running => disaster.State.SessionRunning;
        public event Action Changed;

        public void Configure(DisasterChapter firstChapter, Transform guide, RecoverySeedling[] supply, RecoveryPlantingPoint[] targets, RecoveryTask[] restoration)
        {
            disaster = firstChapter; ranger = guide; seedlings = supply; beds = targets; tasks = restoration;
        }

        private void Awake()
        {
            if (disaster == null || ranger == null || seedlings == null || beds == null || tasks == null)
                throw new InvalidOperationException("Recovery requires its authored ranger, nursery, planting beds and ecosystem targets.");
            Progress = new RecoveryProgress(seedlings.Select(s => s.ObjectId), beds.Select(b => b.ObjectId),
                tasks.Where(t => t.Kind == RecoveryTaskKind.Waste).Select(t => t.ObjectId),
                tasks.Where(t => t.Kind == RecoveryTaskKind.Habitat).Select(t => t.ObjectId));
            Progress.Changed += Notify;
        }

        private void Notify() { Changed?.Invoke(); }
        public bool Begin() => Progress.Begin(disaster.Progress.Stage == DisasterStage.RecoveryReady, Running);
        public bool AcceptBriefing() => Progress.AcceptBriefing(Running);
        public bool AcceptPlantingDebrief() => Progress.AcceptPlantingDebrief(Running);
        public bool AcceptEcosystemDebrief() => Progress.AcceptEcosystemDebrief(Running);

        public Transform Destination(Vector3 playerPosition)
        {
            if (!Active) return null;
            if (Progress.Stage == RecoveryStage.Planting)
            {
                // Paused navigation should still point to the unwatered sapling.
                Transform[] available = beds.Where(b => b.Planted && !b.Watered).Select(b => b.transform).ToArray();
                if (available.Length == 0)
                    available = Progress.Carried > 0 ? beds.Where(b => !b.Planted).Select(b => b.transform).ToArray()
                        : seedlings.Where(s => !s.Collected).Select(s => s.transform).ToArray();
                return Nearest(available, playerPosition) ?? ranger;
            }
            if (Progress.Stage == RecoveryStage.Ecosystem)
                return Nearest(tasks.Where(t => !t.Done).Select(t => t.transform).ToArray(), playerPosition) ?? ranger;
            return ranger;
        }

        private static Transform Nearest(Transform[] targets, Vector3 position) => targets.OrderBy(t => (t.position - position).sqrMagnitude).FirstOrDefault();
        public string DestinationLabel(Transform target)
        {
            if (target == ranger) return "Petugas Hutan";
            RecoveryPlantingPoint bed = target.GetComponent<RecoveryPlantingPoint>();
            if (bed != null) return (bed.NeedsWater ? "Siram bibit " : bed.Dug ? "Tanam bibit " : "Lahan tanam ") + (bed.Slot + 1).ToString("00");
            if (target.GetComponent<RecoverySeedling>() != null) return "Bibit / Persemaian";
            RecoveryTask task = target.GetComponent<RecoveryTask>();
            return task == null ? Title : task.Kind == RecoveryTaskKind.Waste ? "Sampah / Tepian sungai" : "Habitat bertanda";
        }

        public string Title => Progress.Stage == RecoveryStage.Planting ? "TANAM MASA DEPAN"
            : Progress.Stage == RecoveryStage.PlantingDebrief ? "BENIH HARAPAN"
            : Progress.Stage == RecoveryStage.Ecosystem ? "KEMBALIKAN KEHIDUPAN"
            : Progress.Stage >= RecoveryStage.EcosystemDebrief ? "KESEMPATAN KEDUA" : "AWAL PEMULIHAN";
        public string Instruction => Progress.Stage == RecoveryStage.Planting
            ? "Ambil bibit di persemaian. Tekan E untuk menggali dan menanam. Bidik setiap bibit, lalu tahan KLIK KIRI untuk menyiram."
            : Progress.Stage == RecoveryStage.Ecosystem ? "Kumpulkan " + Progress.WasteGoal + " sampah di kedua tepian sungai. Gunakan jembatan untuk menyeberang, lalu pulihkan habitat bertanda."
            : Progress.Stage == RecoveryStage.Complete ? "Zona Pemulihan selesai. Zona Hutan Pulih masih dalam pengembangan."
            : "Temui Petugas Hutan untuk pengarahan berikutnya. Tekan E untuk berbicara.";

        private void OnDestroy() { if (Progress != null) Progress.Changed -= Notify; }
    }
}
