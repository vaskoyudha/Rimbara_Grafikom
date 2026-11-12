using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityMCPTest.Editor
{
    public static class TestCharacterBuilder
    {
        private const string Root = "Assets/UnityMCPTest";
        private const string FbxPath = Root + "/Characters/Mixamo/YBot.fbx";
        private const string MixamoDir = Root + "/Animations/Mixamo";
        private const string AnimDir = Root + "/Animations";
        private const string MatDir = Root + "/Materials";
        private const string SkinsDir = MatDir + "/Skins";
        private const string SceneDir = Root + "/Scenes";
        private const string ScenePath = SceneDir + "/ThirdPersonTest.unity";
        private const string ControllerPath = AnimDir + "/ThirdPersonTest.controller";

        private const float CharacterHeight = 1.8f;

        [MenuItem("UnityMCPTest/Build Third Person Test Scene")]
        public static void Build()
        {
            EnsureFolder(SceneDir);
            EnsureFolder(AnimDir);
            EnsureFolder(MatDir);
            EnsureFolder(SkinsDir);
            EnsureFolder(MixamoDir);

            ConfigureCharacterHumanoid();

            var idle = LoadMixamoClip("Idle.fbx", true);
            var walk = LoadMixamoClip("Walk.fbx", true);
            var run = LoadMixamoClip("Run.fbx", true);
            var jab = LoadMixamoClip("Jab.fbx", false);
            var cross = LoadMixamoClip("Cross.fbx", false);
            var uppercut = LoadMixamoClip("Uppercut.fbx", false);
            var fall = LoadMixamoClip("FallIdle.fbx", true);
            var landing = LoadMixamoClip("Landing.fbx", false, 16, 48);

            var controller = BuildController(idle, walk, run, jab, cross, uppercut, fall, landing);
            var skins = BuildSkinMaterials();
            BuildScene(controller, skins);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[UnityMCPTest] Third-person test scene built at " + ScenePath);
        }

        internal static void ConfigureCharacterHumanoid()
        {
            var importer = AssetImporter.GetAtPath(FbxPath) as ModelImporter;
            if (importer == null)
            {
                throw new FileNotFoundException("Character FBX not found: " + FbxPath);
            }

            bool dirty = false;
            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                dirty = true;
            }

            if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
            {
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                dirty = true;
            }

            if (importer.importAnimation)
            {
                importer.importAnimation = false;
                dirty = true;
            }

            if (dirty)
            {
                importer.SaveAndReimport();
            }

            var avatar = AssetDatabase.LoadAllAssetsAtPath(FbxPath).OfType<Avatar>().FirstOrDefault();
            if (avatar == null || !avatar.isValid || !avatar.isHuman)
            {
                Debug.LogError("[UnityMCPTest] Mixamo character did not produce a valid Humanoid avatar. " +
                               "Check the rig import settings for " + FbxPath);
            }
            else
            {
                Debug.Log("[UnityMCPTest] Mixamo character Humanoid avatar ready: " + avatar.name);
            }
        }

        internal static AnimationClip LoadMixamoClip(
            string fileName,
            bool loop,
            int firstFrame = -1,
            int lastFrame = -1)
        {
            string path = MixamoDir + "/" + fileName;
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                throw new FileNotFoundException("Mixamo FBX not found: " + path);
            }

            bool dirty = false;
            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                dirty = true;
            }

            if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
            {
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                dirty = true;
            }

            if (!importer.importAnimation)
            {
                importer.importAnimation = true;
                dirty = true;
            }

            string clipName = Path.GetFileNameWithoutExtension(fileName);
            var clips = importer.clipAnimations.Length > 0
                ? importer.clipAnimations
                : importer.defaultClipAnimations;

            foreach (var clip in clips)
            {
                if (clip.name != clipName)
                {
                    clip.name = clipName;
                    dirty = true;
                }

                if (clip.loopTime != loop)
                {
                    clip.loopTime = loop;
                    dirty = true;
                }

                if (firstFrame >= 0 && !Mathf.Approximately(clip.firstFrame, firstFrame))
                {
                    clip.firstFrame = firstFrame;
                    dirty = true;
                }

                if (lastFrame >= 0 && !Mathf.Approximately(clip.lastFrame, lastFrame))
                {
                    clip.lastFrame = lastFrame;
                    dirty = true;
                }
            }

            if (clips.Length > 0)
            {
                importer.clipAnimations = clips;
            }

            if (dirty)
            {
                importer.SaveAndReimport();
            }

            var animation = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .Where(clip => !clip.name.StartsWith("__preview__"))
                .OrderByDescending(clip => clip.length)
                .FirstOrDefault();

            if (animation == null)
            {
                throw new FileNotFoundException("No AnimationClip inside " + path);
            }

            Debug.Log("[UnityMCPTest] Mixamo clip " + fileName + ": '" + animation.name +
                      "' length=" + animation.length.ToString("F2") + "s loop=" + loop);
            return animation;
        }

        internal static AnimatorController BuildController(
            AnimationClip idle,
            AnimationClip walk,
            AnimationClip run,
            AnimationClip jab,
            AnimationClip cross,
            AnimationClip uppercut,
            AnimationClip fall,
            AnimationClip landing)
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
            {
                AssetDatabase.DeleteAsset(ControllerPath);
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("AttackL", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("AttackR", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Heavy", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("AttackSpeed", AnimatorControllerParameterType.Float);
            var parameters = controller.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].name == "Grounded")
                {
                    parameters[i].defaultBool = true;
                }
                else if (parameters[i].name == "AttackSpeed")
                {
                    parameters[i].defaultFloat = 1f;
                }
            }

            controller.parameters = parameters;

            var machine = controller.layers[0].stateMachine;

            var locomotion = machine.AddState("Locomotion");
            machine.defaultState = locomotion;

            var blend = new BlendTree
            {
                name = "LocomotionBlend",
                blendType = BlendTreeType.Simple1D,
                blendParameter = "Speed",
                useAutomaticThresholds = false
            };
            AssetDatabase.AddObjectToAsset(blend, controller);
            blend.AddChild(idle, 0f);
            blend.AddChild(walk, 1.8f);
            blend.AddChild(run, 4.5f);

            // Match clip cadence to controller speed so planted feet do not slide.
            // Measured from the clips: Walk covers 1.64 m/s naturally, Run 3.33 m/s.
            var blendChildren = blend.children;
            blendChildren[1].timeScale = 1.09f;
            blendChildren[2].timeScale = 1.35f;
            blend.children = blendChildren;

            locomotion.motion = blend;

            var punchLeftState = machine.AddState("Punch Left");
            punchLeftState.motion = jab;
            punchLeftState.speed = 1.15f;
            UseAttackSpeed(punchLeftState);

            var punchRightState = machine.AddState("Punch Right");
            punchRightState.motion = cross;
            punchRightState.speed = 1.25f;
            UseAttackSpeed(punchRightState);

            var heavyState = machine.AddState("Heavy");
            heavyState.motion = uppercut;
            heavyState.speed = 1.15f;
            UseAttackSpeed(heavyState);

            var airState = machine.AddState("Air");
            airState.motion = fall;

            var landState = machine.AddState("Land");
            landState.motion = landing;

            AddAnyStateTrigger(machine, punchLeftState, "AttackL");
            AddAnyStateTrigger(machine, punchRightState, "AttackR");
            AddAnyStateTrigger(machine, heavyState, "Heavy");

            var toAir = machine.AddAnyStateTransition(airState);
            toAir.hasExitTime = false;
            toAir.exitTime = 0f;
            toAir.duration = 0.16f;
            toAir.hasFixedDuration = true;
            toAir.canTransitionToSelf = false;
            toAir.AddCondition(AnimatorConditionMode.IfNot, 0f, "Grounded");

            var toLand = airState.AddTransition(landState);
            toLand.hasExitTime = false;
            toLand.exitTime = 0f;
            toLand.duration = 0.15f;
            toLand.hasFixedDuration = true;
            toLand.AddCondition(AnimatorConditionMode.If, 0f, "Grounded");

            AddReturnTransition(punchLeftState, locomotion, 0.88f);
            AddReturnTransition(punchRightState, locomotion, 0.88f);
            AddReturnTransition(heavyState, locomotion, 0.88f);
            AddReturnTransition(landState, locomotion, 0.9f);

            return controller;
        }

        private static void UseAttackSpeed(AnimatorState state)
        {
            state.speedParameterActive = true;
            state.speedParameter = "AttackSpeed";
        }

        private static void AddAnyStateTrigger(
            AnimatorStateMachine machine,
            AnimatorState target,
            string parameter)
        {
            var transition = machine.AddAnyStateTransition(target);
            transition.hasExitTime = false;
            transition.exitTime = 0f;
            transition.duration = 0.12f;
            transition.hasFixedDuration = true;
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.If, 0f, parameter);
        }

        private static void AddReturnTransition(AnimatorState from, AnimatorState to, float exitTime)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = true;
            transition.exitTime = exitTime;
            transition.duration = 0.15f;
            transition.hasFixedDuration = true;
        }

        internal static SkinPreset[] BuildSkinMaterials()
        {
            var materials = AssetDatabase.LoadAllAssetsAtPath(FbxPath).OfType<Material>().ToArray();
            var bodyBase = materials.FirstOrDefault(material => material.name == "Alpha_Body_MAT");
            var jointBase = materials.FirstOrDefault(material => material.name == "Alpha_Joints_MAT");
            if (bodyBase == null || jointBase == null)
            {
                throw new FileNotFoundException("Y Bot materials not found inside " + FbxPath);
            }

            var specs = new[]
            {
                new SkinSpec { Name = "Cobalt", Body = new Color(0.18f, 0.55f, 0.95f), Joints = new Color(0.34f, 0.42f, 0.50f), Accent = new Color(0.20f, 0.70f, 1.00f), GlowJoints = false },
                new SkinSpec { Name = "Crimson", Body = new Color(0.72f, 0.12f, 0.14f), Joints = new Color(0.22f, 0.20f, 0.20f), Accent = new Color(1.00f, 0.28f, 0.20f), GlowJoints = false },
                new SkinSpec { Name = "Jade", Body = new Color(0.10f, 0.58f, 0.44f), Joints = new Color(0.78f, 0.76f, 0.66f), Accent = new Color(0.20f, 1.00f, 0.70f), GlowJoints = false },
                new SkinSpec { Name = "Amber", Body = new Color(0.92f, 0.52f, 0.10f), Joints = new Color(0.30f, 0.22f, 0.14f), Accent = new Color(1.00f, 0.75f, 0.20f), GlowJoints = false },
                new SkinSpec { Name = "Obsidian", Body = new Color(0.06f, 0.07f, 0.09f), Joints = new Color(0.10f, 0.55f, 0.85f), Accent = new Color(0.10f, 0.80f, 1.00f), GlowJoints = true }
            };

            var presets = new SkinPreset[specs.Length];
            for (int i = 0; i < specs.Length; i++)
            {
                var spec = specs[i];
                presets[i] = new SkinPreset
                {
                    name = spec.Name,
                    bodyMaterial = CreateSkinMaterial("Skin_" + spec.Name + "_Body", bodyBase, spec.Body, Color.black),
                    jointMaterial = CreateSkinMaterial("Skin_" + spec.Name + "_Joints", jointBase, spec.Joints, spec.GlowJoints ? spec.Accent * 1.6f : Color.black),
                    gearMaterial = CreateSkinMaterial("Skin_" + spec.Name + "_Gear", bodyBase, spec.Accent, spec.Accent * 0.35f)
                };
            }

            return presets;
        }

        private static Material CreateSkinMaterial(string name, Material source, Color baseColor, Color emission)
        {
            string path = SkinsDir + "/" + name + ".mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            var material = new Material(source)
            {
                name = name
            };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", baseColor);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", baseColor);
            }

            if (emission.maxColorComponent > 0.001f)
            {
                material.EnableKeyword("_EMISSION");
                if (material.HasProperty("_EmissionColor"))
                {
                    material.SetColor("_EmissionColor", emission);
                }

                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                if (material.HasProperty("_EmissionColor"))
                {
                    material.SetColor("_EmissionColor", Color.black);
                }
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static AttributePreset[] CreateAttributePresets()
        {
            return new[]
            {
                new AttributePreset { name = "Balanced", walkSpeed = 1.8f, runSpeed = 4.5f, jumpHeight = 1.1f, turnSpeed = 12f, acceleration = 16f, attackSpeed = 1f },
                new AttributePreset { name = "Swift", walkSpeed = 2.2f, runSpeed = 4.9f, jumpHeight = 1.3f, turnSpeed = 16f, acceleration = 22f, attackSpeed = 1.25f },
                new AttributePreset { name = "Power", walkSpeed = 1.5f, runSpeed = 3.8f, jumpHeight = 0.95f, turnSpeed = 9f, acceleration = 12f, attackSpeed = 0.85f }
            };
        }

        private class SkinSpec
        {
            public string Name;
            public Color Body;
            public Color Joints;
            public Color Accent;
            public bool GlowJoints;
        }

        private static void CreateGear(
            CharacterCustomizer customizer,
            Animator animator,
            Material material,
            Transform root)
        {
            Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
            Transform chest = animator.GetBoneTransform(HumanBodyBones.Chest);
            Transform leftArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            Transform rightArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);

            customizer.headband = CreateHeadband(head, material);

            // Gear is parented to animated bones, so it must sit at the bone itself.
            // Fixed world-space heights drift once the idle pose rotates the bone away
            // from the rest pose the offsets were authored against.
            customizer.chestEmblem = CreateGearPart(
                "ChestEmblem",
                chest,
                chest.position + root.forward * 0.14f,
                new Vector3(0.09f, 0.09f, 0.03f),
                root.rotation * Quaternion.Euler(0f, 0f, 45f),
                material,
                PrimitiveType.Cube);
            customizer.leftPauldron = CreateGearPart(
                "LeftPauldron",
                leftArm,
                leftArm.position + root.rotation * new Vector3(-0.03f, 0.04f, 0f),
                new Vector3(0.16f, 0.12f, 0.18f),
                root.rotation,
                material,
                PrimitiveType.Sphere);
            customizer.rightPauldron = CreateGearPart(
                "RightPauldron",
                rightArm,
                rightArm.position + root.rotation * new Vector3(0.03f, 0.04f, 0f),
                new Vector3(0.16f, 0.12f, 0.18f),
                root.rotation,
                material,
                PrimitiveType.Sphere);
        }

        private static GameObject CreateHeadband(Transform head, Material material)
        {
            if (head == null)
            {
                return null;
            }

            var root = new GameObject("Headband");
            root.transform.SetParent(head, true);
            root.transform.position = head.position + new Vector3(0f, 0.096f, 0f);
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one / head.lossyScale.x;

            const int segments = 10;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                var segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segment.name = "Headband_" + i;
                segment.transform.SetParent(root.transform, false);
                segment.transform.localPosition = new Vector3(Mathf.Sin(angle) * 0.09f, 0f, Mathf.Cos(angle) * 0.112f);
                segment.transform.localRotation = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f);
                segment.transform.localScale = new Vector3(0.062f, 0.034f, 0.022f);

                var segmentCollider = segment.GetComponent<Collider>();
                if (segmentCollider != null)
                {
                    Object.DestroyImmediate(segmentCollider);
                }

                var segmentRenderer = segment.GetComponent<MeshRenderer>();
                if (segmentRenderer != null && material != null)
                {
                    segmentRenderer.sharedMaterial = material;
                }
            }

            return root;
        }

        private static GameObject CreateGearPart(
            string name,
            Transform parent,
            Vector3 worldPosition,
            Vector3 worldScale,
            Quaternion worldRotation,
            Material material,
            PrimitiveType primitiveType)
        {
            if (parent == null)
            {
                return null;
            }

            var part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.transform.SetParent(parent, true);
            part.transform.position = worldPosition;
            part.transform.rotation = worldRotation;
            part.transform.localScale = new Vector3(
                worldScale.x / parent.lossyScale.x,
                worldScale.y / parent.lossyScale.y,
                worldScale.z / parent.lossyScale.z);

            var collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            var renderer = part.GetComponent<MeshRenderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            return part;
        }

        private static void BuildScene(AnimatorController controller, SkinPreset[] skins)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var groundMaterial = CreateMaterial("Ground", new Color(0.26f, 0.3f, 0.28f, 1f));
            var blockMaterial = CreateMaterial("Blocks", new Color(0.75f, 0.42f, 0.16f, 1f));
            var platformMaterial = CreateMaterial("Platform", new Color(0.34f, 0.38f, 0.42f, 1f));

            CreateSun(new Vector3(50f, -30f, 0f), new Color(1f, 0.95f, 0.88f, 1f), 1.35f);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(6f, 1f, 6f);
            ground.GetComponent<MeshRenderer>().sharedMaterial = groundMaterial;

            CreateBlock("Block_A", new Vector3(-4f, 0.75f, 3.5f), new Vector3(1.5f, 1.5f, 1.5f), blockMaterial);
            CreateBlock("Block_B", new Vector3(4.5f, 1.25f, 5.5f), new Vector3(2f, 2.5f, 2f), blockMaterial);
            CreateBlock("Block_C", new Vector3(6f, 0.5f, -2f), new Vector3(2.5f, 1f, 2.5f), blockMaterial);
            CreateBlock("Platform", new Vector3(-6f, 0.25f, -5f), new Vector3(4f, 0.5f, 4f), platformMaterial);

            // A wall behind the spawn point gives the camera collision something to react to.
            CreateBlock("CameraWall", new Vector3(0f, 2f, -5f), new Vector3(6f, 4f, 0.4f), platformMaterial);

            CreatePlayer(controller, skins, new Vector3(0f, 0.02f, 0f), Quaternion.identity);

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        internal static Light CreateSun(Vector3 eulerAngles, Color color, float intensity)
        {
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(eulerAngles);
            return light;
        }

        internal static GameObject CreatePlayer(
            AnimatorController controller,
            SkinPreset[] skins,
            Vector3 position,
            Quaternion rotation)
        {
            var player = new GameObject("Player");
            player.transform.position = position;
            player.transform.rotation = rotation;

            var characterController = player.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.radius = 0.35f;
            characterController.center = new Vector3(0f, 0.9f, 0f);
            characterController.slopeLimit = 45f;
            characterController.stepOffset = 0.3f;
            characterController.skinWidth = 0.02f;

            var modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
            var model = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
            model.name = "CharacterModel";
            model.transform.SetParent(player.transform, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            Bounds rawBounds = ComputeBounds(model);
            float uniformScale = rawBounds.size.y > 0.01f ? CharacterHeight / rawBounds.size.y : 1f;
            model.transform.localScale = Vector3.one * uniformScale;

            Bounds bounds = ComputeBounds(model);
            // ComputeBounds is world-space; convert the feet correction into player-local space
            // so the model still stands on the ground when the spawn point is not at y = 0.
            float feetOffset = player.transform.position.y - bounds.min.y;
            model.transform.localPosition = new Vector3(
                0f,
                model.transform.localPosition.y + feetOffset / player.transform.lossyScale.y,
                0f);

            var animator = model.GetComponent<Animator>();
            if (animator == null)
            {
                animator = model.AddComponent<Animator>();
            }

            var avatar = AssetDatabase.LoadAllAssetsAtPath(FbxPath).OfType<Avatar>().FirstOrDefault();
            if (avatar != null)
            {
                animator.avatar = avatar;
            }

            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.1f;
            cameraGo.AddComponent<AudioListener>();

            var controllerScript = player.AddComponent<ThirdPersonController>();
            controllerScript.animator = animator;
            controllerScript.cameraTransform = cameraGo.transform;

            var customizer = player.AddComponent<CharacterCustomizer>();
            customizer.controller = controllerScript;
            customizer.bodyRenderer = model.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .FirstOrDefault(renderer => renderer.name == "Alpha_Surface");
            customizer.jointRenderer = model.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .FirstOrDefault(renderer => renderer.name == "Alpha_Joints");
            customizer.skins = skins;
            customizer.presets = CreateAttributePresets();
            customizer.defaultSkin = 0;
            customizer.defaultPreset = 0;
            customizer.defaultGear = 2;
            customizer.defaultJoints = true;

            if (skins.Length > 0)
            {
                if (customizer.bodyRenderer != null)
                {
                    customizer.bodyRenderer.sharedMaterial = skins[0].bodyMaterial;
                }

                if (customizer.jointRenderer != null)
                {
                    customizer.jointRenderer.sharedMaterial = skins[0].jointMaterial;
                }
            }

            CreateGear(customizer, animator, skins.Length > 0 ? skins[0].gearMaterial : null, player.transform);

            return player;
        }

        private static void CreateBlock(string name, Vector3 position, Vector3 scale, Material material)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.position = position;
            block.transform.localScale = scale;
            block.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static Bounds ComputeBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(root.transform.position, Vector3.one);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        internal static Material CreateMaterial(string name, Color color)
        {
            string path = MatDir + "/" + name + ".mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader)
            {
                name = name
            };

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        internal static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
