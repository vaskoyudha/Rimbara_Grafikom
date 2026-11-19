using Rimbara.Mission;
using UnityEngine;

namespace Rimbara.Interaction
{
    public sealed class FinalPlantingPoint : InteractableBehaviour
    {
        [SerializeField] private RestoredChapter chapter;
        [SerializeField] private GameObject tree;
        [SerializeField] private GameObject wateredSoil;
        [SerializeField] private GameObject beacon;
        [SerializeField] private Collider waterTarget;
        private float contact;
        private void Awake()
        {
            if (chapter == null || tree == null || wateredSoil == null || beacon == null || waterTarget == null)
                throw new System.InvalidOperationException("Final bed requires its chapter, sapling, watered soil, beacon and water collider.");
        }
        public Vector3 AimPosition => transform.position + Vector3.up * 0.55f;
        public bool NeedsWater => chapter.CanWater;
        public float WaterFraction => chapter.Progress.Watered ? 1f : Mathf.Clamp01(contact / 2f);
        public override bool Available => chapter.EnvironmentReady && chapter.Running && chapter.Progress.Stage == RestoredStage.FinalPlanting;
        public void Configure(RestoredChapter owner, GameObject sapling, GameObject soil, GameObject marker, Collider sprayTarget, GameObject ring)
        { SetIdentity("restored-final-bed", ring); chapter = owner; tree = sapling; wateredSoil = soil; beacon = marker; waterTarget = sprayTarget; }
        private void Update()
        {
            tree.SetActive(chapter.Progress.Planted); wateredSoil.SetActive(chapter.Progress.Watered);
            waterTarget.enabled = NeedsWater;
            beacon.SetActive(Available || NeedsWater);
        }
        public override string GetPrompt(VolunteerActor actor) => "[E]  Tanam bibit terakhir";
        public override InteractionResult Interact(VolunteerActor actor)
        {
            if (!Available || !InRange(actor) || !chapter.Progress.Plant(objectId, chapter.Running))
                return new InteractionResult(false, "Temui Tetua dan dekati lahan tanam terakhir.");
            SetFocused(false); return new InteractionResult(true, "Bibit terakhir ditanam. Siram sebagai janji untuk menjaga hutan.");
        }
        public bool ReceiveWater(float seconds)
        {
            if (!NeedsWater || seconds <= 0f || float.IsNaN(seconds) || float.IsInfinity(seconds)) return false;
            contact = Mathf.Min(2f, contact + seconds);
            return contact >= 2f && chapter.Progress.Water(objectId, chapter.Running);
        }
    }
}
