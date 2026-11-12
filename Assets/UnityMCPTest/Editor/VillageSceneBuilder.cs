using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace UnityMCPTest.Editor
{
    public static class VillageSceneBuilder
    {
        private const string Root = "Assets/UnityMCPTest";
        private const string SceneDir = Root + "/Scenes";
        private const string VillageScenePath = SceneDir + "/ThirdPersonVillage.unity";

        private const string KitRoot = "Assets/EmaceArt/Slavic World Free";
        private const string KitDemoScenePath = KitRoot + "/Scene/Slavica Free 2022 Version.unity";
        private const string KitGroundFallback = KitRoot + "/Prefabs/Terrain/Field/EA03_Environment_Field_Comp_01a.prefab";

        private const string SettingsDir = Root + "/Settings";
        private const string PostFxProfilePath = SettingsDir + "/VillagePostFxProfile.asset";
        private const string SkyboxMaterialPath = Root + "/Materials/VillageSkybox.mat";
        private const string RendererDataPath = "Assets/Rendering/Default_PipelineAsset_ForwardRenderer.asset";

        // Open bridge/plaza spot in the kit layout with a clear view into the village.
        private static readonly Vector3 SpawnHint = new Vector3(-8f, 0f, 4f);
        private const float SpawnYaw = 0f;

        [MenuItem("UnityMCPTest/Build Third Person Village Scene")]
        public static void Build()
        {
            TestCharacterBuilder.EnsureFolder(SceneDir);
            TestCharacterBuilder.EnsureFolder(Root + "/Animations");
            TestCharacterBuilder.EnsureFolder(Root + "/Animations/Mixamo");
            TestCharacterBuilder.EnsureFolder(Root + "/Materials");
            TestCharacterBuilder.EnsureFolder(Root + "/Materials/Skins");
            TestCharacterBuilder.EnsureFolder(SettingsDir);
            TestCharacterBuilder.ConfigureCharacterHumanoid();

            var idle = TestCharacterBuilder.LoadMixamoClip("Idle.fbx", true);
            var walk = TestCharacterBuilder.LoadMixamoClip("Walk.fbx", true);
            var run = TestCharacterBuilder.LoadMixamoClip("Run.fbx", true);
            var jab = TestCharacterBuilder.LoadMixamoClip("Jab.fbx", false);
            var cross = TestCharacterBuilder.LoadMixamoClip("Cross.fbx", false);
            var uppercut = TestCharacterBuilder.LoadMixamoClip("Uppercut.fbx", false);
            var fall = TestCharacterBuilder.LoadMixamoClip("FallIdle.fbx", true);
            var landing = TestCharacterBuilder.LoadMixamoClip("Landing.fbx", false, 16, 48);

            var controller = TestCharacterBuilder.BuildController(idle, walk, run, jab, cross, uppercut, fall, landing);
            var skins = TestCharacterBuilder.BuildSkinMaterials();

            BuildVillageScene(controller, skins);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[UnityMCPTest] Third-person village scene built at " + VillageScenePath);
        }

        private static void BuildVillageScene(AnimatorController controller, SkinPreset[] skins)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            if (!ImportVillageComposition(scene))
            {
                BuildFallbackGround();
            }

            Vector3 spawn = FindSpawn(SpawnHint);
            TestCharacterBuilder.CreatePlayer(
                controller,
                skins,
                spawn,
                Quaternion.Euler(0f, SpawnYaw, 0f));

            // The controller derives its initial yaw from the camera's euler Y.
            var camera = GameObject.Find("Main Camera");
            if (camera != null)
            {
                camera.transform.rotation = Quaternion.Euler(0f, SpawnYaw, 0f);
            }

            ConfigureGraphics(camera);

            EditorSceneManager.SaveScene(scene, VillageScenePath);
            Debug.Log("[UnityMCPTest] Player spawned at " + spawn.ToString("F2") +
                      " yaw " + SpawnYaw + " in " + scene.name);
        }

        /// <summary>
        /// Replaces the kit's cyan showcase haze with a warmer daylight setup and turns on
        /// URP post-processing. The imported kit scene has no UniversalAdditionalCameraData,
        /// so post FX never ran and the scene read as flat and blue.
        /// </summary>
        private static void ConfigureGraphics(GameObject cameraObject)
        {
            ConfigureSun();
            ConfigureFillLight();
            ConfigureFogAndAmbient();
            ConfigureSkybox();
            ConfigureCamera(cameraObject);
            ConfigureVolume();
            ConfigureAmbientOcclusion();
            ConfigurePipelineQuality();
        }

        private static void ConfigureSun()
        {
            Light sun = RenderSettings.sun;
            if (sun == null)
            {
                var found = GameObject.Find("Directional Light");
                if (found != null)
                {
                    sun = found.GetComponent<Light>();
                }
            }

            if (sun == null)
            {
                return;
            }

            sun.color = new Color(1f, 0.95f, 0.86f);
            sun.intensity = 1.8f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.72f;
            RenderSettings.sun = sun;
        }

        /// <summary>
        /// The sun comes from the front-right, so the right-hand building casts a hard
        /// shadow across the deck. A shadowless fill from the opposite side lifts those
        /// shadows without flattening the key light.
        /// </summary>
        private static void ConfigureFillLight()
        {
            var fillObject = new GameObject("Fill Light");
            var fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(1f, 0.97f, 0.92f);
            fill.intensity = 0.32f;
            fill.shadows = LightShadows.None;
            fillObject.transform.rotation = Quaternion.Euler(20f, 148f, 0f);
        }

        private static void ConfigureFogAndAmbient()
        {
            // The kit's fog is a pale cyan that starts at -23.8 m, so it tints everything
            // at close range. Push it out and warm it to read as daylight haze.
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.8f, 0.79f, 0.75f);
            RenderSettings.fogStartDistance = 45f;
            RenderSettings.fogEndDistance = 320f;

            // Skybox ambient sampled the blue cloud cubemap directly. Gradient ambient
            // keeps a warm ground bounce without washing the scene blue.
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.6f, 0.65f, 0.72f);
            RenderSettings.ambientEquatorColor = new Color(0.6f, 0.58f, 0.52f);
            RenderSettings.ambientGroundColor = new Color(0.34f, 0.3f, 0.25f);
            RenderSettings.ambientIntensity = 1.3f;
        }

        private static void ConfigureSkybox()
        {
            var source = RenderSettings.skybox;
            if (source == null || source.shader == null)
            {
                return;
            }

            var skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyboxMaterialPath);
            if (skybox == null)
            {
                skybox = new Material(source);
                skybox.name = "VillageSkybox";
                AssetDatabase.CreateAsset(skybox, SkyboxMaterialPath);
            }
            else
            {
                skybox.CopyPropertiesFromMaterial(source);
            }

            // The kit ships a cyan-green tint (0.854, 1.0, 0.968). Neutral-warm it so the
            // sky reads as daylight instead of casting blue over the whole village.
            if (skybox.HasProperty("_Tint"))
            {
                skybox.SetColor("_Tint", new Color(1f, 0.96f, 0.9f, 1f));
            }

            if (skybox.HasProperty("_Exposure"))
            {
                skybox.SetFloat("_Exposure", 1f);
            }

            EditorUtility.SetDirty(skybox);
            RenderSettings.skybox = skybox;
        }

        private static void ConfigureCamera(GameObject cameraObject)
        {
            if (cameraObject == null)
            {
                return;
            }

            var camera = cameraObject.GetComponent<Camera>();
            if (camera != null)
            {
                camera.allowHDR = true;
                camera.allowMSAA = true;
            }

            var data = cameraObject.GetComponent<UniversalAdditionalCameraData>();
            if (data == null)
            {
                data = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            }

            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            data.antialiasingQuality = AntialiasingQuality.High;
            data.renderShadows = true;
            data.requiresDepthTexture = true;
            data.volumeLayerMask = 1;
            data.volumeTrigger = cameraObject.transform;
        }

        private static void ConfigureVolume()
        {
            var profile = BuildPostFxProfile();

            var volumeObject = new GameObject("PostFX Volume");
            volumeObject.layer = 0;
            volumeObject.transform.position = Vector3.zero;

            var volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.sharedProfile = profile;
        }

        private static VolumeProfile BuildPostFxProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(PostFxProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, PostFxProfilePath);
            }
            else
            {
                // Rebuilds must not stack duplicate overrides on the saved profile.
                var stale = profile.components.ToArray();
                for (int i = 0; i < stale.Length; i++)
                {
                    profile.components.Remove(stale[i]);
                    if (stale[i] != null)
                    {
                        AssetDatabase.RemoveObjectFromAsset(stale[i]);
                        Object.DestroyImmediate(stale[i]);
                    }
                }
            }

            var tonemapping = AddVolumeOverride<Tonemapping>(profile);
            tonemapping.mode.Override(TonemappingMode.ACES);

            var color = AddVolumeOverride<ColorAdjustments>(profile);
            color.postExposure.Override(0.2f);
            color.contrast.Override(6f);
            color.saturation.Override(-2f);
            color.colorFilter.Override(new Color(1f, 0.985f, 0.955f));

            var whiteBalance = AddVolumeOverride<WhiteBalance>(profile);
            whiteBalance.temperature.Override(12f);
            whiteBalance.tint.Override(-2f);

            var bloom = AddVolumeOverride<Bloom>(profile);
            bloom.threshold.Override(1.05f);
            bloom.intensity.Override(0.35f);
            bloom.scatter.Override(0.7f);
            bloom.tint.Override(new Color(1f, 0.93f, 0.82f));

            var vignette = AddVolumeOverride<Vignette>(profile);
            vignette.intensity.Override(0.15f);
            vignette.smoothness.Override(0.45f);
            vignette.rounded.Override(false);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        private static T AddVolumeOverride<T>(VolumeProfile profile) where T : VolumeComponent
        {
            var component = ScriptableObject.CreateInstance<T>();
            component.name = typeof(T).Name;
            component.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            profile.components.Add(component);
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }

        /// <summary>
        /// The kit enables SSAO but with a 0.035 m radius, which is invisible at village
        /// scale. Widen it so buildings and props get real contact shadows.
        /// </summary>
        private static void ConfigureAmbientOcclusion()
        {
            var rendererData = AssetDatabase.LoadAssetAtPath<ScriptableObject>(RendererDataPath);
            if (rendererData == null)
            {
                return;
            }

            var serialized = new SerializedObject(rendererData);
            var features = serialized.FindProperty("m_RendererFeatures");
            if (features == null)
            {
                return;
            }

            for (int i = 0; i < features.arraySize; i++)
            {
                var feature = features.GetArrayElementAtIndex(i).objectReferenceValue as ScriptableRendererFeature;
                if (feature == null || feature.GetType().Name != "ScreenSpaceAmbientOcclusion")
                {
                    continue;
                }

                var settingsField = feature.GetType().GetField(
                    "m_Settings",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic);
                if (settingsField == null)
                {
                    continue;
                }

                var settings = settingsField.GetValue(feature);
                if (settings == null)
                {
                    continue;
                }

                SetSettingsMember(settings, "Intensity", 0.8f);
                SetSettingsMember(settings, "Radius", 0.4f);
                SetSettingsMember(settings, "DirectLightingStrength", 0.2f);

                var samplesField = settings.GetType().GetField(
                    "Samples",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic);
                if (samplesField != null && samplesField.FieldType.IsEnum)
                {
                    SetSettingsMember(
                        settings,
                        "Samples",
                        System.Enum.Parse(samplesField.FieldType, "Medium"));
                }

                // Settings may be a struct on older URP versions; write the boxed copy back.
                settingsField.SetValue(feature, settings);
                EditorUtility.SetDirty(feature);
                break;
            }

            serialized.ApplyModifiedProperties();
        }

        private static void SetSettingsMember(object settings, string name, object value)
        {
            var field = settings.GetType().GetField(
                name,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(settings, value);
            }
        }

        private static void ConfigurePipelineQuality()
        {
            var pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (pipeline == null)
            {
                return;
            }

            var serialized = new SerializedObject(pipeline);
            SetSerializedInt(serialized, "m_MSAA", 2); // 4x MSAA
            SetSerializedInt(serialized, "m_MainLightShadowmapResolution", 3); // 2048
            SetSerializedInt(serialized, "m_ShadowCascadeCount", 3);
            SetSerializedFloat(serialized, "m_ShadowDistance", 80f);
            SetSerializedBool(serialized, "m_SoftShadowsSupported", true);
            serialized.ApplyModifiedProperties();

            EditorUtility.SetDirty(pipeline);
        }

        private static void SetSerializedInt(SerializedObject serialized, string name, int value)
        {
            var property = serialized.FindProperty(name);
            if (property == null)
            {
                return;
            }

            if (property.propertyType == SerializedPropertyType.Enum)
            {
                property.enumValueIndex = value;
            }
            else
            {
                property.intValue = value;
            }
        }

        private static void SetSerializedFloat(SerializedObject serialized, string name, float value)
        {
            var property = serialized.FindProperty(name);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void SetSerializedBool(SerializedObject serialized, string name, bool value)
        {
            var property = serialized.FindProperty(name);
            if (property != null)
            {
                property.boolValue = value;
            }
        }

        /// <summary>
        /// Moves the kit's authored demo village into the target scene, skipping its showcase
        /// camera so the third-person rig owns the view. Render settings are copied because
        /// they live per scene, not per object.
        /// </summary>
        private static bool ImportVillageComposition(Scene target)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(KitDemoScenePath) == null)
            {
                Debug.LogWarning("[UnityMCPTest] Village kit demo scene missing: " + KitDemoScenePath);
                return false;
            }

            var demo = EditorSceneManager.OpenScene(KitDemoScenePath, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(demo);

            bool fog = RenderSettings.fog;
            FogMode fogMode = RenderSettings.fogMode;
            Color fogColor = RenderSettings.fogColor;
            float fogDensity = RenderSettings.fogDensity;
            float fogStart = RenderSettings.fogStartDistance;
            float fogEnd = RenderSettings.fogEndDistance;

            UnityEngine.Rendering.AmbientMode ambientMode = RenderSettings.ambientMode;
            Color ambientLight = RenderSettings.ambientLight;
            Color ambientSky = RenderSettings.ambientSkyColor;
            Color ambientEquator = RenderSettings.ambientEquatorColor;
            Color ambientGround = RenderSettings.ambientGroundColor;
            float ambientIntensity = RenderSettings.ambientIntensity;

            Material skybox = RenderSettings.skybox;
            Light sun = RenderSettings.sun;
            float reflectionIntensity = RenderSettings.reflectionIntensity;

            var villageRoots = new List<GameObject>();
            foreach (var root in demo.GetRootGameObjects())
            {
                // The demo camera would fight the third-person rig.
                if (root.name == "Camera")
                {
                    continue;
                }

                villageRoots.Add(root);
            }

            SceneManager.SetActiveScene(target);
            foreach (var root in villageRoots)
            {
                SceneManager.MoveGameObjectToScene(root, target);
            }

            EditorSceneManager.CloseScene(demo, true);

            RenderSettings.fog = fog;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;

            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientLight = ambientLight;
            RenderSettings.ambientSkyColor = ambientSky;
            RenderSettings.ambientEquatorColor = ambientEquator;
            RenderSettings.ambientGroundColor = ambientGround;
            RenderSettings.ambientIntensity = ambientIntensity;

            RenderSettings.skybox = skybox;
            RenderSettings.sun = sun;
            RenderSettings.reflectionIntensity = reflectionIntensity;

            Debug.Log("[UnityMCPTest] Imported " + villageRoots.Count + " village roots from " + KitDemoScenePath);
            return true;
        }

        private static void BuildFallbackGround()
        {
            var sun = TestCharacterBuilder.CreateSun(
                new Vector3(34f, -38f, 0f),
                new Color(1f, 0.9f, 0.74f, 1f),
                1.6f);
            RenderSettings.sun = sun;

            var kitGround = AssetDatabase.LoadAssetAtPath<GameObject>(KitGroundFallback);
            if (kitGround != null)
            {
                for (int x = -8; x <= 8; x++)
                {
                    for (int z = -8; z <= 8; z++)
                    {
                        var tile = (GameObject)PrefabUtility.InstantiatePrefab(kitGround);
                        tile.name = "Ground_" + x + "_" + z;
                        tile.transform.position = new Vector3(x * 5.4f, 0f, z * 5.4f);
                    }
                }

                return;
            }

            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Ground";
            plane.transform.localScale = new Vector3(30f, 1f, 30f);
            plane.GetComponent<MeshRenderer>().sharedMaterial =
                TestCharacterBuilder.CreateMaterial("VillageFallbackGround", new Color(0.31f, 0.39f, 0.24f, 1f));
        }

        private static Vector3 FindSpawn(Vector3 hint)
        {
            Physics.SyncTransforms();

            RaycastHit hit;
            var from = new Vector3(hint.x, 200f, hint.z);
            if (Physics.Raycast(from, Vector3.down, out hit, 500f, ~0, QueryTriggerInteraction.Ignore))
            {
                return new Vector3(hint.x, hit.point.y + 0.04f, hint.z);
            }

            Debug.LogWarning("[UnityMCPTest] No ground under spawn hint; using hint height.");
            return hint;
        }
    }
}
