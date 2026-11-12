using System.Text;
using UnityEngine;
var sb=new StringBuilder();
foreach(var t in Object.FindObjectsByType<Terrain>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
  sb.AppendLine("terrain="+t.name+" go="+t.gameObject.name+" active="+t.gameObject.activeInHierarchy+" size="+t.terrainData.size+" hmRes="+t.terrainData.heightmapResolution);
  var layers=t.terrainData.terrainLayers;
  sb.AppendLine(" layers="+layers.Length);
  foreach(var l in layers) sb.AppendLine("   "+(l!=null?l.name+" diffuse="+(l.diffuseTexture!=null?l.diffuseTexture.name:"null"):"null"));
}
return sb.ToString();
