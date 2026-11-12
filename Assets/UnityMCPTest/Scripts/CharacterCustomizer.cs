using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityMCPTest
{
    [Serializable]
    public class SkinPreset
    {
        public string name;
        public Material bodyMaterial;
        public Material jointMaterial;
        public Material gearMaterial;
    }

    [Serializable]
    public class AttributePreset
    {
        public string name;
        public float walkSpeed = 1.8f;
        public float runSpeed = 4.5f;
        public float jumpHeight = 1.1f;
        public float turnSpeed = 12f;
        public float acceleration = 16f;
        public float attackSpeed = 1f;
    }

    public class CharacterCustomizer : MonoBehaviour
    {
        [Header("References")]
        public ThirdPersonController controller;
        public SkinnedMeshRenderer bodyRenderer;
        public SkinnedMeshRenderer jointRenderer;

        [Header("Skins")]
        public SkinPreset[] skins;
        public int defaultSkin;

        [Header("Attribute Presets")]
        public AttributePreset[] presets;
        public int defaultPreset;

        [Header("Gear")]
        public GameObject headband;
        public GameObject leftPauldron;
        public GameObject rightPauldron;
        public GameObject chestEmblem;
        public int defaultGear = 2;
        public bool defaultJoints = true;

        private const string SkinKey = "UnityMCPTest.Skin";
        private const string PresetKey = "UnityMCPTest.Preset";
        private const string GearKey = "UnityMCPTest.Gear";
        private const string JointsKey = "UnityMCPTest.Joints";

        private int skinIndex;
        private int presetIndex;
        private int gearIndex;
        private bool jointsVisible = true;

        private void Start()
        {
            if (controller == null)
            {
                controller = GetComponent<ThirdPersonController>();
            }

            skinIndex = ClampIndex(PlayerPrefs.GetInt(SkinKey, defaultSkin), skins);
            presetIndex = ClampIndex(PlayerPrefs.GetInt(PresetKey, defaultPreset), presets);
            gearIndex = Mathf.Clamp(PlayerPrefs.GetInt(GearKey, defaultGear), 0, 2);
            jointsVisible = PlayerPrefs.GetInt(JointsKey, defaultJoints ? 1 : 0) == 1;

            ApplySkin();
            ApplyPreset();
            ApplyGear();
            ApplyJoints();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.digit1Key.wasPressedThisFrame) SelectSkin(0);
            if (keyboard.digit2Key.wasPressedThisFrame) SelectSkin(1);
            if (keyboard.digit3Key.wasPressedThisFrame) SelectSkin(2);
            if (keyboard.digit4Key.wasPressedThisFrame) SelectSkin(3);
            if (keyboard.digit5Key.wasPressedThisFrame) SelectSkin(4);
            if (keyboard.jKey.wasPressedThisFrame) ToggleJoints();
            if (keyboard.gKey.wasPressedThisFrame) NextGear();
            if (keyboard.f1Key.wasPressedThisFrame) SelectPreset(0);
            if (keyboard.f2Key.wasPressedThisFrame) SelectPreset(1);
            if (keyboard.f3Key.wasPressedThisFrame) SelectPreset(2);
        }

        public void SelectSkin(int index)
        {
            if (skins == null || skins.Length == 0)
            {
                return;
            }

            skinIndex = Mathf.Clamp(index, 0, skins.Length - 1);
            ApplySkin();
            PlayerPrefs.SetInt(SkinKey, skinIndex);
            PlayerPrefs.Save();
        }

        public void NextSkin()
        {
            if (skins == null || skins.Length == 0)
            {
                return;
            }

            SelectSkin((skinIndex + 1) % skins.Length);
        }

        public void SelectPreset(int index)
        {
            if (presets == null || presets.Length == 0)
            {
                return;
            }

            presetIndex = Mathf.Clamp(index, 0, presets.Length - 1);
            ApplyPreset();
            PlayerPrefs.SetInt(PresetKey, presetIndex);
            PlayerPrefs.Save();
        }

        public void NextPreset()
        {
            if (presets == null || presets.Length == 0)
            {
                return;
            }

            SelectPreset((presetIndex + 1) % presets.Length);
        }

        public void NextGear()
        {
            gearIndex = (gearIndex + 1) % 3;
            ApplyGear();
            PlayerPrefs.SetInt(GearKey, gearIndex);
            PlayerPrefs.Save();
        }

        public void ToggleJoints()
        {
            jointsVisible = !jointsVisible;
            ApplyJoints();
            PlayerPrefs.SetInt(JointsKey, jointsVisible ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void ApplySkin()
        {
            if (skins == null || skins.Length == 0)
            {
                return;
            }

            var skin = skins[Mathf.Clamp(skinIndex, 0, skins.Length - 1)];
            if (bodyRenderer != null && skin.bodyMaterial != null)
            {
                bodyRenderer.sharedMaterial = skin.bodyMaterial;
            }

            if (jointRenderer != null && skin.jointMaterial != null)
            {
                jointRenderer.sharedMaterial = skin.jointMaterial;
            }

            SetGearMaterial(headband, skin.gearMaterial);
            SetGearMaterial(leftPauldron, skin.gearMaterial);
            SetGearMaterial(rightPauldron, skin.gearMaterial);
            SetGearMaterial(chestEmblem, skin.gearMaterial);
        }

        private void ApplyPreset()
        {
            if (controller == null || presets == null || presets.Length == 0)
            {
                return;
            }

            var preset = presets[Mathf.Clamp(presetIndex, 0, presets.Length - 1)];
            controller.walkSpeed = preset.walkSpeed;
            controller.runSpeed = preset.runSpeed;
            controller.jumpHeight = preset.jumpHeight;
            controller.turnSpeed = preset.turnSpeed;
            controller.acceleration = preset.acceleration;
            controller.attackSpeed = preset.attackSpeed;
        }

        private void ApplyGear()
        {
            bool light = gearIndex >= 1;
            bool full = gearIndex >= 2;

            SetActive(headband, light);
            SetActive(chestEmblem, light);
            SetActive(leftPauldron, full);
            SetActive(rightPauldron, full);
        }

        private void ApplyJoints()
        {
            if (jointRenderer != null)
            {
                jointRenderer.enabled = jointsVisible;
            }
        }

        private void SetGearMaterial(GameObject part, Material material)
        {
            if (part == null || material == null)
            {
                return;
            }

            var renderers = part.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sharedMaterial = material;
            }
        }

        private static void SetActive(GameObject part, bool active)
        {
            if (part != null && part.activeSelf != active)
            {
                part.SetActive(active);
            }
        }

        private static int ClampIndex(int index, Array array)
        {
            if (array == null || array.Length == 0)
            {
                return 0;
            }

            return Mathf.Clamp(index, 0, array.Length - 1);
        }

        private void OnGUI()
        {
            string skinName = skins != null && skins.Length > 0 ? skins[Mathf.Clamp(skinIndex, 0, skins.Length - 1)].name : "None";
            string presetName = presets != null && presets.Length > 0 ? presets[Mathf.Clamp(presetIndex, 0, presets.Length - 1)].name : "None";
            string[] gearNames = { "Off", "Light", "Full" };
            string gearName = gearNames[Mathf.Clamp(gearIndex, 0, gearNames.Length - 1)];

            GUI.Box(new Rect(10f, 42f, 640f, 44f), string.Empty);
            GUI.Label(
                new Rect(18f, 47f, 620f, 20f),
                "Skin: " + skinName + "  |  Preset: " + presetName + "  |  Gear: " + gearName + "  |  Joints: " + (jointsVisible ? "on" : "off"));
            GUI.Label(
                new Rect(18f, 65f, 620f, 20f),
                "1-5 skin  |  J joints  |  G gear  |  F1 Balanced  F2 Swift  F3 Power");
        }
    }
}
