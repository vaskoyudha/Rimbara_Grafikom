using Rimbara.Mission;
using Rimbara.UI;
using UnityEngine;

namespace Rimbara.Interaction
{
    public sealed class CoordinatorDialogue : InteractableBehaviour
    {
        [SerializeField] private DisasterChapter chapter;
        [SerializeField] private DisasterHud hud;
        public override bool Available => true;

        public void Configure(DisasterChapter owner, DisasterHud view, GameObject ring)
        {
            SetIdentity("disaster-coordinator", ring);
            chapter = owner;
            hud = view;
        }

        public override string GetPrompt(VolunteerActor actor) => "[E]  Bicara dengan koordinator misi";
        public override InteractionResult Interact(VolunteerActor actor)
        {
            if (!InRange(actor)) return new InteractionResult(false, "Dekati koordinator.");
            hud.OpenCoordinatorDialogue();
            return new InteractionResult(true, "Posko Misi Darurat");
        }
    }
}
