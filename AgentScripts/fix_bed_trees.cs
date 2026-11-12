using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Rimbara.Interaction;
var sb=new StringBuilder();
var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
var trees=new[]{"RestoredTree_01","RestoredTree_02","RestoredTree_03"}.Select(n=>AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/"+n+".prefab")).ToArray();
var beds=Object.FindObjectsByType<PlantingPoint>(FindObjectsInactive.Include,FindObjectsSortMode.None).OrderBy(x=>x.Slot).ToArray();
for(int i=0;i<beds.Length;i++){
  var so=new SerializedObject(beds[i]);
  var p=so.FindProperty("treePrefab");
  var before=p!=null?p.objectReferenceValue:null;
  if(p!=null) p.objectReferenceValue=trees[i%trees.Length];
  so.ApplyModifiedPropertiesWithoutUndo();
  PrefabUtility.RecordPrefabInstancePropertyModifications(beds[i]);
  sb.AppendLine($"bed{i} before={(before?before.name:"null")} after={trees[i%trees.Length].name}");
}
EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
return sb.ToString();
