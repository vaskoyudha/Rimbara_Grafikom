using System;
using System.Linq;
using Rimbara.UI;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Rimbara.Editor
{
    public static class KingsforgeThemeBuilder
    {
        public const string AssetPath = "Assets/UI/KingsforgeUiKit.asset";

        [MenuItem("RIMBARA/UI/Build Kingsforge theme")]
        public static void Build()
        {
            string root = KingsforgeImportReport.DetectRoot();
            UiKit kit = AssetDatabase.LoadAssetAtPath<UiKit>(AssetPath);
            bool created = kit == null;
            if (created) kit = ScriptableObject.CreateInstance<UiKit>();

            TMP_FontAsset fallbackFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            kit.HeadingFont = FindFont(root, "Cinzel") ?? fallbackFont;
            kit.BodyFont = FindFont(root, "Alegreya") ?? fallbackFont;
            if (kit.HeadingFont == null || kit.BodyFont == null)
                Debug.LogWarning("Kingsforge fonts not found; import TMP Essential Resources or assign fonts manually.");

            kit.Window = FindSprite(root, "window");
            kit.ParchmentPanel = FindSprite(root, "parchment") ?? kit.Window;
            kit.Popup = FindSprite(root, "popup") ?? kit.Window;
            kit.Tooltip = FindSprite(root, "tooltip") ?? kit.Popup;
            kit.Inset = FindSprite(root, "inset") ?? kit.Window;
            kit.Divider = FindSprite(root, "divider");
            kit.Ribbon = FindSprite(root, "ribbon");

            kit.ButtonPrimaryNormal = FindSprite(root, "button", "gold", "normal");
            kit.ButtonPrimaryHover = FindSprite(root, "button", "gold", "hover") ?? kit.ButtonPrimaryNormal;
            kit.ButtonPrimaryPressed = FindSprite(root, "button", "gold", "pressed") ?? kit.ButtonPrimaryNormal;
            kit.ButtonPrimaryDisabled = FindSprite(root, "button", "gold", "disabled") ?? kit.ButtonPrimaryNormal;
            kit.ButtonSecondaryNormal = FindSprite(root, "button", "stone", "normal") ?? kit.ButtonPrimaryNormal;
            kit.ButtonSecondaryHover = FindSprite(root, "button", "stone", "hover") ?? kit.ButtonSecondaryNormal;
            kit.ButtonSecondaryPressed = FindSprite(root, "button", "stone", "pressed") ?? kit.ButtonSecondaryNormal;
            kit.ButtonSecondaryDisabled = FindSprite(root, "button", "stone", "disabled") ?? kit.ButtonSecondaryNormal;

            kit.BarFrame = FindSprite(root, "bar", "frame");
            kit.BarHealthFill = FindSprite(root, "fill", "health");
            kit.BarManaFill = FindSprite(root, "fill", "mana") ?? kit.BarHealthFill;
            kit.BarXpFill = FindSprite(root, "fill", "xp") ?? kit.BarHealthFill;
            kit.BarStaminaFill = FindSprite(root, "fill", "stamina") ?? kit.BarHealthFill;

            kit.SliderTrack = FindSprite(root, "slider", "track") ?? kit.BarFrame;
            kit.SliderFill = FindSprite(root, "slider", "fill") ?? kit.BarHealthFill;
            kit.SliderHandle = FindSprite(root, "slider", "handle") ?? kit.SliderFill;
            kit.ScrollbarTrack = FindSprite(root, "scrollbar", "track");
            kit.ScrollbarHandle = FindSprite(root, "scrollbar", "handle");
            kit.ToggleOn = FindSprite(root, "toggle", "on");
            kit.ToggleOff = FindSprite(root, "toggle", "off");
            kit.CheckboxOn = FindSprite(root, "checkbox", "on");
            kit.CheckboxOff = FindSprite(root, "checkbox", "off");
            kit.Dropdown = FindSprite(root, "dropdown");
            kit.InputField = FindSprite(root, "input", "field");

            kit.SlotEmpty = FindSprite(root, "slot", "empty");
            kit.SlotSelected = FindSprite(root, "slot", "selected");
            kit.SlotLocked = FindSprite(root, "slot", "locked");

            if (created) AssetDatabase.CreateAsset(kit, AssetPath);
            EditorUtility.SetDirty(kit);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var missing = kit.MissingRoles();
            if (missing.Count > 0)
                throw new InvalidOperationException("Kingsforge theme is incomplete: " + string.Join(", ", missing)
                    + ". Inspect Artifacts/kingsforge-import.json and adjust the keyword search.");
            Debug.Log("Kingsforge theme ready: " + AssetPath);
        }

        private static TMP_FontAsset FindFont(string root, string keyword) =>
            AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { root })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>)
                .FirstOrDefault(f => f != null && f.name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);

        private static Sprite FindSprite(string root, params string[] keywords)
        {
            string[] paths = AssetDatabase.FindAssets("t:Sprite", new[] { root })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => keywords.All(k => p.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0))
                .OrderBy(p => p.Length)
                .ToArray();
            foreach (string path in paths)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null) continue;
                Debug.Log($"Kingsforge role [{string.Join("+", keywords)}] -> {path}");
                return sprite;
            }
            return null;
        }
    }
}
