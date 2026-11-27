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
        [SerializeField] private float hoverScale = 1.06f;
        [SerializeField] private float pressScale = 0.97f;
        [SerializeField] private float speed = 16f;

        private RectTransform rect;
        private float target = 1f;
        private float current = 1f;

        private void Awake() { rect = (RectTransform)transform; }

        public void OnPointerEnter(PointerEventData eventData) { target = hoverScale; }
        public void OnPointerExit(PointerEventData eventData) { target = 1f; }
        public void OnPointerDown(PointerEventData eventData) { target = pressScale; }
        public void OnPointerUp(PointerEventData eventData) { target = hoverScale; }

        private void OnDisable()
        {
            target = 1f;
            current = 1f;
            if (rect != null) rect.localScale = Vector3.one;
        }

        private void Update()
        {
            if (rect == null) return;
            current = Mathf.Lerp(current, target, 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
            rect.localScale = new Vector3(current, current, 1f);
        }
    }
}
