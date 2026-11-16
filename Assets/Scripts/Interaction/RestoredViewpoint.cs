using Rimbara.Mission;
using UnityEngine;

namespace Rimbara.Interaction
{
    public sealed class RestoredViewpoint : InteractableBehaviour
    {
        [SerializeField] private RestoredChapter chapter;
        [SerializeField] private GameObject beacon;
        public override bool Available => chapter.EnvironmentReady && chapter.Progress.Stage < RestoredStage.Ending;
        public void Configure(RestoredChapter owner, GameObject marker, GameObject ring)
        { SetIdentity("restored-viewpoint", ring); chapter = owner; beacon = marker; }
        private void Update() { beacon.SetActive(Available && chapter.Progress.Stage == RestoredStage.Viewpoint); }
        public override string GetPrompt(VolunteerActor actor) => chapter.Progress.Stage == RestoredStage.Viewpoint
            ? "[E]  Saksikan hutan pulih" : "Tanam dan siram bibit terakhir terlebih dahulu";
        public override InteractionResult Interact(VolunteerActor actor)
        {
            if (!Available || !InRange(actor) || !chapter.VisitViewpoint(objectId))
                return new InteractionResult(false, "Tanam dan siram bibit terakhir, lalu dekati titik pandang.");
            SetFocused(false); return new InteractionResult(true, "Setiap Pohon Menyimpan Cerita.");
        }
    }
}
