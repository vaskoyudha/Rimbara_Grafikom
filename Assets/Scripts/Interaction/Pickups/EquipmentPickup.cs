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

        public override string GetPrompt(VolunteerActor actor) => chapter.Progress.Stage == DisasterStage.Equipment
            ? "[E]  Ambil perlengkapan pemadaman" : "Temui koordinator sebelum mengambil perlengkapan";

        public override InteractionResult Interact(VolunteerActor actor)
        {
            if (!InRange(actor) || !chapter.CollectEquipment())
                return new InteractionResult(false, "Baca pengarahan darurat, lalu ambil perlengkapan.");
            SetFocused(false);
            if (pickupVisual != null) pickupVisual.SetActive(false);
            if (beacon != null) beacon.SetActive(false);
            return new InteractionResult(true, "Perlengkapan siap. Dekati api bertanda, bidik, lalu tahan KLIK KIRI.");
        }
    }
}
