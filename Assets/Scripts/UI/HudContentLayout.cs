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
            { "Main menu card", 584f },
            { "Volunteer briefing", 728f },
            { "How to play card", 670f },
            { "Pause card", 434f },
            { "Restoration ending", 708f }
        };

        public static void Apply(Canvas canvas)
        {
            if (canvas == null) return;

            // Mission card (480x196).
            Text(canvas, "Mission card", "Mission number", 16f, 14f, TextAlignmentOptions.Left, 18f, -14f);
            Text(canvas, "Mission card", "Mission title", 34f, 26f, TextAlignmentOptions.Left, 18f, -36f);
            Text(canvas, "Mission card", "Mission description", 96f, 19f, TextAlignmentOptions.TopLeft, 18f, -76f);

            // Restoration counters (340x200).
            Text(canvas, "Restoration counters", "Seedling counter", 34f, 26f, TextAlignmentOptions.Left, 18f, -14f);
            Text(canvas, "Restoration counters", "Tree counter", 24f, 18f, TextAlignmentOptions.Left, 18f, -56f);
            Text(canvas, "Restoration counters", "Fire counter", 24f, 18f, TextAlignmentOptions.Left, 18f, -86f);
            Text(canvas, "Restoration counters", "Restoration percent", 22f, 16f, TextAlignmentOptions.Left, 18f, -118f);
            Rect(canvas, "Restoration counters", "Restoration bar background", 8f, 18f, -150f);

            // Thin strips and prompts: centred, with side padding.
            Text(canvas, "Controls reminder", "Controls text", 24f, 15f, TextAlignmentOptions.Center, 16f, -7f);
            Text(canvas, "Next destination", "Waypoint instruction", 28f, 18f, TextAlignmentOptions.Center, 16f, -8f);
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
                text.fontSize = fontSize;
                text.alignment = alignment;
            }
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

        private static RectTransform Find(Transform root, string name)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t as RectTransform;
            return null;
        }
    }
}
