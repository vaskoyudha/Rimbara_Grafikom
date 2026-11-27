using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rimbara.UI
{
    /// <summary>
    /// Adds a small, frame-rate independent scale animation to a button so
    /// hover and press read clearly even while the game is paused.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public sealed class ButtonHoverAnimation : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float hoverScale = 1.045f;
        [SerializeField] private float pressScale = 0.97f;
        [SerializeField] private float speed = 16f;
        [SerializeField] private float maxHorizontalExpansionPx = 12f;

        private RectTransform rect;
        private float target = 1f;
        private float current = 1f;

        private void Awake()
        {
            rect = (RectTransform)transform;
            EnsureCenteredPivot();
        }

        private void OnEnable()
        {
            if (rect == null) rect = (RectTransform)transform;
            EnsureCenteredPivot();
        }

        public void OnPointerEnter(PointerEventData eventData) { target = ResolveHoverScale(); }
        public void OnPointerExit(PointerEventData eventData) { target = 1f; }
        public void OnPointerDown(PointerEventData eventData) { target = pressScale; }
        public void OnPointerUp(PointerEventData eventData) { target = ResolveHoverScale(); }

        private float ResolveHoverScale()
        {
            if (rect == null) return hoverScale;
            float width = rect.rect.width > 1f ? rect.rect.width : rect.sizeDelta.x;
            if (width <= 1f) return hoverScale;
            return Mathf.Min(hoverScale, 1f + maxHorizontalExpansionPx / width);
        }

        private void EnsureCenteredPivot()
        {
            if (rect == null) return;
            Vector2 center = new Vector2(0.5f, 0.5f);
            if ((rect.pivot - center).sqrMagnitude < 0.0001f) return;
            Vector2 size = rect.rect.size;
            if (size.x <= 0f || size.y <= 0f) size = rect.sizeDelta;
            if (size.x <= 0f || size.y <= 0f) return;
            Vector2 delta = center - rect.pivot;
            Vector2 scale = rect.localScale;
            rect.pivot = center;
            rect.anchoredPosition += new Vector2(delta.x * size.x * scale.x, delta.y * size.y * scale.y);
        }

        private void OnDisable()
        {
            target = 1f;
            current = 1f;
            if (rect != null) rect.localScale = Vector3.one;
        }

        private void Update()
        {
            if (rect == null) return;
            EnsureCenteredPivot();
            current = Mathf.Lerp(current, target, 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
            rect.localScale = new Vector3(current, current, 1f);
        }
    }
}
