using Rimbara.Player;
using UnityEngine;

namespace Rimbara.Interaction
{
    public sealed class WaterSprayer : MonoBehaviour
    {
        [SerializeField] private VolunteerInput input;
        [SerializeField] private VolunteerActor actor;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Transform nozzle;
        [SerializeField] private ParticleSystem spray;
        [SerializeField] private LayerMask hitLayers;
        [SerializeField] private LayerMask obstructionLayers;
        public FirePoint AimedFire { get; private set; }
        public RecoveryPlantingPoint AimedSeedling { get; private set; }
        public FinalPlantingPoint AimedFinalTree { get; private set; }
        public bool Spraying { get; private set; }
        public string AimHint { get; private set; }
        public Vector3 LastAimPoint { get; private set; }

        public void Configure(VolunteerInput controls, VolunteerActor volunteer, Camera camera,
            Transform sprayNozzle, ParticleSystem particles, LayerMask hits, LayerMask obstructions)
        {
            input = controls;
            actor = volunteer;
            viewCamera = camera;
            nozzle = sprayNozzle;
            spray = particles;
            hitLayers = hits;
            obstructionLayers = obstructions;
        }

        private void Update()
        {
            if (!actor.State.SessionRunning) { SetSpraying(false); AimedFire = null; AimedSeedling = null; AimedFinalTree = null; AimHint = string.Empty; return; }
            UpdateAim();
            bool recoveryTool = actor.Missions.DisasterChapter != null && actor.Missions.DisasterChapter.Recovery != null
                && actor.Missions.DisasterChapter.Recovery.CanPlant;
            bool useTool = input.SprayHeld && (actor.Missions.CanExtinguish || recoveryTool || (AimedFinalTree != null && AimedFinalTree.NeedsWater));
            SetSpraying(useTool);
            if (useTool) ApplySpray(Time.deltaTime);
        }

        public void UpdateAim()
        {
            Ray ray = viewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;
            bool didHit = Physics.Raycast(ray, out hit, 100f, hitLayers, QueryTriggerInteraction.Collide);
            LastAimPoint = didHit ? hit.point : ray.GetPoint(30f);
            if (nozzle != null) nozzle.rotation = Quaternion.LookRotation((LastAimPoint - nozzle.position).normalized);
            AimedFire = didHit && hit.collider is SphereCollider && hit.collider.isTrigger
                ? hit.collider.GetComponent<FirePoint>()
                : null;
            AimedSeedling = didHit ? hit.collider.GetComponentInParent<RecoveryPlantingPoint>() : null;
            AimedFinalTree = didHit ? hit.collider.GetComponentInParent<FinalPlantingPoint>() : null;
            AimHint = string.Empty;
            if (AimedFinalTree != null)
            {
                if (!AimedFinalTree.NeedsWater) return;
                if (Vector3.Distance(actor.transform.position + Vector3.up, AimedFinalTree.AimPosition) > actor.State.Profile.SprayRange)
                { AimHint = "Dekati bibit terakhir untuk menyiram"; return; }
                if (Physics.Linecast(nozzle.position, AimedFinalTree.AimPosition, obstructionLayers, QueryTriggerInteraction.Ignore))
                { AimHint = "Aliran air terhalang / Cari posisi lain"; return; }
                AimHint = "TAHAN KLIK KIRI  |  Siram bibit terakhir"; return;
            }
            if (AimedSeedling != null)
            {
                if (!AimedSeedling.NeedsWater) return;
                if (Vector3.Distance(actor.transform.position + Vector3.up, AimedSeedling.AimPosition) > actor.State.Profile.SprayRange)
                { AimHint = "Dekati bibit untuk menyiram"; return; }
                if (Physics.Linecast(nozzle.position, AimedSeedling.AimPosition, obstructionLayers, QueryTriggerInteraction.Ignore))
                { AimHint = "Aliran air terhalang / Cari posisi lain"; return; }
                AimHint = Rimbara.UI.IndonesianText.SeedlingWaterAction;
                return;
            }
            if (AimedFire == null || AimedFire.Extinguished) return;
            if (!actor.Missions.CanExtinguish)
            {
                AimHint = actor.Missions.DisasterChapter != null ? "Baca pengarahan darurat dan ambil perlengkapan pemadaman" : "Selesaikan Benih Harapan sebelum menyemprot air";
                return;
            }
            float distance = Vector3.Distance(actor.transform.position + Vector3.up, LastAimPoint);
            if (distance > actor.State.Profile.SprayRange) { AimHint = "Dekati api  |  jangkauan " + actor.State.Profile.SprayRange.ToString("0") + " m"; return; }
            if (Physics.Linecast(nozzle.position, LastAimPoint, obstructionLayers, QueryTriggerInteraction.Ignore))
            { AimHint = "Aliran air terhalang  |  cari posisi lain"; return; }
            AimHint = Rimbara.UI.IndonesianText.WaterAction;
        }

        public bool ApplySpray(float deltaTime)
        {
            if (AimedFinalTree != null)
            {
                if (!actor.State.SessionRunning || !AimedFinalTree.NeedsWater
                    || Vector3.Distance(actor.transform.position + Vector3.up, AimedFinalTree.AimPosition) > actor.State.Profile.SprayRange
                    || Physics.Linecast(nozzle.position, AimedFinalTree.AimPosition, obstructionLayers, QueryTriggerInteraction.Ignore)) return false;
                bool done = AimedFinalTree.ReceiveWater(deltaTime);
                if (done) actor.Report(new InteractionResult(true, "Bibit terakhir telah disiram. Datangi titik pandang hutan."));
                return done;
            }
            if (AimedSeedling != null)
            {
                if (!actor.State.SessionRunning || !AimedSeedling.NeedsWater
                    || Vector3.Distance(actor.transform.position + Vector3.up, AimedSeedling.AimPosition) > actor.State.Profile.SprayRange
                    || Physics.Linecast(nozzle.position, AimedSeedling.AimPosition, obstructionLayers, QueryTriggerInteraction.Ignore)) return false;
                bool watered = AimedSeedling.ReceiveWater(deltaTime);
                if (watered) actor.Report(new InteractionResult(true, "Bibit telah disiram. Beri waktu bagi akar muda untuk tumbuh."));
                return watered;
            }
            if (!actor.State.SessionRunning || !actor.Missions.CanExtinguish || AimedFire == null || AimedFire.Extinguished) return false;
            if (Vector3.Distance(actor.transform.position + Vector3.up, LastAimPoint) > actor.State.Profile.SprayRange) return false;
            if (Physics.Linecast(nozzle.position, LastAimPoint, obstructionLayers, QueryTriggerInteraction.Ignore)) return false;
            bool extinguished = AimedFire.ReceiveWater(deltaTime);
            if (extinguished) actor.Report(new InteractionResult(true, "Api telah padam. Arunika dapat bernapas kembali."));
            return extinguished;
        }

        private void SetSpraying(bool value)
        {
            Spraying = value;
            if (spray == null) return;
            if (value && !spray.isPlaying) spray.Play();
            else if (!value && spray.isPlaying) spray.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void OnDisable() { SetSpraying(false); }
    }
}
