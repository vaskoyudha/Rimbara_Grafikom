using Rimbara.Mission;
using Rimbara.Player;
using UnityEngine;

namespace Rimbara.UI
{
    /// <summary>
    /// Tracks an NPC in screen space and renders a clean 2D nameplate pill,
    /// hiding the legacy floating 3D cube/world-text beacon while preserving its active gate.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public sealed class NpcNameplate2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private GameObject gate;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.08f, 0f);
        [SerializeField] private float fadeStartDistance = 18f;
        [SerializeField] private float maxVisibleDistance = 26f;

        private RectTransform rect;
        private RectTransform parentRect;
        private CanvasGroup group;
        private DisasterHud hud;
        private GameFlow flow;
        private Camera viewCamera;

        public void Configure(Transform npcTarget, GameObject beaconGate)
        {
            target = npcTarget;
            gate = beaconGate;
            HideLegacyWorldVisuals();
        }

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            parentRect = transform.parent as RectTransform;
            group = GetComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            HideLegacyWorldVisuals();
        }

        private void HideLegacyWorldVisuals()
        {
            if (gate == null) return;
            foreach (Renderer r in gate.GetComponentsInChildren<Renderer>(true))
                r.enabled = false;
        }

        private void LateUpdate()
        {
            if (flow == null) flow = FindFirstObjectByType<GameFlow>();
            if (hud == null) hud = FindFirstObjectByType<DisasterHud>();
            if (viewCamera == null)
            {
                VolunteerRig rig = FindFirstObjectByType<VolunteerRig>();
                viewCamera = rig != null && rig.ViewCamera != null ? rig.ViewCamera : Camera.main;
            }

            if (target == null || viewCamera == null || parentRect == null
                || (gate != null && !gate.activeInHierarchy)
                || (flow != null && flow.Mode != ScreenMode.Playing)
                || (hud != null && hud.DialogueOpen))
            {
                group.alpha = 0f;
                return;
            }

            Vector3 worldPos = target.position + worldOffset;
            float distance = Vector3.Distance(viewCamera.transform.position, worldPos);
            if (distance > maxVisibleDistance)
            {
                group.alpha = 0f;
                return;
            }

            Vector3 screenPos = viewCamera.WorldToScreenPoint(worldPos);
            if (screenPos.z <= 0.1f)
            {
                group.alpha = 0f;
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, null, out Vector2 local))
            {
                group.alpha = 0f;
                return;
            }

            float floatY = Mathf.Sin(Time.unscaledTime * 2.2f) * 2.5f;
            rect.anchoredPosition = local + new Vector2(0f, floatY);
            float fade = distance <= fadeStartDistance
                ? 1f
                : 1f - Mathf.Clamp01((distance - fadeStartDistance) / Mathf.Max(0.1f, maxVisibleDistance - fadeStartDistance));
            group.alpha = fade;
        }
    }
}
