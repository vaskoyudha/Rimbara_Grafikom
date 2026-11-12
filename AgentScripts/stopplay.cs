using UnityEditor;
var was = EditorApplication.isPlaying;
if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
return "wasPlaying=" + was;
