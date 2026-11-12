using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

var sb = new StringBuilder();

string tilesDir = "Assets/External/KenneyPixelUI/TilesUsed";
foreach (var path in Directory.GetFiles(tilesDir, "*.png"))
{
    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    var imp = AssetImporter.GetAtPath(path) as TextureImporter;
    if (imp == null) { sb.AppendLine("no importer " + path); continue; }
    imp.textureType = TextureImporterType.Sprite;
    imp.spriteImportMode = SpriteImportMode.Single;
    imp.filterMode = FilterMode.Point;
    imp.textureCompression = TextureImporterCompression.Uncompressed;
    imp.mipmapEnabled = false;
    imp.alphaIsTransparency = true;
    imp.spritePixelsPerUnit = 100f;
    bool isRound = path.Contains("tile_52") || path.Contains("tile_39") || path.Contains("tile_65");
    imp.spriteBorder = isRound ? Vector4.zero : new Vector4(8f, 8f, 8f, 8f);
    imp.SaveAndReimport();
}
sb.AppendLine("tiles imported=" + Directory.GetFiles(tilesDir, "*.png").Length);

foreach (var path in Directory.GetFiles("Assets/External/KenneyPixelUI/Icons", "*.png"))
{
    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    var imp = AssetImporter.GetAtPath(path) as TextureImporter;
    if (imp == null) continue;
    imp.textureType = TextureImporterType.Sprite;
    imp.spriteImportMode = SpriteImportMode.Single;
    imp.filterMode = FilterMode.Point;
    imp.textureCompression = TextureImporterCompression.Uncompressed;
    imp.mipmapEnabled = false;
    imp.alphaIsTransparency = true;
    imp.spritePixelsPerUnit = 16f;
    imp.spriteBorder = Vector4.zero;
    imp.SaveAndReimport();
}
sb.AppendLine("icons imported");

string solidPath = "Assets/External/KenneyPixelUI/pixel_solid.png";
if (!File.Exists(solidPath))
{
    var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
    var px = new Color32[64];
    for (int i = 0; i < 64; i++) px[i] = new Color32(255, 255, 255, 255);
    tex.SetPixels32(px);
    tex.Apply();
    File.WriteAllBytes(solidPath, tex.EncodeToPNG());
    Object.DestroyImmediate(tex);
    AssetDatabase.ImportAsset(solidPath, ImportAssetOptions.ForceUpdate);
    var imp = AssetImporter.GetAtPath(solidPath) as TextureImporter;
    imp.textureType = TextureImporterType.Sprite;
    imp.spriteImportMode = SpriteImportMode.Single;
    imp.filterMode = FilterMode.Point;
    imp.textureCompression = TextureImporterCompression.Uncompressed;
    imp.mipmapEnabled = false;
    imp.spritePixelsPerUnit = 8f;
    imp.SaveAndReimport();
    sb.AppendLine("solid created");
}
AssetDatabase.SaveAssets();
AssetDatabase.Refresh();
return sb.ToString();
