using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rimbara.UI
{
    public enum HudSurface { Window, Parchment, Popup, Tooltip, Inset }

    public enum HudFill { Restoration, Water }

    public static class HudTheme
    {
        private static readonly Dictionary<string, HudSurface> Surfaces = new Dictionary<string, HudSurface>
        {
            { "Mission card", HudSurface.Window },
            { "Restoration counters", HudSurface.Window },
            { "Next destination", HudSurface.Inset },
            { "Interaction prompt", HudSurface.Window },
            { "Feedback toast", HudSurface.Tooltip },
            { "Controls reminder", HudSurface.Inset },
            { "Arunika field map", HudSurface.Window },
            { "Main menu card", HudSurface.Window },
            { "Volunteer briefing", HudSurface.Window },
            { "How to play card", HudSurface.Window },
            { "Pause card", HudSurface.Window },
            { "Restoration ending", HudSurface.Window },
            { "Coordinator dialogue", HudSurface.Window }
        };

        private static readonly HashSet<string> PrimaryButtons = new HashSet<string>
        {
            "Start restoration", "Enter Arunika", "Resume mission", "Play again", "Help back", "Dialogue next"
        };

        private static readonly HashSet<string> GoldText = new HashSet<string>
        {
            "Menu eyebrow", "Briefing eyebrow", "Help eyebrow", "Pause eyebrow", "Victory eyebrow",
            "Waypoint instruction", "Briefing targets", "Menu scope", "Restoration percent"
        };

        private static readonly HashSet<string> SuccessText = new HashSet<string>
        {
            "RIMBARA title", "Mission title", "Briefing title", "Help title", "Pause title", "Victory title"
        };

        private static readonly HashSet<string> MutedText = new HashSet<string>
        {
            "Mission number", "Map title", "Volume label", "Menu footer",
            "No fail promise", "Help tip", "Game subtitle"
        };

        private static readonly HashSet<string> CenteredText = new HashSet<string>
        {
            "Menu eyebrow", "Briefing eyebrow", "Help eyebrow", "Pause eyebrow", "Victory eyebrow",
            "RIMBARA title", "Briefing title", "Help title", "Pause title", "Victory title",
            "Game subtitle", "Menu scope"
        };

        private static readonly HashSet<string> TitleText = new HashSet<string>
        {
            "RIMBARA title", "Briefing title", "Help title", "Pause title", "Victory title"
        };

        public static void Apply(Canvas canvas, UiKit kit)
        {
            if (canvas == null || kit == null) return;

            IndonesianText.ApplyStaticLabels(canvas.transform);

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(HudLayout.ReferenceWidth, HudLayout.ReferenceHeight);
                scaler.matchWidthOrHeight = 0.5f;
            }

            EnsureSafeArea(canvas);

            foreach (Image image in canvas.GetComponentsInChildren<Image>(true))
            {
                if (Surfaces.TryGetValue(image.name, out HudSurface surface))
                {
                    ApplyPanel(image, kit, surface);
                    continue;
                }
                if (image.name == "Volume background")
                {
                    image.sprite = kit.SliderTrack;
                    image.type = Image.Type.Sliced;
                    image.color = Color.white;
                }
                else if (image.name == "Volume handle")
                {
                    image.sprite = kit.SliderHandle;
                    image.type = Image.Type.Simple;
                    image.color = Color.white;
                }
                else if (image.name == "Volume filled")
                {
                    image.sprite = kit.SliderFill;
                    image.type = Image.Type.Filled;
                    image.fillMethod = Image.FillMethod.Horizontal;
                    image.color = Color.white;
                }
                else if (image.name == "Restoration bar fill")
                {
                    ApplyFill(image, kit, HudFill.Restoration);
                }
                else if (image.name == "Water contact fill")
                {
                    ApplyFill(image, kit, HudFill.Water);
                }
            }

            foreach (Button button in canvas.GetComponentsInChildren<Button>(true))
            {
                Image image = button.GetComponent<Image>();
                if (image != null) ApplyButton(button, image, kit, PrimaryButtons.Contains(button.name));
            }

            foreach (TMP_Text text in canvas.GetComponentsInChildren<TMP_Text>(true))
            {
                bool isButtonLabel = text.name.EndsWith(" label") && text.transform.parent != null
                    && text.transform.parent.GetComponent<Button>() != null;
                if (isButtonLabel)
                {
                    text.font = kit.HeadingFont;
                    text.fontStyle = FontStyles.SmallCaps;
                    text.color = kit.Cream;
                    continue;
                }

                bool heading = text.fontStyle.HasFlag(FontStyles.Bold) || text.fontSize >= 28f;
                text.font = heading ? kit.HeadingFont : kit.BodyFont;
                if (kit.SeparatorSprite != null) text.spriteAsset = kit.SeparatorSprite;
                if (GoldText.Contains(text.name)) text.color = kit.Gold;
                else if (SuccessText.Contains(text.name)) text.color = kit.Cream;
                else if (MutedText.Contains(text.name)) text.color = kit.Muted;
                else text.color = kit.Parchment;

                if (text.name == "Controls text" || text.name == "Waypoint instruction")
                {
                    text.fontMaterial.EnableKeyword("OUTLINE_ON");
                    text.outlineWidth = 0.18f;
                    text.outlineColor = new Color32(20, 12, 6, 255);
                }

                if (CenteredText.Contains(text.name))
                {
                    text.alignment = TextAlignmentOptions.Center;
                    RectTransform rect = text.rectTransform;
                    rect.anchorMin = new Vector2(0.5f, rect.anchorMin.y);
                    rect.anchorMax = new Vector2(0.5f, rect.anchorMax.y);
                    rect.pivot = new Vector2(0.5f, rect.pivot.y);
                    rect.anchoredPosition = new Vector2(0f, rect.anchoredPosition.y);
                }

                if (TitleText.Contains(text.name) && kit.TitleMaterial != null)
                {
                    text.font = kit.HeadingFont;
                    text.fontSharedMaterial = kit.TitleMaterial;
                }
            }

            HudLayout.Apply(canvas);
            HudContentLayout.Apply(canvas);
        }

        public static void ApplyPanel(Image image, UiKit kit, HudSurface surface)
        {
            image.sprite = surface switch
            {
                HudSurface.Parchment => kit.ParchmentPanel,
                HudSurface.Popup => kit.Popup,
                HudSurface.Tooltip => kit.Tooltip,
                HudSurface.Inset => kit.Inset,
                _ => kit.Window
            };
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
            image.color = Color.white;
        }

        public static void ApplyButton(Button button, Image image, UiKit kit, bool primary)
        {
            image.sprite = primary ? kit.ButtonPrimaryNormal : kit.ButtonSecondaryNormal;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = true;
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                highlightedSprite = primary ? kit.ButtonPrimaryHover : kit.ButtonSecondaryHover,
                pressedSprite = primary ? kit.ButtonPrimaryPressed : kit.ButtonSecondaryPressed,
                selectedSprite = primary ? kit.ButtonPrimaryHover : kit.ButtonSecondaryHover,
                disabledSprite = primary ? kit.ButtonPrimaryDisabled : kit.ButtonSecondaryDisabled
            };
            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.font = kit.HeadingFont;
                label.fontStyle = FontStyles.SmallCaps;
                label.color = kit.Cream;
            }
        }

        public static void ApplyFill(Image image, UiKit kit, HudFill fill)
        {
            image.sprite = fill == HudFill.Water ? kit.BarManaFill : kit.BarXpFill;
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.color = Color.white;
        }

        private static void EnsureSafeArea(Canvas canvas)
        {
            RectTransform safe = canvas.transform.Find("Safe area") as RectTransform;
            if (safe == null)
            {
                var go = new GameObject("Safe area", typeof(RectTransform), typeof(SafeAreaFitter));
                go.layer = 5;
                safe = go.GetComponent<RectTransform>();
                safe.SetParent(canvas.transform, false);
                safe.SetAsFirstSibling();
            }
            string[] layers = { "Gameplay HUD", "Menu dimmer", "Main menu", "Briefing screen", "Controls screen", "Pause screen", "Ending screen" };
            foreach (string name in layers)
            {
                Transform layer = canvas.transform.Find(name);
                if (layer != null && layer.parent != safe) layer.SetParent(safe, false);
            }
        }
    }
}
