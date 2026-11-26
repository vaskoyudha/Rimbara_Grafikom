using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;

namespace Rimbara.Editor
{
    /// <summary>
    /// Builds a TMP sprite asset from the Fantastic kit's Diamond widget so the
    /// menu can use a real image separator instead of a slash or a font glyph.
    /// </summary>
    public static class FantasySpriteAssetInstaller
    {
        public const string AssetPath = "Assets/UI/FantasticSeparator.asset";
        private const string DiamondPath = "Assets/ThirdParty/FantasticUI/Sprites/Widgets/Diamond.png";

        [MenuItem("RIMBARA/UI/Install separator sprite")]
        public static void Install() => Ensure();

        public static TMP_SpriteAsset Ensure()
        {
            Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(DiamondPath)
                .OfType<Sprite>()
                .FirstOrDefault();
            if (sprite == null)
            {
                Debug.LogWarning("Fantastic separator sprite missing: " + DiamondPath + ". Import the kit first.");
                return null;
            }

            TMP_SpriteAsset asset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(AssetPath);
            bool created = asset == null;
            if (created)
            {
                asset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
                AssetDatabase.CreateAsset(asset, AssetPath);
            }

            asset.hashCode = TMP_TextUtilities.GetSimpleHashCode(asset.name);
            typeof(TMP_Asset).GetProperty("version")?.SetValue(asset, "1.1.0");
            asset.spriteSheet = sprite.texture;

            TMP_SpriteGlyph glyph = new TMP_SpriteGlyph
            {
                index = 0,
                metrics = new GlyphMetrics(sprite.rect.width, sprite.rect.height,
                    -sprite.pivot.x, sprite.rect.height - sprite.pivot.y, sprite.rect.width),
                glyphRect = new GlyphRect(sprite.rect),
                scale = 1.0f,
                sprite = sprite
            };
            TMP_SpriteCharacter character = new TMP_SpriteCharacter(0xFFFE, glyph)
            {
                name = "diamond",
                scale = 0.8f
            };

            asset.spriteGlyphTable.Clear();
            asset.spriteGlyphTable.Add(glyph);
            asset.spriteCharacterTable.Clear();
            asset.spriteCharacterTable.Add(character);
            asset.UpdateLookupTables();

            if (asset.material == null)
            {
                Material material = new Material(Shader.Find("TextMeshPro/Sprite"));
                material.SetTexture(ShaderUtilities.ID_MainTex, asset.spriteSheet);
                material.name = asset.name + " Material";
                asset.material = material;
                AssetDatabase.AddObjectToAsset(material, asset);
            }
            else
            {
                asset.material.SetTexture(ShaderUtilities.ID_MainTex, asset.spriteSheet);
            }

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetPath);
            return asset;
        }
    }
}
