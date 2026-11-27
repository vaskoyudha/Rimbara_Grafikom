using Rimbara.Mission;
using UnityEngine;

namespace Rimbara.Interaction
{
    public sealed class EquipmentPickup : InteractableBehaviour
    {
        [SerializeField] private DisasterChapter chapter;
        [SerializeField] private GameObject pickupVisual;
        [SerializeField] private GameObject beacon;
        public override bool Available => chapter.Progress != null && chapter.Progress.Stage == DisasterStage.Equipment;

        private void Start()
        {
            if (beacon == null)
            {
                Transform marker = transform.Find("Beacon - COLLECT EQUIPMENT");
                if (marker != null) beacon = marker.gameObject;
            }
        }

        public void Configure(DisasterChapter owner, GameObject visual, GameObject ring, GameObject marker = null)
        {
            SetIdentity("disaster-equipment", ring);
            chapter = owner;
            pickupVisual = visual;
            beacon = marker;
        }
        public override string GetPrompt(VolunteerActor actor)
        {
            return Available
                ? "[E]  Ambil perlengkapan pemadaman"
                : "Temui koordinator sebelum mengambil perlengkapan.";
        }
        public override InteractionResult Interact(VolunteerActor actor)
        {
            if (!Available || !InRange(actor))
                return new InteractionResult(false, "Ikuti pengarahan, lalu dekati alat.");

            if (!actor.MulaiAmbilAlat(this))
                return new InteractionResult(false, "Sedang mengambil alat.");

            return new InteractionResult(true, "Mengambil perlengkapan pemadaman…");
        }

        public InteractionResult SelesaikanPengambilan(VolunteerActor actor)
        {
            if (!Available || !InRange(actor) || !chapter.CollectEquipment())
                return new InteractionResult(false, "Dekati alat untuk mengambilnya kembali.");

            SetFocused(false);
            if (pickupVisual != null) pickupVisual.SetActive(false);
            if (beacon != null) beacon.SetActive(false);

            return new InteractionResult(true, "Perlengkapan siap. Dekati api bertanda.");
        }

        /// <summary>Hides pickup prop and beacon when equipment is already collected in a saved run.</summary>
        public void SyncFromProgress()
        {
            if (chapter == null || chapter.Progress == null || !chapter.Progress.HasEquipment) return;
            SetFocused(false);
            if (pickupVisual != null) pickupVisual.SetActive(false);
            if (beacon != null) beacon.SetActive(false);
        }
    }
}
