using NUnit.Framework;
using Rimbara.UI;
using UnityEngine;

namespace Rimbara.Tests
{
    public sealed class HudLayoutTests
    {
        private static RectTransform NewRect()
        {
            var go = new GameObject("t", typeof(RectTransform));
            return go.GetComponent<RectTransform>();
        }

        [Test]
        public void TopLeftUsesTopLeftAnchorsAndMargin()
        {
            RectTransform rect = NewRect();
            HudLayout.TopLeft(rect, new Vector2(480f, 196f));
            Assert.That(rect.anchorMin, Is.EqualTo(new Vector2(0f, 1f)));
            Assert.That(rect.anchorMax, Is.EqualTo(new Vector2(0f, 1f)));
            Assert.That(rect.pivot, Is.EqualTo(new Vector2(0f, 1f)));
            Assert.That(rect.anchoredPosition, Is.EqualTo(new Vector2(HudLayout.Margin, -HudLayout.Margin)));
            Assert.That(rect.sizeDelta, Is.EqualTo(new Vector2(480f, 196f)));
            Object.DestroyImmediate(rect.gameObject);
        }

        [Test]
        public void BottomRightUsesBottomRightAnchors()
        {
            RectTransform rect = NewRect();
            HudLayout.BottomRight(rect, new Vector2(200f, 230f));
            Assert.That(rect.anchorMin, Is.EqualTo(new Vector2(1f, 0f)));
            Assert.That(rect.pivot, Is.EqualTo(new Vector2(1f, 0f)));
            Assert.That(rect.anchoredPosition, Is.EqualTo(new Vector2(-HudLayout.Margin, HudLayout.Margin)));
            Object.DestroyImmediate(rect.gameObject);
        }
    }
}
