using Rimbara.Player;
using UnityEngine;

namespace Rimbara.Interaction
{
    [RequireComponent(typeof(VolunteerActor))]
    public sealed class InteractionDetector : MonoBehaviour
    {
        [SerializeField] private VolunteerInput input;
        [SerializeField] private LayerMask interactionLayers;
        [SerializeField] private LayerMask obstructionLayers;
        [SerializeField] private Camera viewCamera;
        [SerializeField, Range(-1f, 1f)] private float facingThreshold = 0.35f;
        private readonly Collider[] hits = new Collider[40];
        private VolunteerActor actor;
        public InteractableBehaviour Focused { get; private set; }

        public string Prompt
        {
            get
            {
                if (Focused == null) return string.Empty;
                string prompt = Focused.GetPrompt(actor);
                return string.IsNullOrWhiteSpace(prompt) ? "[E]  Berinteraksi" : prompt;
            }
        }

        public void Configure(VolunteerInput controls, LayerMask interactions, LayerMask obstructions)
        {
            input = controls;
            interactionLayers = interactions;
            obstructionLayers = obstructions;
        }

        public void ConfigureViewCamera(Camera camera) { viewCamera = camera; }

        private void Awake()
        {
            actor = GetComponent<VolunteerActor>();
            if (viewCamera == null)
            {
                VolunteerRig rig = GetComponent<VolunteerRig>();
                if (rig != null) viewCamera = rig.ViewCamera;
            }
        }

        private void Update()
        {
            if (!actor.State.SessionRunning) { SetFocus(null); return; }
            Scan();
            if (Focused != null)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 5f);
                Focused.SetFocusHighlight(pulse);
            }
            if (input.InteractPressed) InteractFocused();
        }

        public void Scan()
        {
            float range = actor.State.Profile.InteractionRange;
            Vector3 origin = transform.position + Vector3.up * 0.8f;
            Vector3 forward = viewCamera != null ? viewCamera.transform.forward : transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) { forward = transform.forward; forward.y = 0f; }
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
            forward.Normalize();
            Vector3 bodyForward = transform.forward;
            bodyForward.y = 0f;
            if (bodyForward.sqrMagnitude < 0.0001f) bodyForward = forward;
            else bodyForward.Normalize();

            int count = Physics.OverlapSphereNonAlloc(origin, range, hits, interactionLayers, QueryTriggerInteraction.Collide);
            float bestDistance = float.MaxValue;
            InteractableBehaviour best = null;
            for (int i = 0; i < count; i++)
            {
                InteractableBehaviour candidate = hits[i].GetComponentInParent<InteractableBehaviour>();
                if (candidate == null || !candidate.Available) continue;
                Vector3 difference = candidate.transform.position - transform.position;
                difference.y = 0f;
                float distance = difference.sqrMagnitude;
                if (distance > range * range || distance >= bestDistance) continue;
                Vector3 toCandidate = candidate.FocusPosition - origin;
                toCandidate.y = 0f;
                if (toCandidate.sqrMagnitude > 0.0001f)
                {
                    Vector3 direction = toCandidate.normalized;
                    if (Vector3.Dot(forward, direction) < facingThreshold &&
                        Vector3.Dot(bodyForward, direction) < facingThreshold) continue;
                }
                if (Physics.Linecast(origin, candidate.FocusPosition, obstructionLayers, QueryTriggerInteraction.Ignore)) continue;
                best = candidate;
                bestDistance = distance;
            }
            SetFocus(best);
        }

        public InteractionResult InteractFocused()
        {
            InteractionResult result = Focused == null
                ? new InteractionResult(false, "Dekati dan hadapi objek bertanda untuk berinteraksi.")
                : Focused.Interact(actor);
            actor.Report(result);
            if (Focused != null && !Focused.Available) SetFocus(null);
            return result;
        }

        private void SetFocus(InteractableBehaviour value)
        {
            if (Focused == value) return;
            if (Focused != null) Focused.SetFocused(false);
            Focused = value;
            if (Focused != null) Focused.SetFocused(true);
        }

        private void OnDisable() { SetFocus(null); }
    }
}
