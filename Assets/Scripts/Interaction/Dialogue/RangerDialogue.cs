using Rimbara.Mission;
using Rimbara.UI;
using UnityEngine;

namespace Rimbara.Interaction
{
    public sealed class RangerDialogue : InteractableBehaviour
    {
        [SerializeField] private RecoveryChapter chapter;
        [SerializeField] private DisasterHud hud;
        [SerializeField] private GameObject beacon;
        public override bool Available => chapter.Active && chapter.Progress.Stage != RecoveryStage.Complete;
        public void Configure(RecoveryChapter owner, DisasterHud ui, GameObject marker, GameObject ring)
        { SetIdentity("recovery-ranger", ring); chapter = owner; hud = ui; beacon = marker; }
        private void Update() { beacon.SetActive(Available && chapter.Progress.Stage != RecoveryStage.Planting && chapter.Progress.Stage != RecoveryStage.Ecosystem); }
        public override string GetPrompt(VolunteerActor actor) => "[E]  Bicara dengan Petugas Hutan";
        public override InteractionResult Interact(VolunteerActor actor)
        {
            if (!Available || !InRange(actor)) return new InteractionResult(false, "Dekati Petugas Hutan setelah menyelesaikan Zona Bencana.");
            hud.OpenRangerDialogue(transform);
            return new InteractionResult(true, "Petugas Hutan / Pemulihan Arunika");
        }
    }
}
