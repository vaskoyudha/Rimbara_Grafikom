using UnityEditor.SceneManagement;
var s=EditorSceneManager.OpenScene("Assets/Scenes/Main.unity", OpenSceneMode.Single);
return "opened "+s.name;
