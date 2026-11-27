using System;
using System.Linq;
using Rimbara.UI;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Rimbara.Editor
{
    public static class FantasticThemeBuilder
    {
        public const string AssetPath = "Assets/UI/FantasticUiKit.asset";

        [MenuItem("RIMBARA/UI/Build Fantastic theme")]
        public static void Build()
        {
            string root = FantasticImportReport.DetectRoot();
            UiKit kit = AssetDatabase.LoadAssetAtPath<UiKit>(AssetPath);
            bool created = kit == null;
            if (created) kit = ScriptableObject.CreateInstance<UiKit>();

            // Fantastic UI ships sprites and prefabs but no fonts. Use the bundled
            // fantasy faces (Cinzel for headings, EB Garamond for body), falling
            // back to any kit font, then the TMP default.
            TMP_FontAsset fallbackFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            TMP_FontAsset headingFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/External/Fonts/TMP/Cinzel-Bold SDF.asset");
            TMP_FontAsset bodyFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/External/Fonts/TMP/EBGaramond-Regular SDF.asset");
            TMP_FontAsset kitFont = FindAnyFont(root);
            kit.HeadingFont = headingFont ?? kitFont ?? fallbackFont;
            kit.BodyFont = bodyFont ?? kitFont ?? fallbackFont;
            kit.Cream = new Color32(0xFF, 0xF0, 0xCC, 0xFF);
            kit.TitleMaterial = EnsureTitleMaterial(kit.HeadingFont);
            kit.SeparatorSprite = FantasySpriteAssetInstaller.Ensure();
            if (kit.HeadingFont == null || kit.BodyFont == null)
                Debug.LogWarning("No TMP font found; import TMP Essential Resources or assign fonts manually.");

            kit.GameLogo = LoadLogoSprite("Assets/Art/UI/RimbaraLogo.png");
            kit.Window = FindSprite(root, "Panel_WoodFrame");
            kit.ParchmentPanel = FindSprite(root, "Panel_Parchment") ?? kit.Window;
            kit.Popup = FindSprite(root, "Panel_Runestone") ?? kit.Window;
            kit.Tooltip = FindSprite(root, "Tooltip_Ink") ?? kit.Popup;
            kit.Inset = FindSprite(root, "Tile_Leather") ?? kit.Window;
            Sprite externalCardInset = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Panels/CardInset_DarkGoldBracket.png");
            kit.CardInset = externalCardInset != null ? externalCardInset : kit.Tooltip;
            kit.RowInset = FindSprite(root, "Slot_Crimson_Inset") ?? kit.Inset;
            Sprite externalDivider = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Dividers/ElegantDivider_Ornate.png");
            kit.Divider = externalDivider != null ? externalDivider : FindSprite(root, "Divider_Gold");
            kit.Ribbon = FindSprite(root, "Ornament_Horizontal");

            kit.ButtonPrimaryNormal = FindSprite(root, "Button_Crimson_Gilded") ?? FindSprite(root, "Button_Crimson");
            kit.ButtonPrimaryHover = FindSprite(root, "Button_Crimson_Gilded") ?? kit.ButtonPrimaryNormal;
            kit.ButtonPrimaryPressed = FindSprite(root, "Button_Crimson") ?? kit.ButtonPrimaryNormal;
            kit.ButtonPrimaryDisabled = FindSprite(root, "Button_Wood_Dark") ?? kit.ButtonPrimaryNormal;
            kit.ButtonSecondaryNormal = FindSprite(root, "Button_Wood") ?? kit.ButtonPrimaryNormal;
            kit.ButtonSecondaryHover = FindSprite(root, "Button_Leather") ?? kit.ButtonSecondaryNormal;
            kit.ButtonSecondaryPressed = FindSprite(root, "Button_Wood_Dark") ?? kit.ButtonSecondaryNormal;
            kit.ButtonSecondaryDisabled = FindSprite(root, "Button_Leather") ?? kit.ButtonSecondaryNormal;
            kit.ButtonAccentNormal = FindSprite(root, "Button_Olive_Gilded") ?? kit.ButtonPrimaryNormal;
            kit.ButtonAccentPressed = FindSprite(root, "Button_Olive") ?? kit.ButtonAccentNormal;

            kit.BarFrame = FindSprite(root, "Track_Dark");
            kit.BarHealthFill = FindSprite(root, "Bar_Fill_Red");
            kit.BarManaFill = FindSprite(root, "Bar_Fill_Blue") ?? kit.BarHealthFill;
            kit.BarXpFill = FindSprite(root, "Bar_Gold_Thin") ?? kit.BarHealthFill;
            kit.BarStaminaFill = FindSprite(root, "Bar_Fill_Green") ?? kit.BarHealthFill;

            kit.SliderTrack = FindSprite(root, "Track_Dark") ?? kit.BarFrame;
            kit.SliderFill = FindSprite(root, "Rounded_White") ?? kit.BarHealthFill;
            kit.SliderHandle = FindSprite(root, "Handle_Bronze") ?? kit.SliderFill;
            kit.ScrollbarTrack = FindSprite(root, "Track_Dark");
            kit.ScrollbarHandle = FindSprite(root, "Handle_Silver");
            kit.ToggleOn = FindSprite(root, "Toggle_Gold");
            kit.ToggleOff = FindSprite(root, "Toggle_Wood");
            kit.CheckboxOn = FindSprite(root, "Checkmark");
            kit.CheckboxOff = FindSprite(root, "Rounded_White");
            kit.Dropdown = FindSprite(root, "Button_Wood");
            kit.InputField = FindSprite(root, "Tile_Parchment");

            kit.SlotEmpty = FindSprite(root, "Slot_Bone");
            kit.SlotSelected = FindSprite(root, "Slot_Emerald_Glow");
            kit.SlotLocked = FindSprite(root, "Slot_Obsidian_Deep");

            kit.IconQuest = FindSprite(root, "Scroll_Color");
            kit.IconCompass = FindSprite(root, "Compass");
            kit.IconBag = FindSprite(root, "Backpack");
            kit.IconCleanup = FindSprite(root, "Broom_Color");
            kit.IconEmber = FindSprite(root, "Feather_Color");

            kit.IconFire = LoadIcon("Icon_Fire");
            kit.IconRubble = LoadIcon("Icon_Rubble");
            kit.IconSeedling = LoadIcon("Icon_Seedling");
            kit.IconZone = LoadIcon("Icon_Zone");
            kit.IconTree = LoadIcon("Icon_Tree");

            kit.WorldMap = AssetDatabase.LoadAssetAtPath<Sprite>(TopDownMapBaker.MapPath);
            kit.RadarBezel = AssetDatabase.LoadAssetAtPath<Sprite>(TopDownMapBaker.BezelPath);
            kit.RadarChevron = AssetDatabase.LoadAssetAtPath<Sprite>(TopDownMapBaker.ChevronPath);
            kit.RadarFov = AssetDatabase.LoadAssetAtPath<Sprite>(TopDownMapBaker.FovPath);
            kit.RadarBlipDiamond = AssetDatabase.LoadAssetAtPath<Sprite>(TopDownMapBaker.DiamondPath);
            kit.RadarBlipDot = AssetDatabase.LoadAssetAtPath<Sprite>(TopDownMapBaker.DotPath);
            kit.ZoneAThumbnail = AssetDatabase.LoadAssetAtPath<Sprite>(TopDownMapBaker.ZoneAPath);
            kit.ZoneBThumbnail = AssetDatabase.LoadAssetAtPath<Sprite>(TopDownMapBaker.ZoneBPath);
            kit.ZoneCThumbnail = AssetDatabase.LoadAssetAtPath<Sprite>(TopDownMapBaker.ZoneCPath);
            Terrain activeTerrain = Terrain.activeTerrain;
            if (activeTerrain != null && activeTerrain.terrainData != null)
            {
                Vector3 tp = activeTerrain.transform.position;
                Vector3 ts = activeTerrain.terrainData.size;
                float span = Mathf.Max(ts.x, ts.z);
                kit.WorldMin = new Vector2(tp.x, tp.z);
                kit.WorldSize = new Vector2(span, span);
            }

            if (created) AssetDatabase.CreateAsset(kit, AssetPath);
            EditorUtility.SetDirty(kit);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var missing = kit.MissingRoles();
            if (missing.Count > 0)
                throw new InvalidOperationException("Fantastic theme is incomplete: " + string.Join(", ", missing)
                    + ". Inspect Artifacts/fantastic-import.json and adjust the keyword search.");
            Debug.Log("Fantastic theme ready: " + AssetPath);
        }

        private static Material EnsureTitleMaterial(TMP_FontAsset headingFont)
        {
            const string path = "Assets/External/Fonts/TMP/Cinzel-Bold 3D.mat";
            if (headingFont == null || headingFont.material == null) return null;

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(headingFont.material.shader);
                material.CopyPropertiesFromMaterial(headingFont.material);
                AssetDatabase.CreateAsset(material, path);
            }

            // Raised (outer) bevel + dark outline + soft drop shadow for the 3D look.
            material.SetColor("_FaceColor", Color.white);
            material.SetFloat("_FaceDilate", 0.06f);
            material.SetColor("_OutlineColor", new Color(0.16f, 0.09f, 0.03f, 1f));
            material.SetFloat("_OutlineWidth", 0.12f);
            material.SetFloat("_OutlineSoftness", 0.02f);
            material.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.55f));
            material.SetFloat("_UnderlayOffsetX", 0.55f);
            material.SetFloat("_UnderlayOffsetY", -0.55f);
            material.SetFloat("_UnderlayDilate", 0.1f);
            material.SetFloat("_UnderlaySoftness", 0.18f);
            material.SetFloat("_Bevel", 0.6f);
            material.SetFloat("_BevelOffset", 0f);
            material.SetFloat("_BevelWidth", 0.22f);
            material.SetFloat("_BevelClamp", 0f);
            material.SetFloat("_BevelRoundness", 0.6f);
            material.SetFloat("_LightAngle", 2.6f);
            material.SetColor("_SpecularColor", new Color(1f, 0.97f, 0.85f, 1f));
            material.SetFloat("_SpecularPower", 1.5f);
            material.SetFloat("_Diffuse", 0.55f);
            material.SetFloat("_Ambient", 0.4f);
            material.SetFloat("_ShaderFlags", 1f);
            material.EnableKeyword("BEVEL_ON");
            material.EnableKeyword("UNDERLAY_ON");
            material.DisableKeyword("GLOW_ON");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static TMP_FontAsset FindAnyFont(string root) =>
            AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { root })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>)
                .FirstOrDefault(f => f != null);

        private static Sprite LoadLogoSprite(string path)
        {
            if (AssetImporter.GetAtPath(path) is TextureImporter importer)
            {
                bool dirty = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    dirty = true;
                }
                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    dirty = true;
                }
                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    dirty = true;
                }
                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    dirty = true;
                }
                if (dirty) importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite LoadIcon(string name)
        {
            return LoadLogoSprite("Assets/Art/UI/Icons/" + name + ".png");
        }

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
                Debug.Log($"Fantastic role [{string.Join("+", keywords)}] -> {path}");
                return sprite;
            }
            return null;
        }
    }
}
