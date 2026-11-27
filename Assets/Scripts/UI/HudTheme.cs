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
            { "Controls reminder", HudSurface.Tooltip },
            { "Arunika field map", HudSurface.Window },
            { "Main menu card", HudSurface.Window },
            { "Zone select card", HudSurface.Window },
            { "Settings card", HudSurface.Window },
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
            "Start restoration", "Enter Arunika", "Zone A begin", "Resume mission", "Play again", "Help back", "Dialogue next"
        };

        private static readonly HashSet<string> GoldText = new HashSet<string>
        {
            "Menu eyebrow", "Zone select eyebrow", "Settings eyebrow", "Briefing eyebrow", "Help eyebrow", "Pause eyebrow", "Victory eyebrow",
            "Mission number", "Counters header",
            "Briefing targets", "Menu scope", "Restoration percent"
        };

        private static readonly HashSet<string> SuccessText = new HashSet<string>
        {
            "RIMBARA title", "Zone select title", "Settings title", "Mission title", "Briefing title", "Help title", "Pause title", "Victory title",
            "Waypoint instruction", "Waypoint distance"
        };

        private static readonly HashSet<string> MutedText = new HashSet<string>
        {
            "Map title", "Volume label", "Menu footer",
            "No fail promise", "Help tip", "Game subtitle"
        };

        private static readonly HashSet<string> CenteredText = new HashSet<string>
        {
            "Menu eyebrow", "Zone select eyebrow", "Settings eyebrow", "Briefing eyebrow", "Help eyebrow", "Pause eyebrow", "Victory eyebrow",
            "RIMBARA title", "Zone select title", "Settings title", "Briefing title", "Help title", "Pause title", "Victory title",
            "Game subtitle", "Menu scope"
        };

        private static readonly HashSet<string> TitleText = new HashSet<string>
        {
            "RIMBARA title", "Zone select title", "Settings title", "Briefing title", "Help title", "Pause title", "Victory title"
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
                if (text.name == "Nameplate text" || text.name.StartsWith("Choice ")
                    || text.name.StartsWith("Tactical ") || text.name == "Map M hint" || text.name == "North U text"
                    || text.name.StartsWith("Zone ") || text.name.StartsWith("Settings ") || text.name.StartsWith("Quality ")
                    || text.name.StartsWith("TargetCard "))
                    continue;
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
                EnsureImageChild(mission, "Mission task inset",
                    kit.CardInset != null ? kit.CardInset : kit.Tooltip,
                    UnityEngine.UI.Image.Type.Sliced, new Color(1f, 1f, 1f, 0.90f), 0);
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
                    EnsureImageChild(counters, "Counter row " + i,
                        kit.RowInset != null ? kit.RowInset : kit.Inset,
                        UnityEngine.UI.Image.Type.Sliced, new Color(1f, 1f, 1f, 0.80f), 0);
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
            EnsureGtaRadar(canvas, kit);
            EnsureMenuScreens(canvas, kit);
        }

        private static void EnsureGtaRadar(Canvas canvas, UiKit kit)
        {
            Transform map = FindNode(canvas.transform, "Arunika field map");
            Transform gameplay = FindNode(canvas.transform, "Gameplay HUD");
            if (map == null || kit.WorldMap == null) return;

            var toggleBtn = map.GetComponent<Button>() ?? map.gameObject.AddComponent<Button>();
            toggleBtn.transition = Selectable.Transition.None;
            toggleBtn.targetGraphic = map.GetComponent<Image>();

            EnsureTextChild(map, "Map M hint", "[M]", kit, 11f, 0.8f, kit.Gold, FontStyles.Bold);
            var hintText = map.Find("Map M hint")?.GetComponent<TMP_Text>();
            if (hintText != null) hintText.alignment = TextAlignmentOptions.Right;

            Transform coords = map.Find("Map coordinates");
            if (coords == null) return;
            if (coords.GetComponent<RectMask2D>() == null) coords.gameObject.AddComponent<RectMask2D>();

            string[] legacyNodes = { "Zone A", "Zone B", "Zone C", "Trail", "A label", "B label", "C label" };
            foreach (string legacy in legacyNodes)
            {
                Transform t = coords.Find(legacy);
                if (t != null) t.gameObject.SetActive(false);
            }

            // Solid dark ground backing behind rotated map corners
            Transform backdropT = EnsureRectImage(coords, "Radar map backdrop", null, new Color(0.08f, 0.06f, 0.045f, 1f), 0);
            StretchFull(backdropT as RectTransform);

            Transform mapImgT = EnsureRectImage(coords, "Radar topdown image", kit.WorldMap, Color.white, 1);
            var mapImg = mapImgT.GetComponent<Image>();
            mapImg.preserveAspect = false;

            Transform gpsT = EnsureRectImage(coords, "Radar GPS route", null, new Color(0.95f, 0.72f, 0.22f, 0.82f), 2);
            gpsT.gameObject.SetActive(false);

            Transform blipsT = EnsureEmptyRect(coords, "Radar secondary blips", 3);
            StretchFull(blipsT as RectTransform);

            Transform targetT = coords.Find("Destination dot");
            if (targetT == null) targetT = EnsureRectImage(coords, "Destination dot", kit.RadarBlipDiamond, new Color(0.95f, 0.72f, 0.22f, 1f), 4);
            targetT.SetSiblingIndex(4);
            var targetImg = targetT.GetComponent<Image>();
            if (kit.RadarBlipDiamond != null) targetImg.sprite = kit.RadarBlipDiamond;
            targetImg.color = new Color(0.95f, 0.72f, 0.22f, 1f);
            targetImg.raycastTarget = false;
            (targetT as RectTransform).sizeDelta = new Vector2(15f, 15f);

            Transform fovT = EnsureRectImage(coords, "Radar FOV cone", kit.RadarFov, Color.white, 5);
            var fovRect = fovT as RectTransform;
            fovRect.anchorMin = fovRect.anchorMax = new Vector2(0.5f, 0.5f);
            fovRect.pivot = new Vector2(0.5f, 0f);
            fovRect.anchoredPosition = Vector2.zero;
            fovRect.sizeDelta = new Vector2(88f, 88f);

            Transform playerT = coords.Find("Volunteer dot");
            if (playerT == null) playerT = EnsureRectImage(coords, "Volunteer dot", kit.RadarChevron, Color.white, 6);
            playerT.SetSiblingIndex(6);
            var playerImg = playerT.GetComponent<Image>();
            if (kit.RadarChevron != null) playerImg.sprite = kit.RadarChevron;
            playerImg.color = Color.white;
            playerImg.raycastTarget = false;
            (playerT as RectTransform).sizeDelta = new Vector2(18f, 18f);

            Transform bezelT = EnsureRectImage(coords, "Radar bezel frame", kit.RadarBezel, Color.white, 7);
            StretchFull(bezelT as RectTransform);
            var bezelImg = bezelT.GetComponent<Image>();
            bezelImg.type = Image.Type.Sliced;

            Transform northT = EnsureRectImage(coords, "Radar north badge", kit.RadarBlipDot, new Color(0.11f, 0.08f, 0.05f, 0.96f), 8);
            var northRect = northT as RectTransform;
            northRect.anchorMin = northRect.anchorMax = new Vector2(0.5f, 0.5f);
            northRect.pivot = new Vector2(0.5f, 0.5f);
            northRect.sizeDelta = new Vector2(18f, 18f);
            EnsureTextChild(northT, "North U text", "U", kit, 10f, 0f, kit.Gold, FontStyles.Bold);
            var northLabel = northT.Find("North U text") as RectTransform;
            if (northLabel != null)
            {
                StretchFull(northLabel);
                northLabel.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
            }

            var controller = map.GetComponent<GtaRadarController>() ?? map.gameObject.AddComponent<GtaRadarController>();
            controller.ConfigureMinimap(
                kit.WorldMin, kit.WorldSize, kit.RadarBlipDot,
                coords as RectTransform, mapImgT as RectTransform,
                playerT as RectTransform, fovRect,
                targetT as RectTransform, gpsT as RectTransform,
                northRect, blipsT as RectTransform);

            if (gameplay == null) return;
            Transform overlayT = gameplay.Find("Arunika tactical map");
            GameObject overlayGo = overlayT != null ? overlayT.gameObject
                : new GameObject("Arunika tactical map", typeof(RectTransform), typeof(Image));
            overlayGo.layer = 5;
            if (overlayT == null) overlayGo.transform.SetParent(gameplay, false);
            var overlayBg = overlayGo.GetComponent<Image>();
            ApplyPanel(overlayBg, kit, HudSurface.Window);
            overlayBg.raycastTarget = true;

            EnsureTextChild(overlayGo.transform, "Tactical map title", "PETA TAKTIS ARUNIKA", kit, 22f, 2.2f, kit.Gold, FontStyles.Bold);
            var titleRect = overlayGo.transform.Find("Tactical map title") as RectTransform;
            if (titleRect != null)
            {
                titleRect.anchorMin = new Vector2(0f, 1f);
                titleRect.anchorMax = new Vector2(1f, 1f);
                titleRect.pivot = new Vector2(0.5f, 1f);
                titleRect.anchoredPosition = new Vector2(0f, -22f);
                titleRect.sizeDelta = new Vector2(-60f, 30f);
                titleRect.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
            }

            EnsureImageChild(overlayGo.transform, "Tactical map divider", kit.Divider != null ? kit.Divider : kit.Ribbon,
                Image.Type.Simple, new Color(0.86f, 0.68f, 0.28f, 0.92f), 1);
            var divRect = overlayGo.transform.Find("Tactical map divider") as RectTransform;
            if (divRect != null)
            {
                divRect.anchorMin = new Vector2(0f, 1f);
                divRect.anchorMax = new Vector2(1f, 1f);
                divRect.pivot = new Vector2(0.5f, 1f);
                divRect.anchoredPosition = new Vector2(0f, -54f);
                divRect.sizeDelta = new Vector2(-80f, 10f);
            }

            Transform surfaceT = EnsureEmptyRect(overlayGo.transform, "Tactical map surface", 2);
            var surfaceRect = surfaceT as RectTransform;
            surfaceRect.anchorMin = surfaceRect.anchorMax = new Vector2(0.5f, 0.5f);
            surfaceRect.pivot = new Vector2(0.5f, 0.5f);
            surfaceRect.anchoredPosition = new Vector2(0f, -4f);
            surfaceRect.sizeDelta = new Vector2(560f, 560f);
            if (surfaceT.GetComponent<RectMask2D>() == null) surfaceT.gameObject.AddComponent<RectMask2D>();

            Transform fullImgT = EnsureRectImage(surfaceT, "Tactical topdown image", kit.WorldMap, Color.white, 0);
            StretchFull(fullImgT as RectTransform);

            Transform fullGpsT = EnsureRectImage(surfaceT, "Tactical GPS route", null, new Color(0.95f, 0.72f, 0.22f, 0.85f), 1);
            fullGpsT.gameObject.SetActive(false);

            Transform fullBlipsT = EnsureEmptyRect(surfaceT, "Tactical secondary blips", 2);
            StretchFull(fullBlipsT as RectTransform);

            Transform fullTargetT = EnsureRectImage(surfaceT, "Tactical target dot", kit.RadarBlipDiamond, new Color(0.95f, 0.72f, 0.22f, 1f), 3);
            (fullTargetT as RectTransform).sizeDelta = new Vector2(20f, 20f);

            Transform fullFovT = EnsureRectImage(surfaceT, "Tactical FOV cone", kit.RadarFov, Color.white, 4);
            var fullFovRect = fullFovT as RectTransform;
            fullFovRect.anchorMin = fullFovRect.anchorMax = new Vector2(0.5f, 0.5f);
            fullFovRect.pivot = new Vector2(0.5f, 0f);
            fullFovRect.sizeDelta = new Vector2(96f, 96f);

            Transform fullPlayerT = EnsureRectImage(surfaceT, "Tactical player chevron", kit.RadarChevron, Color.white, 5);
            (fullPlayerT as RectTransform).sizeDelta = new Vector2(22f, 22f);

            Transform fullBezelT = EnsureRectImage(surfaceT, "Tactical bezel frame", kit.RadarBezel, Color.white, 6);
            StretchFull(fullBezelT as RectTransform);
            fullBezelT.GetComponent<Image>().type = Image.Type.Sliced;

            EnsureTextChild(overlayGo.transform, "Tactical map legend",
                "<color=#FFF4D2>▲</color> Relawan   <color=#F2B838><sprite name=\"diamond\" tint=1></color> Tujuan   <color=#EB5738>●</color> Api   <color=#7AC75C>●</color> Tugas   <color=#8CD6F2>●</color> NPC   |   [M] Tutup",
                kit, 13.5f, 0.4f, kit.Cream, FontStyles.Normal);
            var legendRect = overlayGo.transform.Find("Tactical map legend") as RectTransform;
            if (legendRect != null)
            {
                legendRect.anchorMin = new Vector2(0f, 0f);
                legendRect.anchorMax = new Vector2(1f, 0f);
                legendRect.pivot = new Vector2(0.5f, 0f);
                legendRect.anchoredPosition = new Vector2(0f, 26f);
                legendRect.sizeDelta = new Vector2(-60f, 24f);
                var lTxt = legendRect.GetComponent<TMP_Text>();
                if (kit.SeparatorSprite != null) lTxt.spriteAsset = kit.SeparatorSprite;
                lTxt.richText = true;
                lTxt.alignment = TextAlignmentOptions.Center;
            }

            overlayGo.SetActive(false);
            controller.ConfigureExpandedMap(
                overlayGo, surfaceRect,
                fullPlayerT as RectTransform, fullFovRect,
                fullTargetT as RectTransform, fullGpsT as RectTransform,
                fullBlipsT as RectTransform);
        }

        private static Transform EnsureRectImage(Transform parent, string name, Sprite sprite, Color color, int sibling)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
            go.layer = 5;
            if (existing == null) go.transform.SetParent(parent, false);
            go.transform.SetSiblingIndex(Mathf.Clamp(sibling, 0, parent.childCount - 1));
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            return go.transform;
        }

        private static Transform EnsureEmptyRect(Transform parent, string name, int sibling)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            if (existing == null) go.transform.SetParent(parent, false);
            go.transform.SetSiblingIndex(Mathf.Clamp(sibling, 0, parent.childCount - 1));
            return go.transform;
        }

        private static void StretchFull(RectTransform rect)
        {
            if (rect == null) return;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
            if (kit.SeparatorSprite != null) text.spriteAsset = kit.SeparatorSprite;
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

        private static void EnsureMenuScreens(Canvas canvas, UiKit kit)
        {
            Transform safe = canvas.transform.Find("Safe area") ?? canvas.transform;
            Transform mainMenuCard = FindNode(canvas.transform, "Main menu card");
            Button settingsOpenBtn = null;
            if (mainMenuCard != null)
            {
                // Ornate divider & story/scope insets to match the HUD's fantasy craft
                EnsureImageChild(mainMenuCard, "Menu divider", kit.Divider != null ? kit.Divider : kit.Ribbon,
                    Image.Type.Simple, new Color(0.86f, 0.68f, 0.28f, 0.92f), 3);
                PositionTopCenter(mainMenuCard.Find("Menu divider") as RectTransform, new Vector2(604f, 10f), -214f);

                EnsureImageChild(mainMenuCard, "Menu story inset", kit.CardInset != null ? kit.CardInset : kit.Tooltip,
                    Image.Type.Sliced, new Color(1f, 1f, 1f, 0.92f), 4);
                PositionTopCenter(mainMenuCard.Find("Menu story inset") as RectTransform, new Vector2(604f, 142f), -234f);

                EnsureImageChild(mainMenuCard, "Menu scope inset", kit.RowInset != null ? kit.RowInset : kit.Inset,
                    Image.Type.Sliced, new Color(1f, 1f, 1f, 0.85f), 5);
                PositionTopCenter(mainMenuCard.Find("Menu scope inset") as RectTransform, new Vector2(604f, 36f), -386f);

                var story = mainMenuCard.Find("Menu story") as RectTransform;
                if (story != null)
                {
                    story.SetAsLastSibling();
                    PositionTopCenter(story, new Vector2(564f, 118f), -246f);
                }

                var scope = mainMenuCard.Find("Menu scope") as RectTransform;
                if (scope != null)
                {
                    scope.SetAsLastSibling();
                    PositionTopCenter(scope, new Vector2(604f, 26f), -391f);
                }

                var startBtn = mainMenuCard.Find("Start restoration") as RectTransform;
                if (startBtn != null)
                    PositionTopLeft(startBtn, 58f, -440f, 604f, 60f);

                var controlsBtn = mainMenuCard.Find("Controls button") as RectTransform;
                var quitBtn = mainMenuCard.Find("Menu quit") as RectTransform;
                settingsOpenBtn = EnsureStyledButton(mainMenuCard, "Settings button",
                    IndonesianText.Labels["Settings button label"], kit, false);
                var settingsRect = settingsOpenBtn.GetComponent<RectTransform>();

                if (controlsBtn != null)
                {
                    PositionTopLeft(controlsBtn, 58f, -516f, 192f, 50f);
                    FixButtonLabel(controlsBtn, 15f);
                }
                PositionTopLeft(settingsRect, 264f, -516f, 192f, 50f);
                FixButtonLabel(settingsRect, 15f);
                if (quitBtn != null)
                {
                    PositionTopLeft(quitBtn, 470f, -516f, 192f, 50f);
                    FixButtonLabel(quitBtn, 15f);
                }

                var footer = mainMenuCard.Find("Menu footer") as RectTransform;
                if (footer != null)
                {
                    footer.SetAsLastSibling();
                    PositionTopCenter(footer, new Vector2(604f, 54f), -594f);
                }
            }

            // 1. Zone Selection Screen
            Transform zoneLayerT = safe.Find("Zone select screen");
            GameObject zoneLayer = zoneLayerT != null ? zoneLayerT.gameObject : CreateLayer(safe, "Zone select screen");
            Transform zoneCardT = zoneLayer.transform.Find("Zone select card");
            if (zoneCardT == null)
            {
                var cardGo = new GameObject("Zone select card", typeof(RectTransform), typeof(Image));
                cardGo.layer = 5;
                cardGo.transform.SetParent(zoneLayer.transform, false);
                zoneCardT = cardGo.transform;
            }
            ApplyPanel(zoneCardT.GetComponent<Image>(), kit, HudSurface.Window);
            zoneCardT.GetComponent<Image>().raycastTarget = true;

            EnsureTextChild(zoneCardT, "Zone select eyebrow", IndonesianText.Labels["Zone select eyebrow"],
                kit, 13f, 2.2f, kit.Gold, FontStyles.Bold);
            var zEyebrow = zoneCardT.Find("Zone select eyebrow") as RectTransform;
            PositionTopCenter(zEyebrow, new Vector2(980f, 22f), -30f);
            zEyebrow.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

            EnsureTextChild(zoneCardT, "Zone select title", IndonesianText.Labels["Zone select title"],
                kit, 38f, 3f, kit.Cream, FontStyles.Bold);
            var zTitle = zoneCardT.Find("Zone select title") as RectTransform;
            PositionTopCenter(zTitle, new Vector2(980f, 48f), -54f);
            var zTitleTxt = zTitle.GetComponent<TMP_Text>();
            zTitleTxt.alignment = TextAlignmentOptions.Center;
            if (kit.TitleMaterial != null) zTitleTxt.fontSharedMaterial = kit.TitleMaterial;

            EnsureImageChild(zoneCardT, "Zone select divider", kit.Divider != null ? kit.Divider : kit.Ribbon,
                Image.Type.Simple, new Color(0.86f, 0.68f, 0.28f, 0.92f), zoneCardT.childCount);
            PositionTopCenter(zoneCardT.Find("Zone select divider") as RectTransform, new Vector2(980f, 10f), -108f);

            // 3 Zone Cards
            Button zoneABegin = BuildZoneCard(zoneCardT, "Zone A", -344f, kit,
                kit.ZoneAThumbnail, unlocked: true,
                "TERBUKA  •  BAB 01-02",
                IndonesianText.Labels["Zone A name"],
                IndonesianText.Labels["Zone A desc"],
                IndonesianText.Labels["Zone begin label"],
                out Image zoneAProgressFill, out TMP_Text zoneAProgressText);

            BuildZoneCard(zoneCardT, "Zone B", 0f, kit,
                kit.ZoneBThumbnail, unlocked: false,
                "TERKUNCI  •  BAB 03-04",
                IndonesianText.Labels["Zone B name"],
                IndonesianText.Labels["Zone B desc"],
                IndonesianText.Labels["Zone locked label"],
                out _, out _);

            BuildZoneCard(zoneCardT, "Zone C", 344f, kit,
                kit.ZoneCThumbnail, unlocked: false,
                "TERKUNCI  •  BAB 05",
                IndonesianText.Labels["Zone C name"],
                IndonesianText.Labels["Zone C desc"],
                IndonesianText.Labels["Zone locked label"],
                out _, out _);

            Button zoneBackBtn = EnsureStyledButton(zoneCardT, "Zone select back",
                IndonesianText.Labels["Zone select back label"], kit, false);
            var zoneBackRect = zoneBackBtn.GetComponent<RectTransform>();
            zoneBackRect.anchorMin = zoneBackRect.anchorMax = new Vector2(0.5f, 0f);
            zoneBackRect.pivot = new Vector2(0.5f, 0f);
            zoneBackRect.anchoredPosition = new Vector2(0f, 28f);
            zoneBackRect.sizeDelta = new Vector2(240f, 48f);

            zoneLayer.SetActive(false);

            // 2. Settings Screen
            Transform settingsLayerT = safe.Find("Settings screen");
            GameObject settingsLayer = settingsLayerT != null ? settingsLayerT.gameObject : CreateLayer(safe, "Settings screen");
            Transform settingsCardT = settingsLayer.transform.Find("Settings card");
            if (settingsCardT == null)
            {
                var cardGo = new GameObject("Settings card", typeof(RectTransform), typeof(Image));
                cardGo.layer = 5;
                cardGo.transform.SetParent(settingsLayer.transform, false);
                settingsCardT = cardGo.transform;
            }
            ApplyPanel(settingsCardT.GetComponent<Image>(), kit, HudSurface.Window);
            settingsCardT.GetComponent<Image>().raycastTarget = true;

            EnsureTextChild(settingsCardT, "Settings eyebrow", IndonesianText.Labels["Settings eyebrow"],
                kit, 13f, 2f, kit.Gold, FontStyles.Bold);
            var sEyebrow = settingsCardT.Find("Settings eyebrow") as RectTransform;
            PositionTopCenter(sEyebrow, new Vector2(580f, 22f), -30f);
            sEyebrow.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

            EnsureTextChild(settingsCardT, "Settings title", IndonesianText.Labels["Settings title"],
                kit, 38f, 3f, kit.Cream, FontStyles.Bold);
            var sTitle = settingsCardT.Find("Settings title") as RectTransform;
            PositionTopCenter(sTitle, new Vector2(580f, 48f), -54f);
            var sTitleTxt = sTitle.GetComponent<TMP_Text>();
            sTitleTxt.alignment = TextAlignmentOptions.Center;
            if (kit.TitleMaterial != null) sTitleTxt.fontSharedMaterial = kit.TitleMaterial;

            EnsureImageChild(settingsCardT, "Settings divider", kit.Divider != null ? kit.Divider : kit.Ribbon,
                Image.Type.Simple, new Color(0.86f, 0.68f, 0.28f, 0.92f), settingsCardT.childCount);
            PositionTopCenter(settingsCardT.Find("Settings divider") as RectTransform, new Vector2(580f, 10f), -108f);

            // Volume box
            Transform volBox = EnsureCardBox(settingsCardT, "Settings volume box", kit, -134f, 580f, 104f);
            EnsureTextChild(volBox, "Settings volume label", IndonesianText.Labels["Settings volume label"], kit, 13.5f, 1.5f, kit.Gold, FontStyles.Bold);
            PositionTopLeft(volBox.Find("Settings volume label") as RectTransform, 22f, -16f, 320f, 22f);
            EnsureTextChild(volBox, "Settings volume value", "100%", kit, 13.5f, 0.8f, kit.Cream, FontStyles.Bold);
            var volValRect = volBox.Find("Settings volume value") as RectTransform;
            PositionTopLeft(volValRect, 438f, -16f, 120f, 22f);
            volValRect.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Right;
            Slider settingsVolSlider = EnsureSlider(volBox, "Settings volume slider", kit, 22f, -52f, 536f, 26f);

            // Quality box
            Transform qualBox = EnsureCardBox(settingsCardT, "Settings quality box", kit, -252f, 580f, 114f);
            EnsureTextChild(qualBox, "Settings quality label", IndonesianText.Labels["Settings quality label"], kit, 13.5f, 1.5f, kit.Gold, FontStyles.Bold);
            PositionTopLeft(qualBox.Find("Settings quality label") as RectTransform, 22f, -16f, 320f, 22f);
            Button qLow = EnsureStyledButton(qualBox, "Quality low", IndonesianText.Labels["Settings quality low"], kit, false);
            Button qMed = EnsureStyledButton(qualBox, "Quality med", IndonesianText.Labels["Settings quality med"], kit, false);
            Button qHigh = EnsureStyledButton(qualBox, "Quality high", IndonesianText.Labels["Settings quality high"], kit, false);
            PositionTopLeft(qLow.GetComponent<RectTransform>(), 22f, -50f, 170f, 44f);
            PositionTopLeft(qMed.GetComponent<RectTransform>(), 205f, -50f, 170f, 44f);
            PositionTopLeft(qHigh.GetComponent<RectTransform>(), 388f, -50f, 170f, 44f);

            // Controls quick reference box
            Transform ctrlBox = EnsureCardBox(settingsCardT, "Settings controls box", kit, -380f, 580f, 178f);
            EnsureTextChild(ctrlBox, "Settings controls header", IndonesianText.Labels["Settings controls header"], kit, 13.5f, 1.5f, kit.Gold, FontStyles.Bold);
            PositionTopLeft(ctrlBox.Find("Settings controls header") as RectTransform, 22f, -16f, 320f, 22f);
            EnsureTextChild(ctrlBox, "Settings controls list",
                "WASD  Bergerak   •   SHIFT  Berlari   •   SPASI  Melompat\nMOUSE  Putar Kamera   •   E  Interaksi   •   KLIK KIRI  Siram\nM  Peta Taktis   •   I / TAB  Tas   •   ESC  Jeda",
                kit, 14.5f, 0.3f, kit.Parchment, FontStyles.Normal);
            var ctrlListRect = ctrlBox.Find("Settings controls list") as RectTransform;
            PositionTopLeft(ctrlListRect, 22f, -48f, 536f, 110f);
            var ctrlListTxt = ctrlListRect.GetComponent<TMP_Text>();
            ctrlListTxt.font = kit.BodyFont != null ? kit.BodyFont : kit.HeadingFont;
            ctrlListTxt.lineSpacing = 8f;

            Button settingsBackBtn = EnsureStyledButton(settingsCardT, "Settings back",
                IndonesianText.Labels["Settings back label"], kit, false);
            var settingsBackRect = settingsBackBtn.GetComponent<RectTransform>();
            settingsBackRect.anchorMin = settingsBackRect.anchorMax = new Vector2(0.5f, 0f);
            settingsBackRect.pivot = new Vector2(0.5f, 0f);
            settingsBackRect.anchoredPosition = new Vector2(0f, 28f);
            settingsBackRect.sizeDelta = new Vector2(240f, 48f);

            settingsLayer.SetActive(false);

            // 3. Volunteer Briefing — replace plain target text with 4 icon cards + inset polish
            Transform briefingCard = FindNode(canvas.transform, "Volunteer briefing");
            if (briefingCard != null)
            {
                EnsureImageChild(briefingCard, "Briefing divider", kit.Divider != null ? kit.Divider : kit.Ribbon,
                    Image.Type.Simple, new Color(0.86f, 0.68f, 0.28f, 0.92f), 2);
                PositionTopCenter(briefingCard.Find("Briefing divider") as RectTransform, new Vector2(728f, 10f), -144f);

                var bStory = briefingCard.Find("Briefing story") as RectTransform;
                if (bStory != null) PositionTopCenter(bStory, new Vector2(728f, 88f), -164f);

                EnsureImageChild(briefingCard, "Briefing steps inset", kit.CardInset != null ? kit.CardInset : kit.Tooltip,
                    Image.Type.Sliced, new Color(1f, 1f, 1f, 0.92f), 4);
                PositionTopCenter(briefingCard.Find("Briefing steps inset") as RectTransform, new Vector2(728f, 152f), -262f);

                var bSteps = briefingCard.Find("Briefing steps") as RectTransform;
                if (bSteps != null)
                {
                    bSteps.SetAsLastSibling();
                    PositionTopCenter(bSteps, new Vector2(692f, 132f), -272f);
                }

                // Keep Briefing targets TMP_Text for tests/smoke, hide visually via CanvasGroup
                var bTargets = briefingCard.Find("Briefing targets") as RectTransform;
                if (bTargets != null)
                {
                    var cg = bTargets.GetComponent<CanvasGroup>();
                    if (cg == null) cg = bTargets.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    cg.blocksRaycasts = false;
                }

                Transform cardsRow = EnsureEmptyRect(briefingCard, "Briefing target cards", briefingCard.childCount);
                PositionTopCenter(cardsRow as RectTransform, new Vector2(728f, 68f), -428f);

                BuildBriefingTargetCard(cardsRow, 0, -273f, kit, kit.IconEmber, "10", "TITIK API");
                BuildBriefingTargetCard(cardsRow, 1, -91f, kit, kit.IconCleanup, "6", "PUING");
                BuildBriefingTargetCard(cardsRow, 2, 91f, kit, kit.IconBag, "5", "BIBIT");
                BuildBriefingTargetCard(cardsRow, 3, 273f, kit, kit.IconCompass, "1–2", "ZONA");

                var noFail = briefingCard.Find("No fail promise") as RectTransform;
                if (noFail != null) PositionTopCenter(noFail, new Vector2(728f, 28f), -512f);

                var beginBtn = briefingCard.Find("Enter Arunika") as RectTransform;
                if (beginBtn != null) beginBtn.anchoredPosition = new Vector2(beginBtn.anchoredPosition.x, -562f);
                var backBtn = briefingCard.Find("Briefing back") as RectTransform;
                if (backBtn != null) backBtn.anchoredPosition = new Vector2(backBtn.anchoredPosition.x, -562f);
            }

            // Wire MenuScreensController and HudReferences
            var menuController = canvas.GetComponent<MenuScreensController>() ?? canvas.gameObject.AddComponent<MenuScreensController>();
            menuController.Configure(
                settingsVolSlider,
                volValRect.GetComponent<TMP_Text>(),
                new[] { qLow, qMed, qHigh },
                zoneAProgressFill,
                zoneAProgressText);

            var hud = Object.FindFirstObjectByType<RimbaraHud>(FindObjectsInactive.Include);
            if (hud != null && hud.View != null)
            {
                hud.View.ZoneSelect = zoneLayer;
                hud.View.Settings = settingsLayer;
                hud.View.OpenSettings = settingsOpenBtn;
                hud.View.SettingsBack = settingsBackBtn;
                hud.View.ZoneSelectBack = zoneBackBtn;
                hud.View.ZoneABegin = zoneABegin;
                hud.View.MenuScreens = menuController;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(hud);
#endif
            }
        }

        private static void FixButtonLabel(RectTransform btnRect, float maxFontSize)
        {
            if (btnRect == null) return;
            var txt = btnRect.GetComponentInChildren<TMP_Text>(true);
            if (txt == null) return;
            StretchFull(txt.rectTransform);
            txt.rectTransform.offsetMin = new Vector2(8f, 4f);
            txt.rectTransform.offsetMax = new Vector2(-8f, -4f);
            txt.alignment = TextAlignmentOptions.Center;
            txt.enableAutoSizing = true;
            txt.fontSizeMin = 11f;
            txt.fontSizeMax = maxFontSize;
            txt.fontSize = maxFontSize;
        }

        private static GameObject CreateLayer(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            go.transform.SetParent(parent, false);
            StretchFull(go.GetComponent<RectTransform>());
            return go;
        }

        private static Transform EnsureCardBox(Transform parent, string name, UiKit kit, float y, float width, float height)
        {
            Transform box = EnsureRectImage(parent, name, kit.CardInset != null ? kit.CardInset : kit.Tooltip,
                new Color(1f, 1f, 1f, 0.94f), parent.childCount);
            box.GetComponent<Image>().type = Image.Type.Sliced;
            PositionTopCenter(box as RectTransform, new Vector2(width, height), y);
            return box;
        }

        private static void BuildBriefingTargetCard(
            Transform parent, int index, float xOffset, UiKit kit, Sprite icon, string valueCopy, string labelCopy)
        {
            string cardName = "TargetCard " + index;
            Transform cardT = EnsureRectImage(parent, cardName,
                kit.RowInset != null ? kit.RowInset : (kit.CardInset != null ? kit.CardInset : kit.Inset),
                new Color(1f, 1f, 1f, 0.96f), index);
            cardT.GetComponent<Image>().type = Image.Type.Sliced;
            var rect = cardT as RectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(xOffset, 0f);
            rect.sizeDelta = new Vector2(170f, 68f);

            Transform iconT = EnsureRectImage(cardT, "TargetCard icon " + index, icon, Color.white, 0);
            var iconRect = iconT as RectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(14f, 0f);
            iconRect.sizeDelta = new Vector2(32f, 32f);
            iconT.GetComponent<Image>().preserveAspect = true;

            EnsureTextChild(cardT, "TargetCard value " + index, valueCopy, kit, 20f, 0.8f, kit.Cream, FontStyles.Bold);
            var valRect = cardT.Find("TargetCard value " + index) as RectTransform;
            valRect.anchorMin = valRect.anchorMax = new Vector2(0f, 0.5f);
            valRect.pivot = new Vector2(0f, 0.5f);
            valRect.anchoredPosition = new Vector2(54f, 10f);
            valRect.sizeDelta = new Vector2(106f, 24f);
            valRect.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Left;

            EnsureTextChild(cardT, "TargetCard label " + index, labelCopy, kit, 11f, 1.2f, kit.Gold, FontStyles.Bold);
            var lblRect = cardT.Find("TargetCard label " + index) as RectTransform;
            lblRect.anchorMin = lblRect.anchorMax = new Vector2(0f, 0.5f);
            lblRect.pivot = new Vector2(0f, 0.5f);
            lblRect.anchoredPosition = new Vector2(54f, -12f);
            lblRect.sizeDelta = new Vector2(106f, 18f);
            lblRect.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Left;
        }

        private static Button BuildZoneCard(
            Transform parent,
            string prefix,
            float xOffset,
            UiKit kit,
            Sprite thumbnail,
            bool unlocked,
            string badgeCopy,
            string titleCopy,
            string descCopy,
            string buttonCopy,
            out Image progressFill,
            out TMP_Text progressText)
        {
            string cardName = prefix + " card";
            Transform cardT = EnsureRectImage(parent, cardName,
                kit.CardInset != null ? kit.CardInset : kit.Tooltip,
                unlocked ? new Color(1f, 1f, 1f, 0.98f) : new Color(0.68f, 0.64f, 0.58f, 0.90f),
                parent.childCount);
            cardT.GetComponent<Image>().type = Image.Type.Sliced;
            var cardRect = cardT as RectTransform;
            cardRect.anchorMin = cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = new Vector2(xOffset, -14f);
            cardRect.sizeDelta = new Vector2(320f, 480f);

            // Thumbnail box
            Transform thumbT = EnsureRectImage(cardT, prefix + " thumbnail", thumbnail,
                unlocked ? Color.white : new Color(0.32f, 0.30f, 0.28f, 1f), 0);
            PositionTopCenter(thumbT as RectTransform, new Vector2(284f, 168f), -18f);
            thumbT.GetComponent<Image>().preserveAspect = false;

            Transform bezelT = EnsureRectImage(thumbT, prefix + " bezel", kit.RadarBezel, Color.white, 1);
            StretchFull(bezelT as RectTransform);
            bezelT.GetComponent<Image>().type = Image.Type.Sliced;

            Transform lockOverlay = thumbT.Find(prefix + " lock badge");
            if (!unlocked)
            {
                if (lockOverlay == null)
                    lockOverlay = EnsureRectImage(thumbT, prefix + " lock badge", kit.RowInset != null ? kit.RowInset : kit.Tooltip,
                        new Color(0.08f, 0.06f, 0.04f, 0.88f), 2);
                lockOverlay.gameObject.SetActive(true);
                var lockRect = lockOverlay as RectTransform;
                lockRect.anchorMin = lockRect.anchorMax = new Vector2(0.5f, 0.5f);
                lockRect.pivot = new Vector2(0.5f, 0.5f);
                lockRect.anchoredPosition = Vector2.zero;
                lockRect.sizeDelta = new Vector2(148f, 36f);
                lockOverlay.GetComponent<Image>().type = Image.Type.Sliced;
                EnsureTextChild(lockOverlay, prefix + " lock text", "TERKUNCI", kit, 13f, 2f, kit.Muted, FontStyles.Bold);
                var lt = lockOverlay.Find(prefix + " lock text") as RectTransform;
                StretchFull(lt);
                lt.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
            }
            else if (lockOverlay != null)
            {
                lockOverlay.gameObject.SetActive(false);
            }

            // Badge
            EnsureTextChild(cardT, prefix + " badge", badgeCopy, kit, 11.5f, 1.5f,
                unlocked ? kit.Gold : kit.Muted, FontStyles.Bold);
            PositionTopCenter(cardT.Find(prefix + " badge") as RectTransform, new Vector2(280f, 18f), -198f);

            // Title
            EnsureTextChild(cardT, prefix + " name", titleCopy, kit, 20f, 1.2f,
                unlocked ? kit.Cream : kit.Muted, FontStyles.Bold);
            PositionTopCenter(cardT.Find(prefix + " name") as RectTransform, new Vector2(280f, 28f), -220f);

            // Description
            EnsureTextChild(cardT, prefix + " desc", descCopy, kit, 13.5f, 0.2f,
                unlocked ? kit.Parchment : kit.Muted, FontStyles.Normal);
            var descRect = cardT.Find(prefix + " desc") as RectTransform;
            PositionTopCenter(descRect, new Vector2(280f, 68f), -254f);
            var descTxt = descRect.GetComponent<TMP_Text>();
            descTxt.font = kit.BodyFont != null ? kit.BodyFont : kit.HeadingFont;
            descTxt.textWrappingMode = TextWrappingModes.Normal;

            // Progress label + bar
            EnsureTextChild(cardT, prefix + " progress text",
                unlocked ? "PROGRES  0%" : "SELESAIKAN ZONA SEBELUMNYA",
                kit, 11.5f, 1f, unlocked ? kit.Gold : kit.Muted, FontStyles.Bold);
            PositionTopCenter(cardT.Find(prefix + " progress text") as RectTransform, new Vector2(280f, 18f), -336f);

            Transform barBg = EnsureRectImage(cardT, prefix + " bar bg",
                kit.BarFrame != null ? kit.BarFrame : kit.Inset, Color.white, cardT.childCount);
            barBg.GetComponent<Image>().type = Image.Type.Sliced;
            PositionTopCenter(barBg as RectTransform, new Vector2(280f, 14f), -360f);

            Transform barFillT = EnsureRectImage(barBg, prefix + " bar fill",
                kit.BarXpFill != null ? kit.BarXpFill : kit.BarHealthFill,
                unlocked ? Color.white : new Color(0.4f, 0.4f, 0.4f, 0.4f), 0);
            var fillRect = barFillT as RectTransform;
            StretchFull(fillRect);
            fillRect.offsetMin = new Vector2(3f, 3f);
            fillRect.offsetMax = new Vector2(-3f, -3f);
            progressFill = barFillT.GetComponent<Image>();
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillAmount = 0f;
            progressText = cardT.Find(prefix + " progress text").GetComponent<TMP_Text>();

            // Action button
            Button btn = EnsureStyledButton(cardT, prefix + " begin", buttonCopy, kit, unlocked);
            btn.interactable = unlocked;
            var btnRect = btn.GetComponent<RectTransform>();
            btnRect.anchorMin = btnRect.anchorMax = new Vector2(0.5f, 0f);
            btnRect.pivot = new Vector2(0.5f, 0f);
            btnRect.anchoredPosition = new Vector2(0f, 22f);
            btnRect.sizeDelta = new Vector2(280f, 48f);
            return btn;
        }

        private static Button EnsureStyledButton(Transform parent, string name, string copy, UiKit kit, bool primary)
        {
            Transform existing = parent.Find(name);
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
                go.layer = 5;
                go.transform.SetParent(parent, false);
                var lbl = new GameObject(name + " label", typeof(RectTransform), typeof(TextMeshProUGUI));
                lbl.layer = 5;
                lbl.transform.SetParent(go.transform, false);
                StretchFull(lbl.GetComponent<RectTransform>());
            }

            var btn = go.GetComponent<Button>();
            var img = go.GetComponent<Image>();
            ApplyButton(btn, img, kit, primary);
            if (go.GetComponent<ButtonHoverAnimation>() == null)
                go.AddComponent<ButtonHoverAnimation>();

            var txt = go.GetComponentInChildren<TMP_Text>(true);
            if (txt != null)
            {
                txt.text = copy;
                txt.font = kit.HeadingFont;
                txt.fontStyle = FontStyles.SmallCaps;
                txt.enableAutoSizing = true;
                txt.fontSizeMin = 11f;
                txt.fontSizeMax = 15f;
                txt.fontSize = 15f;
                txt.color = kit.Cream;
                txt.alignment = TextAlignmentOptions.Center;
                txt.raycastTarget = false;
            }
            return btn;
        }

        private static Slider EnsureSlider(Transform parent, string name, UiKit kit, float x, float y, float width, float height)
        {
            Transform existing = parent.Find(name);
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = new GameObject(name, typeof(RectTransform), typeof(Slider));
                go.layer = 5;
                go.transform.SetParent(parent, false);

                var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
                bg.layer = 5;
                bg.transform.SetParent(go.transform, false);
                StretchFull(bg.GetComponent<RectTransform>());

                var fillArea = new GameObject("Fill Area", typeof(RectTransform));
                fillArea.layer = 5;
                fillArea.transform.SetParent(go.transform, false);
                StretchFull(fillArea.GetComponent<RectTransform>());

                var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                fill.layer = 5;
                fill.transform.SetParent(fillArea.transform, false);
                StretchFull(fill.GetComponent<RectTransform>());

                var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
                handleArea.layer = 5;
                handleArea.transform.SetParent(go.transform, false);
                StretchFull(handleArea.GetComponent<RectTransform>());

                var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
                handle.layer = 5;
                handle.transform.SetParent(handleArea.transform, false);
                handle.GetComponent<RectTransform>().sizeDelta = new Vector2(24f, 24f);

                var slider = go.GetComponent<Slider>();
                slider.fillRect = fill.GetComponent<RectTransform>();
                slider.handleRect = handle.GetComponent<RectTransform>();
                slider.targetGraphic = handle.GetComponent<Image>();
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.value = 1f;
            }

            PositionTopLeft(go.GetComponent<RectTransform>(), x, y, width, height);
            var bgImg = go.transform.Find("Background")?.GetComponent<Image>();
            if (bgImg != null)
            {
                bgImg.sprite = kit.SliderTrack;
                bgImg.type = Image.Type.Sliced;
                bgImg.color = Color.white;
            }
            var fillImg = go.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
            if (fillImg != null)
            {
                fillImg.sprite = kit.SliderFill;
                fillImg.type = Image.Type.Filled;
                fillImg.fillMethod = Image.FillMethod.Horizontal;
                fillImg.color = kit.Gold;
            }
            var handleImg = go.transform.Find("Handle Slide Area/Handle")?.GetComponent<Image>();
            if (handleImg != null)
            {
                handleImg.sprite = kit.SliderHandle;
                handleImg.type = Image.Type.Simple;
                handleImg.color = Color.white;
            }
            return go.GetComponent<Slider>();
        }

        private static void PositionTopCenter(RectTransform rect, Vector2 size, float topY)
        {
            if (rect == null) return;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, topY);
            rect.sizeDelta = size;
        }

        private static void PositionTopLeft(RectTransform rect, float x, float y, float width, float height)
        {
            if (rect == null) return;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
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
            string[] layers = { "Gameplay HUD", "Menu dimmer", "Main menu", "Zone select screen", "Settings screen", "Briefing screen", "Controls screen", "Pause screen", "Ending screen" };
            foreach (string name in layers)
            {
                Transform layer = canvas.transform.Find(name);
                if (layer != null && layer.parent != safe) layer.SetParent(safe, false);
            }
        }
    }
}
