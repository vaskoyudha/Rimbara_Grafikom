using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rimbara.UI
{
    /// <summary>
    /// Pads and spaces the contents of the small HUD panels so text never
    /// crowds the ornate frames and the counters stay legible.
    /// </summary>
    public static class HudContentLayout
    {
        private static readonly Dictionary<string, float> CenteredCards = new Dictionary<string, float>
        {
            { "Main menu card", 604f },
            { "Volunteer briefing", 728f },
            { "How to play card", 670f },
            { "Pause card", 434f },
            { "Restoration ending", 708f }
        };

        public static void Apply(Canvas canvas)
        {
            if (canvas == null) return;

            // Mission card (484x192) with 28px/24px inner wood-frame safe padding.
            Icon(canvas, "Mission card", "Mission icon", 38f, 28f, -24f);
            TextAt(canvas, "Mission card", "Mission number", 18f, 12.5f, TextAlignmentOptions.Left, 76f, 28f, -24f);
            TextAt(canvas, "Mission card", "Mission title", 30f, 24f, TextAlignmentOptions.Left, 76f, 28f, -44f);
            Rect(canvas, "Mission card", "Mission divider", 10f, 28f, -79f);
            Rect(canvas, "Mission card", "Mission task inset", 74f, 26f, -92f);
            Text(canvas, "Mission card", "Mission description", 54f, 17f, TextAlignmentOptions.TopLeft, 40f, -102f);

            // Restoration counters (428x248) with proper icons and clean wood-frame clearance.
            Icon(canvas, "Restoration counters", "Counters icon", 18f, 28f, -23f);
            TextAt(canvas, "Restoration counters", "Counters header", 18f, 12.5f, TextAlignmentOptions.Left, 54f, 28f, -23f);
            Rect(canvas, "Restoration counters", "Counters divider", 9f, 28f, -44f);
            Rect(canvas, "Restoration counters", "Counter row 0", 32f, 26f, -56f);
            Rect(canvas, "Restoration counters", "Counter row 1", 32f, 26f, -94f);
            Rect(canvas, "Restoration counters", "Counter row 2", 32f, 26f, -132f);
            Icon(canvas, "Restoration counters", "Counter icon 0", 20f, 38f, -62f);
            Icon(canvas, "Restoration counters", "Counter icon 1", 20f, 38f, -100f);
            Icon(canvas, "Restoration counters", "Counter icon 2", 20f, 38f, -138f);
            TextAt(canvas, "Restoration counters", "Seedling counter", 24f, 13.5f, TextAlignmentOptions.Left, 66f, 36f, -60f);
            TextAt(canvas, "Restoration counters", "Tree counter", 24f, 13.5f, TextAlignmentOptions.Left, 66f, 36f, -98f);
            TextAt(canvas, "Restoration counters", "Fire counter", 24f, 13.5f, TextAlignmentOptions.Left, 66f, 36f, -136f);
            Text(canvas, "Restoration counters", "Restoration percent", 20f, 13f, TextAlignmentOptions.Left, 28f, -176f);
            Rect(canvas, "Restoration counters", "Restoration bar background", 16f, 28f, -200f);
            InsetFill(canvas, "Restoration counters", "Restoration bar background", "Restoration bar fill", 3f);

            // Thin strips and prompts: centred, with side padding.
            Text(canvas, "Controls reminder", "Controls text", 26f, 17f, TextAlignmentOptions.Center, 18f, -6f);
            TextAt(canvas, "Arunika field map", "Map title", 18f, 11.5f, TextAlignmentOptions.Left, 24f, 64f, -18f);
            TextAt(canvas, "Arunika field map", "Map M hint", 18f, 11f, TextAlignmentOptions.Right, 24f, 24f, -18f);
            Rect(canvas, "Arunika field map", "Map coordinates", 204f, 24f, -44f);
            Text(canvas, "Next destination", "Waypoint distance", 22f, 16f, TextAlignmentOptions.Center, 16f, -4f);
            CenterChild(canvas, "Next destination", "Waypoint 3D arrow", new Vector2(84f, 84f), new Vector2(0f, -22f));
            Text(canvas, "Next destination", "Waypoint instruction", 24f, 15.5f, TextAlignmentOptions.Center, 16f, -112f);
            Text(canvas, "Interaction prompt", "Interaction text", 30f, 20f, TextAlignmentOptions.Center, 20f, -13f);
            Text(canvas, "Feedback toast", "Feedback text", 34f, 20f, TextAlignmentOptions.Center, 20f, -13f);

            foreach (KeyValuePair<string, float> card in CenteredCards)
                CenterCard(canvas, card.Key, card.Value);
        }

        /// <summary>
        /// Gives a screen-centred card symmetric inner margins: full-width
        /// blocks share one centred left edge, and rows of buttons are
        /// centred as a group while keeping their spacing.
        /// </summary>
        private static void CenterCard(Canvas canvas, string cardName, float contentWidth)
        {
            RectTransform card = Find(canvas.transform, cardName);
            if (card == null) return;

            float left = (card.rect.width - contentWidth) * 0.5f;
            var rows = new Dictionary<int, List<RectTransform>>();

            foreach (Transform child in card)
            {
                if (!(child is RectTransform rect)) continue;

                if (rect.GetComponent<Button>() != null)
                {
                    int rowKey = Mathf.RoundToInt(rect.anchoredPosition.y);
                    if (!rows.TryGetValue(rowKey, out List<RectTransform> row))
                        rows[rowKey] = row = new List<RectTransform>();
                    row.Add(rect);
                    continue;
                }

                if (rect.rect.width < contentWidth - 1f) continue;
                PlaceTopLeft(rect, left, contentWidth);
            }

            foreach (List<RectTransform> row in rows.Values)
                CenterRow(row, card.rect.width);
        }

        private static void CenterRow(List<RectTransform> row, float cardWidth)
        {
            row.Sort((a, b) => a.anchoredPosition.x.CompareTo(b.anchoredPosition.x));

            int count = row.Count;
            var gaps = new float[count];
            float total = 0f;
            for (int i = 0; i < count; i++)
            {
                total += row[i].rect.width;
                if (i > 0)
                {
                    float previousRight = row[i - 1].anchoredPosition.x + row[i - 1].rect.width;
                    gaps[i] = Mathf.Max(0f, row[i].anchoredPosition.x - previousRight);
                    total += gaps[i];
                }
            }

            float cursor = (cardWidth - total) * 0.5f;
            for (int i = 0; i < count; i++)
            {
                PlaceTopLeft(row[i], cursor, row[i].rect.width);
                cursor += row[i].rect.width;
                if (i + 1 < count) cursor += gaps[i + 1];
            }
        }

        private static void PlaceTopLeft(RectTransform rect, float x, float width)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(width, rect.sizeDelta.y);
            rect.anchoredPosition = new Vector2(x, rect.anchoredPosition.y);
        }


        private static void Text(Canvas canvas, string panelName, string childName, float height,
            float fontSize, TextAlignmentOptions alignment, float padding, float top)
        {
            RectTransform panel = Find(canvas.transform, panelName);
            if (panel == null) return;
            RectTransform child = Find(panel, childName);
            if (child == null) return;

            Place(child, panel, height, padding, top);
            TMP_Text text = child.GetComponent<TMP_Text>();
            if (text != null)
            {
                text.enableAutoSizing = false;
                text.fontSize = fontSize;
                text.alignment = alignment;
            }
        }

        private static void TextAt(Canvas canvas, string panelName, string childName, float height,
            float fontSize, TextAlignmentOptions alignment, float left, float rightPadding, float top)
        {
            RectTransform panel = Find(canvas.transform, panelName);
            if (panel == null) return;
            RectTransform child = Find(panel, childName);
            if (child == null) return;
            child.anchorMin = new Vector2(0f, 1f);
            child.anchorMax = new Vector2(0f, 1f);
            child.pivot = new Vector2(0f, 1f);
            child.sizeDelta = new Vector2(panel.rect.width - left - rightPadding, height);
            child.anchoredPosition = new Vector2(left, top);
            TMP_Text text = child.GetComponent<TMP_Text>();
            if (text != null)
            {
                text.enableAutoSizing = false;
                text.fontSize = fontSize;
                text.alignment = alignment;
            }
        }

        private static void Icon(Canvas canvas, string panelName, string childName, float size, float left, float top)
        {
            RectTransform panel = Find(canvas.transform, panelName);
            if (panel == null) return;
            RectTransform child = Find(panel, childName);
            if (child == null) return;
            child.anchorMin = new Vector2(0f, 1f);
            child.anchorMax = new Vector2(0f, 1f);
            child.pivot = new Vector2(0f, 1f);
            child.sizeDelta = new Vector2(size, size);
            child.anchoredPosition = new Vector2(left, top);
        }

        private static void Rect(Canvas canvas, string panelName, string childName, float height, float padding, float top)
        {
            RectTransform panel = Find(canvas.transform, panelName);
            if (panel == null) return;
            RectTransform child = Find(panel, childName);
            if (child != null) Place(child, panel, height, padding, top);
        }

        private static void Place(RectTransform child, RectTransform panel, float height, float padding, float top)
        {
            child.anchorMin = new Vector2(0f, 1f);
            child.anchorMax = new Vector2(0f, 1f);
            child.pivot = new Vector2(0f, 1f);
            child.sizeDelta = new Vector2(panel.rect.width - padding * 2f, height);
            child.anchoredPosition = new Vector2(padding, top);
        }

        private static void CenterChild(Canvas canvas, string panelName, string childName, Vector2 size, Vector2 offset)
        {
            RectTransform panel = Find(canvas.transform, panelName);
            if (panel == null) return;
            RectTransform child = Find(panel, childName);
            if (child == null) return;
            child.anchorMin = new Vector2(0.5f, 1f);
            child.anchorMax = new Vector2(0.5f, 1f);
            child.pivot = new Vector2(0.5f, 1f);
            child.sizeDelta = size;
            child.anchoredPosition = offset;
        }

        private static void InsetFill(Canvas canvas, string panelName, string barName, string fillName, float inset)
        {
            RectTransform panel = Find(canvas.transform, panelName);
            if (panel == null) return;
            RectTransform bar = Find(panel, barName);
            if (bar == null) return;
            RectTransform fill = Find(bar, fillName);
            if (fill == null) return;
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = new Vector2(inset, inset);
            fill.offsetMax = new Vector2(-inset, -inset);
        }

        private static RectTransform Find(Transform root, string name)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t as RectTransform;
            return null;
        }
    }
}
