using UnityEditor;
AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
AssetDatabase.SaveAssets();
return "refreshed";
