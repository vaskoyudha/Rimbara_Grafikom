using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rimbara.UI
{
    public static class HudLayout
    {
        public const float ReferenceWidth = 1920f;
        public const float ReferenceHeight = 1080f;
        public const float Margin = 0f;
        public const float Gap = 16f;

        private static readonly Dictionary<string, Action<RectTransform>> Placements = new Dictionary<string, Action<RectTransform>>
        {
            { "Mission card", r => TopLeft(r, new Vector2(484f, 192f)) },
            { "Restoration counters", r => TopRight(r, new Vector2(428f, 248f)) },
            { "Next destination", r => TopCenter(r, new Vector2(480f, 148f), new Vector2(0f, -6f)) },
            { "Interaction prompt", r => BottomCenter(r, new Vector2(780f, 56f), new Vector2(0f, 72f)) },
            { "Feedback toast", r => BottomCenter(r, new Vector2(840f, 60f), new Vector2(0f, 140f)) },
            { "Controls reminder", r => BottomLeft(r, new Vector2(680f, 38f)) },
            { "Arunika field map", r => BottomRight(r, new Vector2(200f, 230f)) },
            { "Crosshair horizontal", r => Center(r, new Vector2(14f, 2f)) },
            { "Crosshair vertical", r => Center(r, new Vector2(2f, 14f)) },
            { "Water contact progress", r => Center(r, new Vector2(160f, 8f), new Vector2(0f, -30f)) },
            { "Main menu card", r => Center(r, new Vector2(700f, 700f)) },
            { "Volunteer briefing", r => Center(r, new Vector2(860f, 680f)) },
            { "How to play card", r => Center(r, new Vector2(800f, 680f)) },
            { "Pause card", r => Center(r, new Vector2(540f, 500f)) },
            { "Restoration ending", r => Center(r, new Vector2(840f, 660f)) },
            { "Coordinator dialogue", r => BottomCenter(r, new Vector2(1180f, 300f), new Vector2(0f, 48f)) }
        };

        public static void Apply(Canvas canvas)
        {
            foreach (Transform node in canvas.GetComponentsInChildren<Transform>(true))
                if (node is RectTransform rect && Placements.TryGetValue(node.name, out Action<RectTransform> place))
                    place(rect);
        }

        public static void TopLeft(RectTransform r, Vector2 size, Vector2 offset = default) =>
            Place(r, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(Margin, -Margin) + offset, size);

        public static void TopRight(RectTransform r, Vector2 size, Vector2 offset = default) =>
            Place(r, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-Margin, -Margin) + offset, size);

        public static void TopCenter(RectTransform r, Vector2 size, Vector2 offset = default) =>
            Place(r, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -Margin) + offset, size);

        public static void BottomCenter(RectTransform r, Vector2 size, Vector2 offset = default) =>
            Place(r, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, Margin) + offset, size);

        public static void BottomLeft(RectTransform r, Vector2 size, Vector2 offset = default) =>
            Place(r, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(Margin, Margin) + offset, size);

        public static void BottomRight(RectTransform r, Vector2 size, Vector2 offset = default) =>
            Place(r, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-Margin, Margin) + offset, size);

        public static void Center(RectTransform r, Vector2 size, Vector2 offset = default) =>
            Place(r, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), offset, size);

        private static void Place(RectTransform r, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            r.anchorMin = r.anchorMax = anchor;
            r.pivot = pivot;
            r.anchoredPosition = position;
            r.sizeDelta = size;
        }
    }
}
