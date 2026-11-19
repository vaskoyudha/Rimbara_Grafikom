using Rimbara.Mission;
using Rimbara.UI;
using UnityEngine;

namespace Rimbara.Interaction
{
    public sealed class ElderDialogue : InteractableBehaviour
    {
        [SerializeField] private RestoredChapter chapter;
        [SerializeField] private DisasterHud hud;
        [SerializeField] private GameObject beacon;
        public override bool Available => chapter.EnvironmentReady && chapter.Progress.Stage < RestoredStage.Ending;
        public void Configure(RestoredChapter owner, DisasterHud ui, GameObject marker, GameObject ring)
        { SetIdentity("restored-elder", ring); chapter = owner; hud = ui; beacon = marker; }
        private void Update() { beacon.SetActive(Available && chapter.Progress.Stage == RestoredStage.ElderBriefing); }
        public override string GetPrompt(VolunteerActor actor) => "[E]  Bicara dengan Tetua Desa";
        public override InteractionResult Interact(VolunteerActor actor)
        {
            if (!Available || !InRange(actor)) return new InteractionResult(false, "Dekati Tetua Desa setelah pemulihan selesai.");
            hud.OpenElderDialogue(); return new InteractionResult(true, "Tetua Desa / Hutan Arunika");
        }
    }
}
