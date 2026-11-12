using UnityEditor.SceneManagement;
var s = EditorSceneManager.OpenScene("Assets/Scenes/VillageRestoration.unity", OpenSceneMode.Single);
return "opened " + s.name + " roots=" + s.GetRootGameObjects().Length;
