using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

var sb = new System.Text.StringBuilder();

string atlas = "Assets/External/KenneyPixelUI/Pack/Tilesheets/Large tiles/Thick outline/tilemap_packed.png";
AssetDatabase.ImportAsset(atlas, ImportAssetOptions.ForceUpdate);
var ti = AssetImporter.GetAtPath(atlas) as TextureImporter;
if (ti == null) throw new System.Exception("atlas importer missing after import");

ti.textureType = TextureImporterType.Sprite;
ti.spriteImportMode = SpriteImportMode.Multiple;
ti.filterMode = FilterMode.Point;
ti.textureCompression = TextureImporterCompression.Uncompressed;
ti.mipmapEnabled = false;
ti.alphaIsTransparency = true;
ti.spritePixelsPerUnit = 100f;
ti.SaveAndReimport();

var factory = new SpriteDataProviderFactories();
factory.Init();
var dp = factory.GetSpriteEditorDataProviderFromObject(ti);
dp.InitSpriteEditorDataProvider();
var rects = new List<SpriteRect>();
for (int r = 0; r < 7; r++)
    for (int c = 0; c < 13; c++)
    {
        int idx = r * 13 + c;
        rects.Add(new SpriteRect
        {
            name = "tile_" + idx,
            rect = new Rect(c * 32, 224 - (r + 1) * 32, 32, 32),
            border = new Vector4(8, 8, 8, 8),
            alignment = SpriteAlignment.Center,
            pivot = new Vector2(0.5f, 0.5f)
        });
    }
dp.SetSpriteRects(rects.ToArray());
dp.Apply();
ti.SaveAndReimport();
sb.AppendLine("atlas sliced=" + rects.Count);

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
