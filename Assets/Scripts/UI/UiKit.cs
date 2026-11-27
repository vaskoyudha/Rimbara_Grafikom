using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Rimbara.UI
{
    [CreateAssetMenu(menuName = "RIMBARA/UI Kit", fileName = "FantasticUiKit")]
    public sealed class UiKit : ScriptableObject
    {
        [Header("Fonts")]
        public TMP_FontAsset HeadingFont;
        public TMP_FontAsset BodyFont;
        public Material TitleMaterial;
        public TMP_SpriteAsset SeparatorSprite;

        [Header("Colours")]
        public Color Ink = new Color32(0x1B, 0x14, 0x0E, 0xFF);
        public Color Cream = new Color32(0xFF, 0xF0, 0xCC, 0xFF);
        public Color Parchment = new Color32(0xF3, 0xE3, 0xC3, 0xFF);
        public Color Gold = new Color32(0xC9, 0x9A, 0x3F, 0xFF);
        public Color Success = new Color32(0x6F, 0x9E, 0x4A, 0xFF);
        public Color Danger = new Color32(0xB4, 0x4A, 0x3A, 0xFF);
        public Color Muted = new Color32(0xA8, 0x94, 0x72, 0xFF);

        [Header("Windows")]
        public Sprite Window;
        public Sprite ParchmentPanel;
        public Sprite Popup;
        public Sprite Tooltip;
        public Sprite Inset;
        public Sprite CardInset;
        public Sprite RowInset;
        public Sprite Divider;
        public Sprite Ribbon;

        [Header("Radar Map")]
        public Sprite WorldMap;
        public Sprite RadarBezel;
        public Sprite RadarChevron;
        public Sprite RadarFov;
        public Sprite RadarBlipDiamond;
        public Sprite RadarBlipDot;
        public Vector2 WorldMin = new Vector2(-60f, -55f);
        public Vector2 WorldSize = new Vector2(120f, 120f);

        [Header("Zone Thumbnails")]
        public Sprite ZoneAThumbnail;
        public Sprite ZoneBThumbnail;
        public Sprite ZoneCThumbnail;

        [Header("Buttons")]
        public Sprite ButtonPrimaryNormal;
        public Sprite ButtonPrimaryHover;
        public Sprite ButtonPrimaryPressed;
        public Sprite ButtonPrimaryDisabled;
        public Sprite ButtonSecondaryNormal;
        public Sprite ButtonSecondaryHover;
        public Sprite ButtonSecondaryPressed;
        public Sprite ButtonSecondaryDisabled;

        [Header("Bars")]
        public Sprite BarFrame;
        public Sprite BarHealthFill;
        public Sprite BarManaFill;
        public Sprite BarXpFill;
        public Sprite BarStaminaFill;

        [Header("Controls")]
        public Sprite SliderTrack;
        public Sprite SliderFill;
        public Sprite SliderHandle;
        public Sprite ScrollbarTrack;
        public Sprite ScrollbarHandle;
        public Sprite ToggleOn;
        public Sprite ToggleOff;
        public Sprite CheckboxOn;
        public Sprite CheckboxOff;
        public Sprite Dropdown;
        public Sprite InputField;

        [Header("Slots")]
        public Sprite SlotEmpty;
        public Sprite SlotSelected;
        public Sprite SlotLocked;

        [Header("Icons")]
        public Sprite IconQuest;
        public Sprite IconCompass;
        public Sprite IconBag;
        public Sprite IconCleanup;
        public Sprite IconEmber;

        public bool IsComplete => MissingRoles().Count == 0;

        public List<string> MissingRoles()
        {
            var missing = new List<string>();
            void Need(string name, Object value) { if (value == null) missing.Add(name); }
            Need(nameof(HeadingFont), HeadingFont);
            Need(nameof(BodyFont), BodyFont);
            Need(nameof(Window), Window);
            Need(nameof(ParchmentPanel), ParchmentPanel);
            Need(nameof(Popup), Popup);
            Need(nameof(Tooltip), Tooltip);
            Need(nameof(ButtonPrimaryNormal), ButtonPrimaryNormal);
            Need(nameof(ButtonPrimaryHover), ButtonPrimaryHover);
            Need(nameof(ButtonPrimaryPressed), ButtonPrimaryPressed);
            Need(nameof(ButtonSecondaryNormal), ButtonSecondaryNormal);
            Need(nameof(ButtonSecondaryHover), ButtonSecondaryHover);
            Need(nameof(ButtonSecondaryPressed), ButtonSecondaryPressed);
            Need(nameof(ButtonPrimaryDisabled), ButtonPrimaryDisabled);
            Need(nameof(ButtonSecondaryDisabled), ButtonSecondaryDisabled);
            Need(nameof(BarFrame), BarFrame);
            Need(nameof(BarHealthFill), BarHealthFill);
            Need(nameof(BarManaFill), BarManaFill);
            Need(nameof(BarXpFill), BarXpFill);
            Need(nameof(SliderTrack), SliderTrack);
            Need(nameof(SliderFill), SliderFill);
            Need(nameof(SliderHandle), SliderHandle);
            Need(nameof(SlotEmpty), SlotEmpty);
            Need(nameof(Inset), Inset);
            return missing;
        }
    }
}
