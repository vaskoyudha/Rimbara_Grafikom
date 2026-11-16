using Rimbara.Mission;
using UnityEngine;

namespace Rimbara.Interaction
{
    public sealed class RecoverySeedling : InteractableBehaviour
    {
        [SerializeField] private RecoveryChapter chapter;
        [SerializeField] private GameObject visual;
        [SerializeField] private GameObject beacon;
        private void Awake()
        {
            if (chapter == null || visual == null || beacon == null)
                throw new System.InvalidOperationException("Recovery seedling requires its chapter, visual and beacon.");
        }
        public bool Collected => chapter.Progress != null && chapter.Progress.IsCollected(objectId);
        public override bool Available => chapter.CanPlant && !Collected;
        public void Configure(string id, RecoveryChapter owner, GameObject model, GameObject marker, GameObject ring)
        { SetIdentity(id, ring); chapter = owner; visual = model; beacon = marker; }
        private void Update()
        {
            visual.SetActive(!Collected);
            beacon.SetActive(Available);
        }
        public override string GetPrompt(VolunteerActor actor) => "[E]  Ambil bibit pohon";
        public override InteractionResult Interact(VolunteerActor actor)
        {
            if (!Available || !InRange(actor)) return new InteractionResult(false, "Ikuti pengarahan dan dekati bibit di persemaian.");
            if (!chapter.Progress.Collect(objectId, chapter.Running)) return new InteractionResult(false, "Bibit ini sudah diambil.");
            SetFocused(false);
            return new InteractionResult(true, "Bibit diambil. Siapkan lahan tanam, lalu tanam dan siram bibit.");
        }
    }
}
