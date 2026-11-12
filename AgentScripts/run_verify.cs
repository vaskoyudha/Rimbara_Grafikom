using UnityEditor;
using UnityEngine;

var before = Time.realtimeSinceStartup;
EditorApplication.ExecuteMenuItem("RIMBARA/Verify authored project");
return "invoked at " + before + " scene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
