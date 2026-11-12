using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
var sb=new StringBuilder();
sb.AppendLine("active="+SceneManager.GetActiveScene().name+" path="+SceneManager.GetActiveScene().path);
foreach(var r in SceneManager.GetActiveScene().GetRootGameObjects()) sb.AppendLine(" root: "+r.name);
sb.AppendLine("zoneA="+(GameObject.Find("Zone A - volunteer post and damaged forest")!=null));
sb.AppendLine("seed1="+(GameObject.Find("Native seedling 01")!=null));
return sb.ToString();
