using UnityEngine;

namespace Rimbara.Interaction
{
    public abstract class InteractableBehaviour : MonoBehaviour, IInteractable
    {
        [SerializeField] protected string objectId;
        [SerializeField] private GameObject focusRing;
        private Vector3 focusRingBaseScale = Vector3.one;
        private bool focusRingScaleCaptured;
        public string ObjectId => objectId;
        public virtual Vector3 FocusPosition => transform.position + Vector3.up * 0.55f;
        public abstract bool Available { get; }
        public abstract string GetPrompt(VolunteerActor actor);
        public abstract InteractionResult Interact(VolunteerActor actor);

        public void SetIdentity(string id, GameObject ring)
        {
            objectId = id;
            focusRing = ring;
            if (focusRing == null) return;
            focusRingBaseScale = focusRing.transform.localScale;
            focusRingScaleCaptured = true;
            focusRing.SetActive(false);
        }

        public void SetFocused(bool focused)
        {
            if (focusRing == null) return;
            CaptureFocusRingScale();
            focusRing.SetActive(focused && Available);
            if (!focused) focusRing.transform.localScale = focusRingBaseScale;
        }

        public void SetFocusHighlight(float pulse)
        {
            if (focusRing == null) return;
            CaptureFocusRingScale();
            float scale = 1f + Mathf.Clamp01(pulse) * 0.12f;
            focusRing.transform.localScale = new Vector3(
                focusRingBaseScale.x * scale,
                focusRingBaseScale.y,
                focusRingBaseScale.z * scale);
        }

        private void CaptureFocusRingScale()
        {
            if (focusRingScaleCaptured) return;
            focusRingBaseScale = focusRing.transform.localScale;
            focusRingScaleCaptured = true;
        }

        protected bool InRange(VolunteerActor actor)
        {
            if (actor == null || actor.State == null || !actor.State.SessionRunning) return false;
            Vector3 difference = actor.transform.position - transform.position;
            difference.y = 0f;
            float range = actor.State.Profile.InteractionRange;
            return difference.sqrMagnitude <= range * range;
        }
    }
}
