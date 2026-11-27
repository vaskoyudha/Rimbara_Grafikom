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
            { "Interaction prompt", HudSurface.Window },
            { "Feedback toast", HudSurface.Tooltip },
            { "Controls reminder", HudSurface.Inset },
            { "Arunika field map", HudSurface.Window },
            { "Main menu card", HudSurface.Window },
            { "Volunteer briefing", HudSurface.Window },
            { "How to play card", HudSurface.Window },
            { "Pause card", HudSurface.Window },
            { "Restoration ending", HudSurface.Window },
            { "Coordinator dialogue", HudSurface.Window },
            { "Dialogue portrait frame", HudSurface.Inset },
            { "Coordinator nameplate 2D", HudSurface.Inset }
        };

        private static readonly HashSet<string> PrimaryButtons = new HashSet<string>
        {
            "Start restoration", "Enter Arunika", "Resume mission", "Play again", "Help back", "Dialogue next"
        };

        private static readonly HashSet<string> GoldText = new HashSet<string>
        {
            "Menu eyebrow", "Briefing eyebrow", "Help eyebrow", "Pause eyebrow", "Victory eyebrow",
            "Mission number", "Counters header",
            "Briefing targets", "Menu scope", "Restoration percent"
        };

        private static readonly HashSet<string> SuccessText = new HashSet<string>
        {
            "RIMBARA title", "Mission title", "Briefing title", "Help title", "Pause title", "Victory title",
            "Waypoint instruction", "Waypoint distance"
        };

        private static readonly HashSet<string> MutedText = new HashSet<string>
        {
            "Map title", "Volume label", "Menu footer",
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
            EnsureCornerChrome(canvas, kit);

            foreach (UnityEngine.UI.Image image in canvas.GetComponentsInChildren<UnityEngine.UI.Image>(true))
            {
                if (Surfaces.TryGetValue(image.name, out HudSurface surface))
                {
                    ApplyPanel(image, kit, surface);
                    continue;
                }
                if (image.name == "Next destination")
                {
                    image.sprite = null;
                    image.color = Color.clear;
                    image.raycastTarget = false;
                    continue;
                }
                if (image.name == "Restoration bar background")
                {
                    image.sprite = kit.BarFrame != null ? kit.BarFrame : kit.Inset;
                    image.type = UnityEngine.UI.Image.Type.Sliced;
                    image.color = Color.white;
                    image.raycastTarget = false;
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
                if (text.name == "Nameplate text" || text.name.StartsWith("Choice ")) continue;
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

                if (text.name == "Controls text" || text.name == "Waypoint instruction" || text.name == "Waypoint distance")
                    ApplyReadableShadow(text, text.name == "Controls text" ? 0.18f : 0.25f);

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

        public static void ApplyReadableShadow(TMP_Text text, float outlineWidth = 0.25f)
        {
            if (text == null) return;
            Shader fullSdf = Shader.Find("TextMeshPro/Distance Field");
            Material mat = text.fontMaterial;
            if (mat == null) return;
            if (fullSdf != null && mat.shader != fullSdf) mat.shader = fullSdf;
            mat.EnableKeyword("OUTLINE_ON");
            mat.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
            mat.SetColor(ShaderUtilities.ID_OutlineColor, new Color32(10, 6, 3, 255));
            mat.EnableKeyword("UNDERLAY_ON");
            mat.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.95f));
            mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.9f);
            mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.9f);
            mat.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0.45f);
            mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.25f);
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

        private static void EnsureCornerChrome(Canvas canvas, UiKit kit)
        {
            Transform mission = FindNode(canvas.transform, "Mission card");
            if (mission != null)
            {
                EnsureImageChild(mission, "Mission icon", kit.IconQuest,
                    UnityEngine.UI.Image.Type.Simple, Color.white, 0, true);
                EnsureImageChild(mission, "Mission divider", kit.Divider != null ? kit.Divider : kit.Ribbon,
                    UnityEngine.UI.Image.Type.Simple, new Color(0.86f, 0.68f, 0.28f, 0.92f), 0);
                EnsureImageChild(mission, "Mission task inset", kit.Inset,
                    UnityEngine.UI.Image.Type.Sliced, new Color(1f, 1f, 1f, 0.58f), 0);
            }

            Transform counters = FindNode(canvas.transform, "Restoration counters");
            if (counters != null)
            {
                EnsureTextChild(counters, "Counters header", IndonesianText.Labels["Counters header"], kit, 12.5f, 1.6f);
                EnsureImageChild(counters, "Counters icon", kit.IconCompass,
                    UnityEngine.UI.Image.Type.Simple, kit.Gold, 0, true);
                EnsureImageChild(counters, "Counters divider", kit.Divider != null ? kit.Divider : kit.Ribbon,
                    UnityEngine.UI.Image.Type.Simple, new Color(0.86f, 0.68f, 0.28f, 0.92f), 0);
                for (int i = 0; i < 3; i++)
                    EnsureImageChild(counters, "Counter row " + i, kit.Inset,
                        UnityEngine.UI.Image.Type.Sliced, new Color(1f, 1f, 1f, 0.45f), 0);
                EnsureImageChild(counters, "Counter icon 0", kit.IconBag,
                    UnityEngine.UI.Image.Type.Simple, kit.Cream, counters.childCount - 1, true);
                EnsureImageChild(counters, "Counter icon 1", kit.IconCleanup,
                    UnityEngine.UI.Image.Type.Simple, Color.white, counters.childCount - 1, true);
                EnsureImageChild(counters, "Counter icon 2", kit.IconEmber,
                    UnityEngine.UI.Image.Type.Simple, Color.white, counters.childCount - 1, true);
            }

            Transform waypoint = FindNode(canvas.transform, "Next destination");
            if (waypoint != null)
            {
                EnsureTextChild(waypoint, "Waypoint distance", "0 M", kit, 16f, 1.0f, kit.Cream, FontStyles.Bold);
                Transform arrow = waypoint.Find("Waypoint 3D arrow");
                GameObject arrowGo = arrow != null ? arrow.gameObject
                    : new GameObject("Waypoint 3D arrow", typeof(RectTransform), typeof(UnityEngine.UI.RawImage), typeof(WaypointArrow3D));
                arrowGo.layer = 5;
                if (arrow == null) arrowGo.transform.SetParent(waypoint, false);
                if (arrowGo.GetComponent<WaypointArrow3D>() == null) arrowGo.AddComponent<WaypointArrow3D>();
                var raw = arrowGo.GetComponent<UnityEngine.UI.RawImage>();
                raw.raycastTarget = false;
            }

            Transform dialogue = FindNode(canvas.transform, "Coordinator dialogue");
            if (dialogue != null) EnsureDialogueChoices(dialogue, kit);

            EnsureCoordinatorNameplate(canvas, kit);
        }

        private static void EnsureCoordinatorNameplate(Canvas canvas, UiKit kit)
        {
            Transform gameplay = FindNode(canvas.transform, "Gameplay HUD");
            var coordinator = Object.FindFirstObjectByType<Rimbara.Interaction.CoordinatorDialogue>(FindObjectsInactive.Include);
            if (gameplay == null || coordinator == null) return;

            Transform existing = gameplay.Find("Coordinator nameplate 2D");
            GameObject go = existing != null ? existing.gameObject
                : new GameObject("Coordinator nameplate 2D", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(CanvasGroup), typeof(NpcNameplate2D));
            go.layer = 5;
            if (existing == null) go.transform.SetParent(gameplay, false);
            go.transform.SetAsFirstSibling();

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(204f, 46f);

            var bg = go.GetComponent<UnityEngine.UI.Image>();
            ApplyPanel(bg, kit, HudSurface.Inset);
            bg.color = new Color(1f, 1f, 1f, 0.94f);
            bg.raycastTarget = false;

            Transform iconT = go.transform.Find("Nameplate icon");
            GameObject iconGo = iconT != null ? iconT.gameObject : new GameObject("Nameplate icon", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            iconGo.layer = 5;
            if (iconT == null) iconGo.transform.SetParent(go.transform, false);
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(12f, 0f);
            iconRect.sizeDelta = new Vector2(20f, 20f);
            var iconImg = iconGo.GetComponent<UnityEngine.UI.Image>();
            iconImg.sprite = kit.IconQuest != null ? kit.IconQuest : kit.IconCompass;
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;
            iconImg.raycastTarget = false;

            Transform textT = go.transform.Find("Nameplate text");
            GameObject textGo = textT != null ? textT.gameObject : new GameObject("Nameplate text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.layer = 5;
            if (textT == null) textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(38f, 4f);
            textRect.offsetMax = new Vector2(-10f, -4f);

            var label = textGo.GetComponent<TMP_Text>();
            label.font = kit.BodyFont != null ? kit.BodyFont : kit.HeadingFont;
            label.fontStyle = FontStyles.Normal;
            label.enableAutoSizing = false;
            label.fontSize = 11.5f;
            label.lineSpacing = -6f;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.richText = true;
            label.color = kit.Cream;
            label.raycastTarget = false;
            label.text = "<b><size=13><color=#C99A3F>" + DialogueCopy.CoordinatorName + "</color></size></b>\n<size=10.5><color=#FFF0CC>" + DialogueCopy.CoordinatorSpeaker + "</color></size>";
            ApplyReadableShadow(label, 0.16f);

            Transform beacon = coordinator.transform.Find("Beacon - MISSION COORDINATOR");
            var nameplate = go.GetComponent<NpcNameplate2D>() ?? go.AddComponent<NpcNameplate2D>();
            nameplate.Configure(coordinator.transform, beacon != null ? beacon.gameObject : null);
        }

        private static void EnsureDialogueChoices(Transform dialogue, UiKit kit)
        {
            EnsureImageChild(dialogue, "Dialogue divider", kit.Divider != null ? kit.Divider : kit.Ribbon,
                UnityEngine.UI.Image.Type.Simple, new Color(0.86f, 0.68f, 0.28f, 0.92f), dialogue.childCount - 1);
            Transform nextTransform = dialogue.Find("Dialogue next");
            for (int i = 0; i < 2; i++)
            {
                string choiceName = "Dialogue choice " + i;
                Transform existing = dialogue.Find(choiceName);
                GameObject go;
                if (existing != null)
                {
                    go = existing.gameObject;
                }
                else if (nextTransform != null)
                {
                    go = Object.Instantiate(nextTransform.gameObject, dialogue);
                    go.name = choiceName;
                }
                else
                {
                    go = new GameObject(choiceName, typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
                    go.layer = 5;
                    go.transform.SetParent(dialogue, false);
                    var lbl = new GameObject("Choice label", typeof(RectTransform), typeof(TextMeshProUGUI));
                    lbl.layer = 5;
                    lbl.transform.SetParent(go.transform, false);
                }

                var btn = go.GetComponent<UnityEngine.UI.Button>();
                var img = go.GetComponent<UnityEngine.UI.Image>();
                btn.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                ApplyButton(btn, img, kit, false);
                if (go.GetComponent<ButtonHoverAnimation>() == null)
                    go.AddComponent<ButtonHoverAnimation>();

                var label = go.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    label.name = "Choice " + i + " text";
                    label.font = kit.BodyFont != null ? kit.BodyFont : kit.HeadingFont;
                    label.fontStyle = FontStyles.Normal;
                    label.color = kit.Cream;
                    label.enableAutoSizing = true;
                    label.fontSizeMin = 12f;
                    label.fontSizeMax = 15.5f;
                    label.alignment = TextAlignmentOptions.Center;
                    label.rectTransform.anchorMin = Vector2.zero;
                    label.rectTransform.anchorMax = Vector2.one;
                    label.rectTransform.offsetMin = new Vector2(10f, 4f);
                    label.rectTransform.offsetMax = new Vector2(-10f, -4f);
                }
            }
        }

        private static void EnsureImageChild(Transform parent, string name, Sprite sprite, UnityEngine.UI.Image.Type type, Color color, int siblingIndex, bool preserveAspect = false)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            go.layer = 5;
            if (existing == null) go.transform.SetParent(parent, false);
            go.transform.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, parent.childCount - 1));
            var image = go.GetComponent<UnityEngine.UI.Image>();
            image.sprite = sprite;
            image.type = type;
            image.preserveAspect = preserveAspect;
            image.color = color;
            image.enabled = sprite != null;
            image.raycastTarget = false;
        }

        private static void EnsureTextChild(Transform parent, string name, string copy, UiKit kit, float fontSize, float spacing, Color? color = null, FontStyles style = FontStyles.Normal)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.layer = 5;
            if (existing == null) go.transform.SetParent(parent, false);
            var text = go.GetComponent<TMP_Text>();
            text.text = copy;
            text.font = kit.HeadingFont != null ? kit.HeadingFont : kit.BodyFont;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.characterSpacing = spacing;
            text.color = color ?? kit.Gold;
            text.raycastTarget = false;
        }

        private static Transform FindNode(Transform root, string name)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
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
