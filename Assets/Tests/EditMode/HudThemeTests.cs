using NUnit.Framework;
using Rimbara.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Rimbara.Tests
{
    public sealed class HudThemeTests
    {
        private static UiKit Load() => AssetDatabase.LoadAssetAtPath<UiKit>("Assets/UI/FantasticUiKit.asset");

        [Test]
        public void ButtonUsesFantasticSpritesForEveryState()
        {
            UiKit kit = Load();
            if (kit == null) Assert.Ignore("Fantastic kit not imported.");
            var go = new GameObject("btn", typeof(RectTransform), typeof(Image), typeof(Button));
            var image = go.GetComponent<Image>();
            var button = go.GetComponent<Button>();
            HudTheme.ApplyButton(button, image, kit, true);
            Assert.That(image.sprite, Is.EqualTo(kit.ButtonPrimaryNormal));
            Assert.That(button.spriteState.highlightedSprite, Is.EqualTo(kit.ButtonPrimaryHover));
            Assert.That(button.spriteState.pressedSprite, Is.EqualTo(kit.ButtonPrimaryPressed));
            Object.DestroyImmediate(go);
        }

        [Test]
        public void PanelUsesNineSlicedWindow()
        {
            UiKit kit = Load();
            if (kit == null) Assert.Ignore("Fantastic kit not imported.");
            var go = new GameObject("panel", typeof(RectTransform), typeof(Image));
            var image = go.GetComponent<Image>();
            HudTheme.ApplyPanel(image, kit, HudSurface.Window);
            Assert.That(image.sprite, Is.EqualTo(kit.Window));
            Assert.That(image.type, Is.EqualTo(Image.Type.Sliced));
            Object.DestroyImmediate(go);
        }
    }
}
