using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using TMPro;

var sb = new StringBuilder();
string fontDir = "Assets/External/Fonts";
string assetDir = "Assets/External/Fonts/TMP";
if (!AssetDatabase.IsValidFolder(assetDir)) AssetDatabase.CreateFolder(fontDir, "TMP");

string[] sources = {
    "Nunito-Regular", "Nunito-SemiBold", "Nunito-Bold", "Nunito-ExtraBold",
    "Silkscreen-Regular", "Silkscreen-Bold"
};

foreach (var name in sources)
{
    string ttf = fontDir + "/" + name + ".ttf";
    string assetPath = assetDir + "/" + name + " SDF.asset";
    if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath) != null)
    {
        sb.AppendLine("exists " + name);
        continue;
    }
    AssetDatabase.ImportAsset(ttf, ImportAssetOptions.ForceUpdate);
    var font = AssetDatabase.LoadAssetAtPath<Font>(ttf);
    if (font == null) { sb.AppendLine("FONT MISSING " + ttf); continue; }

    var fa = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
    if (fa == null) { sb.AppendLine("CREATE FAILED " + name); continue; }
    fa.name = name + " SDF";
    AssetDatabase.CreateAsset(fa, assetPath);

    if (fa.atlasTextures != null && fa.atlasTextures.Length > 0 && fa.atlasTextures[0] != null)
    {
        fa.atlasTextures[0].name = name + " Atlas";
        AssetDatabase.AddObjectToAsset(fa.atlasTextures[0], fa);
    }
    if (fa.material != null)
    {
        fa.material.name = name + " Material";
        AssetDatabase.AddObjectToAsset(fa.material, fa);
    }
    fa.atlasPopulationMode = AtlasPopulationMode.Dynamic;
    string missing;
    fa.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,:;/%+-!?()[]&'\u2019\u201C\u201D", out missing);
    EditorUtility.SetDirty(fa);
    sb.AppendLine("created " + name + " missing=" + (missing == null ? "none" : missing));
}
AssetDatabase.SaveAssets();
AssetDatabase.Refresh();
return sb.ToString();
