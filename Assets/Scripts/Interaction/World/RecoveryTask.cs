using Rimbara.Mission;
using UnityEngine;

namespace Rimbara.Interaction
{
    public enum RecoveryTaskKind { Waste, Habitat }
    public sealed class RecoveryTask : InteractableBehaviour
    {
        [SerializeField] private RecoveryChapter chapter;
        [SerializeField] private RecoveryTaskKind kind;
        [SerializeField] private GameObject damaged;
        [SerializeField] private GameObject restored;
        [SerializeField] private GameObject beacon;
        private void Awake()
        {
            if (chapter == null || damaged == null || beacon == null || (kind == RecoveryTaskKind.Habitat && restored == null))
                throw new System.InvalidOperationException("Recovery task requires its chapter and before/after visuals.");
        }
        public RecoveryTaskKind Kind => kind;
        public bool Done => chapter.Progress != null && (kind == RecoveryTaskKind.Waste
            ? chapter.Progress.IsWasteCleaned(objectId) : chapter.Progress.IsHabitatRestored(objectId));
        public override bool Available => chapter.CanRestore && !Done;
        public void Configure(string id, RecoveryChapter owner, RecoveryTaskKind type, GameObject before, GameObject after, GameObject marker, GameObject ring)
        { SetIdentity(id, ring); chapter = owner; kind = type; damaged = before; restored = after; beacon = marker; }
        private void Update()
        {
            damaged.SetActive(!Done);
            if (restored != null) restored.SetActive(Done);
            beacon.SetActive(Available);
        }
        public override string GetPrompt(VolunteerActor actor) => kind == RecoveryTaskKind.Waste ? "[E]  Ambil sampah tepian sungai" : "[E]  Pulihkan tempat berlindung satwa";
        public override InteractionResult Interact(VolunteerActor actor)
        {
            if (!Available || !InRange(actor)) return new InteractionResult(false, "Ikuti misi Kembalikan Kehidupan dan dekati target bertanda.");
            bool success = kind == RecoveryTaskKind.Waste ? chapter.Progress.CleanWaste(objectId, chapter.Running) : chapter.Progress.RestoreHabitat(objectId, chapter.Running);
            if (!success) return new InteractionResult(false, "Target ini sudah dipulihkan.");
            SetFocused(false);
            return new InteractionResult(true, kind == RecoveryTaskKind.Waste ? "Sampah sungai dikumpulkan. Air dan tepian mulai pulih." : "Ranting disusun dan tanaman penutup dipulihkan sebagai tempat berlindung satwa kecil.");
        }
    }
}
