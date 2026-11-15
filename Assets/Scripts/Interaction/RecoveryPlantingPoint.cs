using Rimbara.Mission;
using UnityEngine;

namespace Rimbara.Interaction
{
    public sealed class RecoveryPlantingPoint : InteractableBehaviour
    {
        [SerializeField] private RecoveryChapter chapter;
        [SerializeField] private GameObject preparedSoil;
        [SerializeField] private GameObject youngTree;
        [SerializeField] private GameObject tendedSoil;
        [SerializeField] private GameObject beacon;
        [SerializeField] private int slot;
        [SerializeField] private float waterSeconds = 2f;
        private float contact;
        private Collider waterTarget;
        public int Slot => slot;
        public bool Dug => chapter.Progress != null && chapter.Progress.IsDug(objectId);
        public bool Planted => chapter.Progress != null && chapter.Progress.IsPlanted(objectId);
        public bool Watered => chapter.Progress != null && chapter.Progress.IsWatered(objectId);
        public bool NeedsWater => chapter.CanPlant && Planted && !Watered;
        public float WaterFraction => Watered ? 1f : Mathf.Clamp01(contact / waterSeconds);
        public Vector3 AimPosition => transform.position + Vector3.up * 0.55f;
        public override Vector3 FocusPosition => transform.position + Vector3.up * 0.35f;
        public override bool Available => chapter.CanPlant && !Watered;
        public void Configure(string id, RecoveryChapter owner, int index, GameObject soil, GameObject tree, GameObject wateredSoil, GameObject marker, GameObject ring)
        { SetIdentity(id, ring); chapter = owner; slot = index; preparedSoil = soil; youngTree = tree; tendedSoil = wateredSoil; beacon = marker; }
        private void Awake()
        {
            if (chapter == null || preparedSoil == null || youngTree == null || tendedSoil == null || beacon == null
                || waterSeconds <= 0f || float.IsNaN(waterSeconds) || float.IsInfinity(waterSeconds) || transform.Find("Sapling water target") == null)
                throw new System.InvalidOperationException("Recovery bed requires valid visuals, water target and positive watering duration.");
            waterTarget = transform.Find("Sapling water target").GetComponent<Collider>();
        }
        private void Update()
        {
            if (chapter.Restored != null && chapter.Restored.WorldApplied) return;
            preparedSoil.SetActive(Dug && !Watered);
            youngTree.SetActive(Planted);
            tendedSoil.SetActive(Watered);
            beacon.SetActive(Available);
            waterTarget.enabled = NeedsWater;
            // Replay reloads the scene; explicit model reset also clears partial watering.
            if (!Planted) contact = 0f;
        }
        public override string GetPrompt(VolunteerActor actor) => !Dug ? "[E]  Gali lubang tanam"
            : !Planted ? chapter.Progress.Carried > 0 ? "[E]  Tanam bibit" : "Ambil bibit di persemaian"
            : "Bidik bibit / TAHAN KLIK KIRI untuk menyiram";
        public override InteractionResult Interact(VolunteerActor actor)
        {
            if (!Available || !InRange(actor)) return new InteractionResult(false, "Dekati lahan tanam pada misi Tanam Masa Depan.");
            if (!Dug && chapter.Progress.Dig(objectId, chapter.Running))
                return new InteractionResult(true, "Lubang tanam siap. Tekan E lagi untuk menanam bibit yang dibawa.");
            if (!Planted && chapter.Progress.Plant(objectId, chapter.Running))
                return new InteractionResult(true, "Bibit ditanam. Bidik bibit dan tahan KLIK KIRI untuk menyiramnya.");
            return new InteractionResult(false, Planted ? "Gunakan semprotan air untuk merawat bibit ini." : "Kamu belum membawa bibit. Datangi persemaian.");
        }
        public bool ReceiveWater(float seconds)
        {
            if (!NeedsWater || seconds <= 0f || float.IsNaN(seconds) || float.IsInfinity(seconds)) return false;
            contact = Mathf.Min(waterSeconds, contact + seconds);
            return contact >= waterSeconds && chapter.Progress.Water(objectId, chapter.Running);
        }
    }
}
