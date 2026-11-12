using UnityEditor;
var path = System.Environment.GetEnvironmentVariable("HOME") + "/.local/share/unity3d/Asset Store-5.x/maanetorn/Textures MaterialsGUI Skins/Farm Game UI - Starter 2D.unitypackage";
AssetDatabase.ImportPackage(path, false);
return "import requested";
