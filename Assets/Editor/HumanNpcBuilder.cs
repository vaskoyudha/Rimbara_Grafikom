using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Rimbara.Player;
using Rimbara.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Rimbara.Editor
{
    public static class HumanNpcBuilder
    {
        public const string Folder = "Assets/Art/Characters/HumanNPCs";
        [Serializable] private sealed class Library { public Character[] characters; }
        [Serializable] private sealed class Character { public string id; public float height; public Surface[] materials; }
        [Serializable] private sealed class Surface { public string name; public string texture; public float[] color; public bool cutout; }
        private static readonly string[] Roots = {
            "Coordinator visual - Ch42", "Ranger visual - Ch42 placeholder", "Elder visual - existing character placeholder",
            "Volunteer awaiting orders", "Evacuated villager at relief post",
            "Restored villager ambient 0", "Restored villager ambient 1", "Restored villager ambient 2", "Restored villager ambient 3", "Restored villager ambient 4"
        };
        private static readonly string[] Ids = { "Coordinator", "Ranger", "Elder", "Volunteer", "ResidentWoman", "VillagerMan", "VillagerWoman", "Volunteer", "ResidentWoman", "VillagerSenior" };

        [MenuItem("RIMBARA/Storyline/Replace NPC visuals with human models")]
        public static void Install()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Library library = JsonUtility.FromJson<Library>(File.ReadAllText(Folder + "/manifest.json"));
            if (library.characters == null || library.characters.Length != 8) throw new InvalidOperationException("Incomplete NPC library.");
            Directory.CreateDirectory(Folder + "/Materials");
            AssetDatabase.Refresh();
            foreach (Character character in library.characters) ConfigureImport(character);
            var scene = EditorSceneManager.OpenScene(StorylineBuilder.ScenePath, OpenSceneMode.Single);
            Transform[] all = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToArray();
            Transform[] targets = Roots.Select(name => all.Single(t => t.name == name)).ToArray();
            if (targets.Any(t => t.GetComponentInParent<VolunteerRig>() != null)) throw new InvalidOperationException("NPC target overlaps player.");
            if (targets.Any(t => t.GetComponent<Renderer>() != null)) throw new InvalidOperationException("Unexpected mesh on NPC container; inspect before replacing.");
            Directory.CreateDirectory("Backups");
            EditorSceneManager.SaveScene(scene, "Backups/FromAshes-before-human-npcs-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".unity", true);
            for (int i = 0; i < targets.Length; i++)
            {
                Transform target = targets[i];
                // These are the old visual-only containers, not the interaction roots.
                // Keep transforms and root ambient scripts; never remove mission components.
                foreach (Transform child in target.Cast<Transform>().ToArray()) UnityEngine.Object.DestroyImmediate(child.gameObject);
                Animator previous = target.GetComponent<Animator>();
                if (previous != null) UnityEngine.Object.DestroyImmediate(previous);
                GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/" + Ids[i] + ".fbx"), target);
                model.name = "Human model - " + Ids[i];
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                Vector3 scale = target.lossyScale;
                model.transform.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
                Animator animator = model.GetComponent<Animator>() ?? model.AddComponent<Animator>();
                animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Art/Characters/Mixamo/Volunteer.controller");
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                HumanNpcVisual identity = target.GetComponent<HumanNpcVisual>() ?? target.gameObject.AddComponent<HumanNpcVisual>();
                identity.Configure(Ids[i]);
                Debug.Log("RIMBARA NPC installed: " + target.name + " -> " + Ids[i]);
            }
            foreach (string obsolete in new[] { "Elder outer robe", "Elder grey cap", "Ranger green badge" })
            {
                Transform overlay = all.SingleOrDefault(t => t != null && t.name == obsolete);
                if (overlay != null) UnityEngine.Object.DestroyImmediate(overlay.gameObject);
            }
            Validate();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static void ConfigureImport(Character character)
        {
            string path = Folder + "/" + character.id + ".fbx";
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) throw new InvalidOperationException("Missing generated model: " + path);
            var bindings = new Dictionary<string, Material>();
            foreach (Surface surface in character.materials)
            {
                string materialPath = Folder + "/Materials/" + surface.name + ".mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, materialPath); }
                material.SetColor("_BaseColor", new Color(surface.color[0], surface.color[1], surface.color[2], surface.color[3]));
                material.SetFloat("_Smoothness", 0.18f);
                material.SetFloat("_Cull", surface.cutout ? 0f : 2f);
                material.SetFloat("_AlphaClip", surface.cutout ? 1f : 0f);
                material.SetFloat("_Cutoff", 0.4f);
                if (surface.cutout) material.EnableKeyword("_ALPHATEST_ON"); else material.DisableKeyword("_ALPHATEST_ON");
                material.renderQueue = surface.cutout ? 2450 : 2000;
                if (!string.IsNullOrEmpty(surface.texture))
                {
                    string texturePath = Folder + "/Textures/" + surface.texture;
                    var textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                    if (textureImporter == null) throw new InvalidOperationException("Missing texture: " + texturePath);
                    textureImporter.maxTextureSize = 1024;
                    textureImporter.alphaIsTransparency = surface.cutout;
                    textureImporter.SaveAndReimport();
                    material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
                }
                else material.SetTexture("_BaseMap", null);
                EditorUtility.SetDirty(material);
                bindings.Add(surface.name, material);
            }
            AssetDatabase.SaveAssets();
            // Texture/material imports may reload native importers; acquire a fresh
            // reference only after those imports, then save all rig settings together.
            importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.useFileScale = true;
            importer.globalScale = 1f;
            // Compression perturbs fitted clothing/body boundaries differently.
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.skinWeights = ModelImporterSkinWeights.Custom;
            importer.maxBonesPerVertex = 4;
            importer.isReadable = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            foreach (var binding in bindings)
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), binding.Key), binding.Value);
            importer.SaveAndReimport();
            Animator animator = AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<Animator>();
            if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                throw new InvalidOperationException("Invalid Humanoid avatar: " + character.id);
        }

        [MenuItem("RIMBARA/Storyline/Verify human NPCs")]
        public static void Verify()
        {
            EditorSceneManager.OpenScene(StorylineBuilder.ScenePath, OpenSceneMode.Single);
            Validate();
        }

        public static void RefreshImports()
        {
            Library library = JsonUtility.FromJson<Library>(File.ReadAllText(Folder + "/manifest.json"));
            AssetDatabase.Refresh();
            foreach (Character character in library.characters) ConfigureImport(character);
            AssetDatabase.SaveAssets();
            Verify();
        }

        private static void Validate()
        {
            HumanNpcVisual[] npcs = UnityEngine.Object.FindObjectsByType<HumanNpcVisual>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (npcs.Length != 10 || npcs.Select(n => n.CharacterId).Distinct().Count() != 8) throw new InvalidOperationException("Expected ten NPCs / eight unique characters.");
            foreach (HumanNpcVisual npc in npcs)
            {
                Animator animator = npc.GetComponentInChildren<Animator>(true);
                if (animator == null || !animator.isHuman || animator.avatar == null || !animator.avatar.isValid) throw new InvalidOperationException("Invalid NPC rig: " + npc.name);
                var renderers = npc.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                if (renderers.Length < 3 || renderers.Any(r => r.sharedMesh == null || r.bones.Length == 0 || r.sharedMaterials.Any(m => m == null))) throw new InvalidOperationException("Incomplete NPC mesh/materials: " + npc.name);
                if (npc.GetComponentInParent<VolunteerRig>() != null || npc.GetComponentsInChildren<Collider>(true).Length != 0) throw new InvalidOperationException("NPC visual interferes with player/physics: " + npc.name);
            }
            Debug.Log("RIMBARA human NPC validation passed: 10 visuals, 8 distinct human avatars; player preserved.");
            string[] identities = npcs.Select(n => n.CharacterId).Distinct().OrderBy(n => n).ToArray();
            ProjectBuilder.WriteArtifact("human-npc-verification.json", "{\"scene\":\"" + StorylineBuilder.ScenePath
                + "\",\"visuals\":10,\"uniqueAvatars\":8,\"playerPreserved\":true,\"result\":\"passed\",\"identities\":[\""
                + string.Join("\",\"", identities) + "\"]}");
        }
    }
}
