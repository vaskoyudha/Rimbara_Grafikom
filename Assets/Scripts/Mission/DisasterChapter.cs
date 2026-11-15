using System;
using System.Linq;
using Rimbara.Interaction;
using UnityEngine;

namespace Rimbara.Mission
{
    [DefaultExecutionOrder(-200)]
    public sealed class DisasterChapter : MonoBehaviour
    {
        [SerializeField] private GameState state;
        [SerializeField] private Transform coordinator;
        [SerializeField] private Transform equipment;
        [SerializeField] private DebrisPickup[] debris;
        [SerializeField] private FirePoint[] fires;
        [SerializeField] private Renderer[] equipmentRenderers;
        public DisasterProgress Progress { get; private set; }
        public GameState State => state;
        public Transform Coordinator => coordinator;
        public event Action Changed;
        public bool CanExtinguish => state.SessionRunning && Progress != null && Progress.Stage == DisasterStage.StopTheFlames;

        public void Configure(GameState gameState, Transform post, Transform kit, DebrisPickup[] piles, FirePoint[] firePoints)
        {
            state = gameState;
            coordinator = post;
            equipment = kit;
            debris = piles;
            fires = firePoints;
        }

        public void ConfigureEquipmentVisuals(Renderer[] renderers) { equipmentRenderers = renderers; }

        private void Awake()
        {
            if (state == null || coordinator == null || equipment == null || debris == null || debris.Length == 0 || fires == null)
                throw new InvalidOperationException("Disaster chapter requires its authored post, equipment, debris and fires.");
            Progress = new DisasterProgress(state.Profile.Fires, debris.Length);
            Progress.Changed += Notify;
            state.FireExtinguished += RecordFire;
            UpdateEquipmentVisuals();
            if (GetComponent<DisasterAtmosphere>() == null)
            {
                Light sun = FindObjectsByType<Light>(FindObjectsSortMode.None).FirstOrDefault(l => l.type == LightType.Directional);
                gameObject.AddComponent<DisasterAtmosphere>().Configure(this, sun);
            }
        }

        private void RecordFire(string id)
        {
            if (!Progress.Extinguish(id, state.SessionRunning))
                throw new InvalidOperationException("Fire progress did not match the active Disaster chapter: " + id);
        }

        public bool ClearDebris(string id) => Progress.ClearDebris(id, state.SessionRunning);
        public bool AcceptBriefing() => Progress.AcceptBriefing(state.SessionRunning);
        public bool CollectEquipment() => Progress.CollectEquipment(state.SessionRunning);
        private void Notify() { UpdateEquipmentVisuals(); Changed?.Invoke(); }

        private void UpdateEquipmentVisuals()
        {
            if (equipmentRenderers == null) return;
            foreach (Renderer renderer in equipmentRenderers)
                if (renderer != null) renderer.enabled = Progress.HasEquipment;
        }

        public Transform Destination(Vector3 playerPosition)
        {
            switch (Progress.Stage)
            {
                case DisasterStage.Briefing: return coordinator;
                case DisasterStage.Equipment: return equipment;
                case DisasterStage.StopTheFlames:
                    return fires.Where(f => f.gameObject.activeInHierarchy && !f.Extinguished)
                        .OrderBy(f => (f.transform.position - playerPosition).sqrMagnitude).Select(f => f.transform).FirstOrDefault();
                case DisasterStage.CleanTheWasteland:
                    return debris.Where(d => d.Available).OrderBy(d => (d.transform.position - playerPosition).sqrMagnitude)
                        .Select(d => d.transform).FirstOrDefault();
                default: return coordinator;
            }
        }

        public string DestinationLabel(Transform target)
        {
            if (target == coordinator) return "Koordinator / Posko darurat";
            if (target == equipment) return "Perlengkapan pemadaman";
            FirePoint fire = target.GetComponent<FirePoint>();
            if (fire != null) return "Titik api " + (fire.Slot + 1).ToString("00");
            int pile = Array.FindIndex(debris, d => d.transform == target);
            return pile >= 0 ? "Puing bertanda " + (pile + 1).ToString("00") : Title;
        }

        public string Title => Progress == null || Progress.Stage == DisasterStage.Briefing ? "KEDATANGAN" : Progress.Stage == DisasterStage.CleanTheWasteland
            ? "BERSIHKAN LAHAN" : Progress.Stage == DisasterStage.RecoveryReady ? "AWAL YANG BERSIH" : "PADAMKAN API";
        public string Instruction
        {
            get
            {
                if (Progress == null) return string.Empty;
                switch (Progress.Stage)
                {
                    case DisasterStage.Briefing: return "Datangi posko darurat. Tekan E untuk berbicara dengan koordinator misi.";
                    case DisasterStage.Equipment: return "Ambil perlengkapan pemadaman di samping posko. Tekan E.";
                    case DisasterStage.StopTheFlames: return "Bidik api yang ditandai dan tahan KLIK KIRI. Api padam: " + Progress.Fires + " / " + Progress.FireGoal + ".";
                    case DisasterStage.CleanTheWasteland: return "Tekan E di dekat puing bertanda untuk menyiapkan lahan. Dibersihkan: " + Progress.Debris + " / " + Progress.DebrisGoal + ".";
                    default: return "Kembali ke koordinator. Lahan siap dipulihkan; bab selanjutnya masih dalam pengembangan.";
                }
            }
        }

        private void OnDestroy()
        {
            if (Progress != null) Progress.Changed -= Notify;
            if (state != null) state.FireExtinguished -= RecordFire;
        }
    }
}
