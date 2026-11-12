using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
var sb=new StringBuilder();
foreach(var r in SceneManager.GetActiveScene().GetRootGameObjects()){
  int rend=0, col=0;
  foreach(var t in r.GetComponentsInChildren<Transform>(true)){ rend+=t.GetComponents<Renderer>().Length; col+=t.GetComponents<Collider>().Length; }
  sb.AppendLine($"{r.name} renders={rend} colliders={col}");
}
return sb.ToString();
