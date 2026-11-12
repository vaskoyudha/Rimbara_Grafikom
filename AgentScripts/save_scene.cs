using UnityEditor.SceneManagement;
var s=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
EditorSceneManager.MarkSceneDirty(s);
EditorSceneManager.SaveScene(s);
return "saved "+s.path;
