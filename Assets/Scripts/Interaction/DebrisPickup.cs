using Rimbara.Mission;
using UnityEngine;

namespace Rimbara.Interaction
{
    public sealed class DebrisPickup : InteractableBehaviour
    {
        [SerializeField] private DisasterChapter chapter;
        [SerializeField] private GameObject burntVisual;
        [SerializeField] private GameObject preparedGround;
        public bool Cleared { get; private set; }
        public override bool Available => !Cleared && gameObject.activeInHierarchy;

        public void Configure(string id, DisasterChapter owner, GameObject visual, GameObject ground, GameObject ring)
        {
            SetIdentity(id, ring);
            chapter = owner;
            burntVisual = visual;
            preparedGround = ground;
        }

        public override string GetPrompt(VolunteerActor actor) => chapter.Progress.Stage == DisasterStage.CleanTheWasteland
            ? "[E]  Bersihkan puing sisa kebakaran" : "Padamkan api sebelum membersihkan area ini";

        public override InteractionResult Interact(VolunteerActor actor)
        {
            if (!Available || !InRange(actor)) return new InteractionResult(false, "Dekati puing yang ditandai.");
            if (!chapter.ClearDebris(objectId)) return new InteractionResult(false, "Padamkan api sebelum menyiapkan lahan.");
            Cleared = true;
            SetFocused(false);
            if (burntVisual != null) burntVisual.SetActive(false);
            if (preparedGround != null) preparedGround.SetActive(true);
            return new InteractionResult(true, "Awal yang bersih. Lahan ini siap untuk pemulihan.");
        }
    }
}
