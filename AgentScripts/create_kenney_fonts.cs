using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using TMPro;
var sb=new StringBuilder();
string dir="Assets/External/KenneyFonts";
string outDir=dir+"/TMP";
if(!AssetDatabase.IsValidFolder(outDir)) AssetDatabase.CreateFolder(dir,"TMP");
string[] names={"Kenney Future","Kenney Pixel","Kenney Mini Square","Kenney Blocks"};
foreach(var n in names){
  string ttf=dir+"/"+n+".ttf";
  string ap=outDir+"/"+n+" SDF.asset";
  if(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ap)!=null){ sb.AppendLine("exists "+n); continue; }
  AssetDatabase.ImportAsset(ttf, ImportAssetOptions.ForceUpdate);
  var font=AssetDatabase.LoadAssetAtPath<Font>(ttf);
  if(font==null){ sb.AppendLine("missing "+ttf); continue; }
  var fa=TMP_FontAsset.CreateFontAsset(font,90,9,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
  fa.name=n+" SDF";
  AssetDatabase.CreateAsset(fa,ap);
  fa.atlasTextures[0].name=n+" Atlas";
  AssetDatabase.AddObjectToAsset(fa.atlasTextures[0],fa);
  fa.material.name=n+" Material";
  AssetDatabase.AddObjectToAsset(fa.material,fa);
  string missing;
  fa.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,:;/%+-!?()[]&'\u2019\u201C\u201D",out missing);
  EditorUtility.SetDirty(fa);
  sb.AppendLine("created "+n);
}
AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
return sb.ToString();
