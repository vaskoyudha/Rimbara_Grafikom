using Rimbara.Audio;
using Rimbara.Mission;
using UnityEngine;

namespace Rimbara.Interaction
{
    public sealed class DebrisPickup : InteractableBehaviour
    {
        [SerializeField] private DisasterChapter chapter;
        [SerializeField] private GameObject burntVisual;
        [SerializeField] private GameObject preparedGround;
        [SerializeField] private GameObject clearDust;
        public bool Cleared { get; private set; }
        public override bool Available => !Cleared && gameObject.activeInHierarchy;

        public void Configure(string id, DisasterChapter owner, GameObject visual, GameObject ground, GameObject ring, GameObject dust = null)
        {
            SetIdentity(id, ring);
            chapter = owner;
            burntVisual = visual;
            preparedGround = ground;
            clearDust = dust;
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
            if (clearDust != null)
            {
                clearDust.SetActive(true);
                foreach (ParticleSystem particles in clearDust.GetComponentsInChildren<ParticleSystem>()) particles.Play();
            }
            GameAudio audio = FindFirstObjectByType<GameAudio>();
            if (audio != null) audio.PlayDebrisClear();
            gameObject.SetActive(false);
            return new InteractionResult(true, "Awal yang bersih. Lahan ini siap untuk pemulihan.");
        }
    }
}
