using Rimbara.Interaction;
using Rimbara.Mission;
using Rimbara.Player;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Rimbara.UI
{
    [DefaultExecutionOrder(220)]
    public sealed class DisasterHud : MonoBehaviour
    {
        [SerializeField] private DisasterChapter chapter;
        [SerializeField] private GameFlow flow;
        [SerializeField] private VolunteerRig rig;
        [SerializeField] private WaterSprayer sprayer;
        [SerializeField] private HudReferences view;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Button dialogueNext;
        [SerializeField] private Animator coordinatorAnimator;
        private AnimatorUpdateMode previousAnimatorMode;
        private bool coordinatorGreetingActive;
        private string[] pages;
        private int page;
        private bool acceptingBriefing;
        private bool finishingChapter;
        private RecoveryChapter recovery;
        private RecoveryStage rangerStage;
        private bool rangerDialogue;
        private RestoredChapter restored;
        private bool elderBriefing;
        private string speaker = "BIMA PRATAMA";
        private string speakerRole = "KOORDINATOR MISI";
        private string narration = string.Empty;
        private GameObject portraitFrame;
        private const float RevealCharactersPerSecond = 55f;
        private const float PanelFadeSeconds = 0.16f;
        private const float InputGraceSeconds = 0.22f;
        private const string SpeakerColor = "#C99A3F";
        private float revealElapsed;
        private int revealTotal;
        private bool revealing;
        private float dialogueOpenedAt;
        private CanvasGroup dialogueGroup;
        private Button dialogueSurface;
        private WaypointArrow3D waypointArrow;
        private TMP_Text waypointDistance;
        private GameObject waypointRoot;
        private GameObject crosshairH;
        private GameObject crosshairV;
        private readonly Button[] choiceButtons = new Button[2];
        private readonly TMP_Text[] choiceLabels = new TMP_Text[2];
        private RectTransform dialogueDivider;
        private DialogueOption[][] optionGroups;
        private string reactionOverride;
        private bool portraitShown;
        private Transform activeNpc;
        private Quaternion savedNpcRotation;
        private Quaternion savedPlayerRotation;
        private Quaternion playerTargetRot;
        private Quaternion npcTargetRot;
        private Animator activeNpcAnimator;
        private AnimatorUpdateMode savedNpcAnimMode;
        private AnimatorUpdateMode savedPlayerAnimMode;
        private CinemachineBrain cameraBrain;
        private bool brainWasEnabled;
        private bool ownsDialogueCamera;
        private Vector3 camStartPos;
        private Vector3 camTargetPos;
        private Quaternion camStartRot;
        private Quaternion camTargetRot;
        private float stageBlend;
        private GtaRadarController radar;
        [SerializeField] private Vector2 mapCenter = new Vector2(0f, 22f);
        [SerializeField] private Vector2 mapHalfSize = new Vector2(60f, 65f);
        public void ConfigureMap(Vector2 center, Vector2 halfSize) { mapCenter = center; mapHalfSize = halfSize; }
        public bool DialogueOpen => dialoguePanel.activeSelf;
        public Button DialogueNext => dialogueNext;
        public int DialoguePage => page;
        public bool DialogueRevealing => revealing;

        public void Configure(DisasterChapter owner, GameFlow gameFlow, VolunteerRig player, WaterSprayer water,
            HudReferences references, GameObject panel, TMP_Text text, Button next)
        {
            chapter = owner; flow = gameFlow; rig = player; sprayer = water;
            view = references; dialoguePanel = panel; dialogueText = text; dialogueNext = next;
        }

        private void Awake()
        {
            IndonesianText.ApplyStaticLabels(view.Canvas.transform);
            foreach (TextMeshPro label in FindObjectsByType<TextMeshPro>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                label.text = IndonesianText.WorldLabel(label.text);
            chapter.Changed += Refresh;
            recovery = chapter.Recovery;
            if (recovery != null) recovery.Changed += Refresh;
            restored = recovery == null ? null : recovery.Restored;
            if (restored != null) restored.Changed += Refresh;
            flow.ModeChanged += RefreshMode;
            dialogueNext.onClick.AddListener(NextPage);
            TMP_Text caption = dialogueNext.GetComponentInChildren<TMP_Text>();
            caption.rectTransform.anchorMin = Vector2.zero;
            caption.rectTransform.anchorMax = Vector2.one;
            caption.rectTransform.offsetMin = new Vector2(12f, 8f);
            caption.rectTransform.offsetMax = new Vector2(-12f, -8f);
            caption.alignment = TextAlignmentOptions.Center;
            caption.enableAutoSizing = true;
            caption.fontSizeMin = 14f;
            caption.fontSizeMax = 22f;

            RectTransform textRect = dialogueText.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(40f, 96f);
            textRect.offsetMax = new Vector2(-40f, -30f);
            RectTransform nextRect = dialogueNext.GetComponent<RectTransform>();
            nextRect.anchorMin = nextRect.anchorMax = new Vector2(1f, 0f);
            nextRect.pivot = new Vector2(0.5f, 0.5f);
            nextRect.anchoredPosition = new Vector2(-142f, 44f);
            nextRect.sizeDelta = new Vector2(220f, 46f);
            if (dialogueNext.GetComponent<ButtonHoverAnimation>() == null)
                dialogueNext.gameObject.AddComponent<ButtonHoverAnimation>();

            dialogueGroup = dialoguePanel.GetComponent<CanvasGroup>();
            if (dialogueGroup == null) dialogueGroup = dialoguePanel.AddComponent<CanvasGroup>();
            dialogueSurface = dialoguePanel.GetComponent<Button>();
            if (dialogueSurface == null) dialogueSurface = dialoguePanel.AddComponent<Button>();
            dialogueSurface.transition = Selectable.Transition.None;
            dialogueSurface.targetGraphic = dialoguePanel.GetComponent<Image>();
            dialogueSurface.onClick.AddListener(NextPage);
            Transform portrait = dialoguePanel.transform.Find("Dialogue portrait frame");
            if (portrait != null) portraitFrame = portrait.gameObject;
            Transform div = dialoguePanel.transform.Find("Dialogue divider");
            if (div != null) dialogueDivider = div as RectTransform;
            InitializeChoiceButtons();
            if (view != null && view.Waypoint != null && view.Waypoint.transform.parent != null)
            {
                waypointRoot = view.Waypoint.transform.parent.gameObject;
                HudTheme.ApplyReadableShadow(view.Waypoint, 0.25f);
                waypointArrow = view.Waypoint.transform.parent.GetComponentInChildren<WaypointArrow3D>(true);
                Transform dist = view.Waypoint.transform.parent.Find("Waypoint distance");
                if (dist != null)
                {
                    waypointDistance = dist.GetComponent<TMP_Text>();
                    HudTheme.ApplyReadableShadow(waypointDistance, 0.25f);
                }
            }
            if (view != null && view.Gameplay != null)
            {
                Transform ch = view.Gameplay.transform.Find("Crosshair horizontal");
                Transform cv = view.Gameplay.transform.Find("Crosshair vertical");
                if (ch != null) crosshairH = ch.gameObject;
                if (cv != null) crosshairV = cv.gameObject;
                radar = view.Gameplay.GetComponentInChildren<GtaRadarController>(true);
            }
        }

        private void Start() { Refresh(); }

        private void Update()
        {
            if (DialogueOpen)
            {
                if (flow.Mode != ScreenMode.Paused) CloseDialogue(false);
                else { view.Pause.SetActive(false); UpdateDialogue(); }
            }
            if (flow.Mode != ScreenMode.Playing && flow.Mode != ScreenMode.Paused) return;
            bool useGtaRadar = radar != null && radar.HasWorldMap;
            if (!useGtaRadar) view.PlayerDot.anchoredPosition = MapPosition(rig.transform.position);
            bool recovering = recovery != null && recovery.Active;
            Transform target = recovering ? recovery.Destination(rig.transform.position) : chapter.Destination(rig.transform.position);
            if (!useGtaRadar) view.TargetDot.gameObject.SetActive(target != null);
            string destination = target == null ? (recovering ? recovery.Title : chapter.Title)
                : (recovering ? recovery.DestinationLabel(target) : chapter.DestinationLabel(target));
            view.Waypoint.text = destination.ToUpperInvariant();
            if (waypointDistance != null)
                waypointDistance.text = target == null ? string.Empty
                    : Vector3.Distance(rig.transform.position, target.position).ToString("0") + " M";
            if (target != null && !useGtaRadar)
                view.TargetDot.anchoredPosition = MapPosition(target.position);
            if (restored != null && restored.Active)
            {
                target = restored.Destination;
                view.Waypoint.text = restored.DestinationLabel.ToUpperInvariant();
                if (waypointDistance != null)
                    waypointDistance.text = Vector3.Distance(rig.transform.position, target.position).ToString("0") + " M";
                if (!useGtaRadar) view.TargetDot.anchoredPosition = MapPosition(target.position);
            }
            if (waypointArrow != null)
                waypointArrow.UpdateDirection(rig.transform.position, rig.ViewCamera != null ? rig.ViewCamera.transform : null, target);
            bool aimed = chapter.CanExtinguish && sprayer.AimedFire != null && !sprayer.AimedFire.Extinguished;
            view.FireProgress.SetActive(aimed);
            if (aimed) view.FireFill.fillAmount = sprayer.AimedFire.SuppressionFraction;
            if (recovering && sprayer.AimedSeedling != null && sprayer.AimedSeedling.NeedsWater)
            {
                view.FireProgress.SetActive(true);
                view.FireFill.fillAmount = sprayer.AimedSeedling.WaterFraction;
            }
            if (sprayer.AimedFinalTree != null && sprayer.AimedFinalTree.NeedsWater)
            { view.FireProgress.SetActive(true); view.FireFill.fillAmount = sprayer.AimedFinalTree.WaterFraction; }
        }

        private void UpdateDialogue()
        {
            float elapsed = Time.unscaledTime - dialogueOpenedAt;
            if (dialogueGroup != null) dialogueGroup.alpha = Mathf.Clamp01(elapsed / PanelFadeSeconds);
            UpdateDialogueStage();

            if (revealing)
            {
                if (revealTotal < 0)
                {
                    revealTotal = ResolveRevealTotal();
                    if (revealTotal <= 0) { CompleteReveal(); return; }
                    dialogueText.maxVisibleCharacters = 0;
                }
                else
                {
                    revealElapsed += Time.unscaledDeltaTime;
                    int shown = Mathf.FloorToInt(revealElapsed * RevealCharactersPerSecond);
                    if (shown >= revealTotal) CompleteReveal();
                    else if (dialogueText != null) dialogueText.maxVisibleCharacters = shown;
                }
            }

            if (elapsed < InputGraceSeconds) return;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.digit1Key.wasPressedThisFrame && choiceButtons[0] != null && choiceButtons[0].gameObject.activeSelf)
                { ChooseOption(0); return; }
                if (keyboard.digit2Key.wasPressedThisFrame && choiceButtons[1] != null && choiceButtons[1].gameObject.activeSelf)
                { ChooseOption(1); return; }
                if (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame
                    || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
                    NextPage();
            }
        }

        private int ResolveRevealTotal()
        {
            if (dialogueText == null) return 0;
            dialogueText.maxVisibleCharacters = int.MaxValue;
            dialogueText.ForceMeshUpdate();
            return dialogueText.textInfo.characterCount;
        }

        private void CompleteReveal()
        {
            if (dialogueText == null) { revealing = false; return; }
            if (revealTotal < 0) revealTotal = ResolveRevealTotal();
            revealing = false;
            dialogueText.maxVisibleCharacters = revealTotal > 0 ? revealTotal : int.MaxValue;
        }

        private void Refresh()
        {
            if (restored != null && restored.Active)
            {
                RestoredProgress p = restored.Progress;
                view.MissionNumber.text = "ZONA HUTAN PULIH  <sprite name=\"diamond\" tint=1>  MISI 05 / 05";
                view.MissionTitle.text = "JANJI UNTUK HUTAN";
                view.MissionDescription.text = KeepPromptTogether(restored.Instruction);
                view.Seedlings.text = FormatCounter("BIBIT TERAKHIR", p.HasSeedling ? "DIBAWA 1" : "DIBAWA 0", p.HasSeedling);
                view.Trees.text = FormatCounter("DITANAM  /  DISIRAM", (p.Planted ? "1" : "0") + " / 1  •  " + (p.Watered ? "1" : "0") + " / 1", p.Planted && p.Watered);
                view.Fires.text = FormatCounter("TITIK PANDANG", p.ViewpointVisited ? "DIKUNJUNGI" : "BELUM", p.ViewpointVisited);
                view.Restoration.text = "MISI 05  <sprite name=\"diamond\" tint=1>  " + (p.Watered ? "Saksikan hasil pemulihan" : "Satu janji untuk masa depan");
                view.RestorationFill.fillAmount = ((p.Planted ? 1f : 0f) + (p.Watered ? 1f : 0f) + (p.ViewpointVisited ? 1f : 0f)) / 3f;
                return;
            }
            if (recovery != null && recovery.Active)
            {
                RecoveryProgress p = recovery.Progress;
                view.MissionNumber.text = "ZONA PEMULIHAN  <sprite name=\"diamond\" tint=1>  MISI " + (p.Stage >= RecoveryStage.Ecosystem ? "04" : "03") + " / 05";
                view.MissionTitle.text = recovery.Title;
                view.MissionDescription.text = KeepPromptTogether(recovery.Instruction);
                view.Seedlings.text = FormatCounter("BIBIT", p.Collected + " / " + p.TreeGoal + " (bawa " + p.Carried + ")", p.Collected >= p.TreeGoal);
                view.Trees.text = FormatCounter("POHON / SIRAM", p.Planted + "/" + p.TreeGoal + "  •  " + p.Watered + "/" + p.TreeGoal, p.Watered >= p.TreeGoal);
                view.Fires.text = FormatCounter("SAMPAH / HABITAT", p.Waste + "/" + p.WasteGoal + "  •  " + p.Habitats + "/" + p.HabitatGoal, p.Waste >= p.WasteGoal && p.Habitats >= p.HabitatGoal);
                view.Restoration.text = Mathf.RoundToInt(p.Fraction * 100f) + "% BAB PEMULIHAN";
                view.RestorationFill.fillAmount = p.Fraction;
                return;
            }
            DisasterProgress progress = chapter.Progress;
            view.MissionNumber.text = progress.Stage == DisasterStage.Briefing ? "PROLOG  <sprite name=\"diamond\" tint=1>  POSKO DARURAT"
                : "ZONA BENCANA  <sprite name=\"diamond\" tint=1>  " + (progress.Stage >= DisasterStage.CleanTheWasteland ? "02" : "01") + " / 02";
            view.MissionTitle.text = chapter.Title;
            view.MissionDescription.text = KeepPromptTogether(chapter.Instruction);
            view.Seedlings.text = FormatCounter("PERLENGKAPAN", progress.HasEquipment ? "SIAP" : "BELUM DIAMBIL", progress.HasEquipment);
            view.Trees.text = FormatCounter("PUING DIBERSIHKAN", progress.Debris + " / " + progress.DebrisGoal, progress.Debris >= progress.DebrisGoal);
            view.Fires.text = FormatCounter("API PADAM", progress.Fires + " / " + progress.FireGoal, progress.Fires >= progress.FireGoal);
            view.Restoration.text = Mathf.RoundToInt(progress.ChapterFraction * 100f) + "% BAB BENCANA";
            view.RestorationFill.fillAmount = progress.ChapterFraction;
            view.BriefingTargets.text = progress.FireGoal + " TITIK API / " + progress.DebrisGoal + " PUING"
                + (recovery == null ? " / ZONA 1" : " / " + recovery.Progress.TreeGoal + " BIBIT / ZONA 1–2");
        }

        private static string FormatCounter(string label, string value, bool complete)
        {
            string color = complete ? "#6F9E4A" : "#FFF0CC";
            return label + "<pos=206><b><color=" + color + ">" + value + "</color></b>";
        }

        private static string KeepPromptTogether(string text) =>
            string.IsNullOrEmpty(text) ? text : text.Replace(" E.", "\u00A0E.");

        private Vector2 MapPosition(Vector3 point) => new Vector2(Mathf.Clamp((point.x - mapCenter.x) / Mathf.Max(1f, mapHalfSize.x), -1f, 1f) * 77f,
            Mathf.Clamp((point.z - mapCenter.y) / Mathf.Max(1f, mapHalfSize.y), -1f, 1f) * 82f);

        private void RefreshMode(ScreenMode mode)
        {
            Refresh();
            if (mode == ScreenMode.Victory)
            {
                if (restored != null && restored.Progress.Stage == RestoredStage.Complete)
                {
                    foreach (TMP_Text text in view.Victory.GetComponentsInChildren<TMP_Text>(true))
                    {
                        if (text.name == "Victory eyebrow") text.text = "RIMBARA / PERJALANAN SELESAI";
                        if (text.name == "Victory title") text.text = "DARI ABU MENUJU KEHIDUPAN";
                        if (text.name == "Victory story") text.text = "Kamu bukan sekadar menyelesaikan misi.\nKamu membantu menghidupkan kembali hutan ini.";
                    }
                    view.VictoryResult.text = "<b>MISI 01–05 SELESAI</b>\n" + chapter.Progress.FireGoal + " titik api / " + chapter.Progress.DebrisGoal
                        + " lahan bersih / 5 bibit ditanam dan disiram\n10 sampah sungai / 2 habitat dipulihkan\n\nSetiap Pohon Menyimpan Cerita.\nTerima kasih, relawan.";
                    return;
                }
                if (recovery != null && recovery.Active)
                {
                    foreach (TMP_Text text in view.Victory.GetComponentsInChildren<TMP_Text>(true))
                    {
                        if (text.name == "Victory eyebrow") text.text = "ZONA PEMULIHAN  <sprite name=\"diamond\" tint=1>  BAB SELESAI";
                        if (text.name == "Victory title") text.text = "KESEMPATAN KEDUA";
                        if (text.name == "Victory story") text.text = "Pohon muda telah dirawat. Air lebih bersih. Habitat mulai pulih.\nHutan mendapat kesempatan kedua.";
                    }
                    view.VictoryResult.text = "<b>" + recovery.Progress.Watered + " bibit ditanam dan disiram / " + recovery.Progress.Habitats
                        + " habitat dipulihkan</b>\nZona 1 dan Zona 2 selesai.\n\nZona Hutan Pulih masih dalam pengembangan.\nIni belum akhir rangkaian lima misi.";
                    return;
                }
                view.VictoryResult.text = "<b>" + chapter.Progress.Fires + " titik api dipadamkan / " + chapter.Progress.Debris
                    + " tumpukan puing dibersihkan</b>\nZona Bencana selesai.\n\nBab Pemulihan dan Hutan Pulih masih dalam pengembangan.\nIni belum akhir dari rangkaian lima misi.";
            }
        }

        public void OpenCoordinatorDialogue() => OpenCoordinatorDialogue(null);

        public void OpenCoordinatorDialogue(Transform npcTransform)
        {
            if (flow.Mode != ScreenMode.Playing || DialogueOpen) return;
            speaker = DialogueCopy.CoordinatorName;
            speakerRole = DialogueCopy.CoordinatorSpeaker;
            SetDialoguePortraitVisible(true);
            elderBriefing = false;
            rangerDialogue = false;
            acceptingBriefing = chapter.Progress.Stage == DisasterStage.Briefing;
            finishingChapter = chapter.Progress.Stage == DisasterStage.RecoveryReady;
            pages = acceptingBriefing ? DialogueCopy.CoordinatorBriefing
                : finishingChapter ? (recovery != null ? DialogueCopy.CoordinatorDebrief
                    : new[] { "Kamu telah menyelesaikan Bab Zona Bencana. Bab berikutnya masih dalam pengembangan." })
                : new[] { chapter.Instruction };
            optionGroups = acceptingBriefing ? DialogueCopy.CoordinatorBriefingOptions
                : (finishingChapter && recovery != null) ? DialogueCopy.CoordinatorDebriefOptions
                : null;
            narration = acceptingBriefing
                ? "Bima Pratama, Koordinator Misi, menunjuk ke arah asap yang masih membubung di balik posko."
                : finishingChapter
                    ? "Bima Pratama menghela napas lega sambil memandang lahan yang mulai bersih."
                    : "Bima Pratama mengangguk, lalu mengulang arahan terakhirnya.";
            page = 0;
            reactionOverride = null;
            flow.Pause();
            view.Pause.SetActive(false);
            BeginDialogueStage(npcTransform != null ? npcTransform : FindFirstObjectByType<CoordinatorDialogue>()?.transform);
            ShowDialogue();
            DisplayPage();
        }

        private void DisplayPage()
        {
            string roleTag = string.IsNullOrEmpty(speakerRole)
                ? string.Empty
                : "  <size=80%><color=#F3E3C3>—  " + speakerRole + "</color></size>";
            string header = "<b><size=112%><color=" + SpeakerColor + ">" + speaker + "</color></size></b>" + roleTag;
            string beat = page == 0 && !string.IsNullOrEmpty(narration) && string.IsNullOrEmpty(reactionOverride)
                ? "\n<i><size=86%><color=#A89472>" + narration + "</color></size></i>" : string.Empty;
            string body = string.IsNullOrEmpty(reactionOverride) ? pages[page] : reactionOverride;
            string progress = pages.Length > 1
                ? "  <size=72%><alpha=#AA>(" + (page + 1) + " / " + pages.Length + ")<alpha=#FF></size>" : string.Empty;
            dialogueText.text = header + "\n<size=55%> </size>" + beat + "\n" + body + progress;
            dialogueText.maxVisibleCharacters = 0;
            revealTotal = -1;
            revealElapsed = 0f;
            revealing = true;
            dialogueNext.GetComponentInChildren<TMP_Text>().text = page + 1 == pages.Length ? "LANJUTKAN" : "BERIKUTNYA";
            RefreshChoiceButtons();
        }

        public void NextPage()
        {
            if (!DialogueOpen || flow.Mode != ScreenMode.Paused) return;
            if (revealing) { CompleteReveal(); return; }
            reactionOverride = null;
            page++;
            if (page < pages.Length) { DisplayPage(); return; }
            CloseDialogue(true);
        }

        private void ShowDialogue()
        {
            dialogueOpenedAt = Time.unscaledTime;
            if (dialogueGroup != null) dialogueGroup.alpha = 0f;
            if (waypointRoot != null) waypointRoot.SetActive(false);
            if (crosshairH != null) crosshairH.SetActive(false);
            if (crosshairV != null) crosshairV.SetActive(false);
            dialoguePanel.SetActive(true);
        }

        private void CloseDialogue(bool completed)
        {
            EndDialogueStage();
            if (waypointRoot != null) waypointRoot.SetActive(true);
            if (crosshairH != null) crosshairH.SetActive(true);
            if (crosshairV != null) crosshairV.SetActive(true);
            reactionOverride = null;
            optionGroups = null;
            SetChoiceButtonsActive(false);
            dialoguePanel.SetActive(false);
            revealing = false;
            if (flow.Mode == ScreenMode.Paused) flow.Resume();
            if (completed && acceptingBriefing) chapter.AcceptBriefing();
            if (completed && finishingChapter) flow.CompleteDisasterChapter();
            if (completed && rangerDialogue && recovery.Progress.Stage == rangerStage)
            {
                if (rangerStage == RecoveryStage.Briefing) recovery.AcceptBriefing();
                else if (rangerStage == RecoveryStage.PlantingDebrief) recovery.AcceptPlantingDebrief();
                else if (rangerStage == RecoveryStage.EcosystemDebrief)
                {
                    recovery.AcceptEcosystemDebrief();
                    flow.CompleteRecoveryChapter();
                }
            }
            if (completed && elderBriefing) restored.AcceptBriefing();
            elderBriefing = false;
            rangerDialogue = false;
            acceptingBriefing = finishingChapter = false;
            Refresh();
        }

        private void OnDestroy()
        {
            EndDialogueStage();
            if (recovery != null) recovery.Changed -= Refresh;
            if (restored != null) restored.Changed -= Refresh;
            if (chapter != null) chapter.Changed -= Refresh;
            if (flow != null) flow.ModeChanged -= RefreshMode;
            if (dialogueNext != null) dialogueNext.onClick.RemoveListener(NextPage);
            if (dialogueSurface != null) dialogueSurface.onClick.RemoveListener(NextPage);
            for (int i = 0; i < choiceButtons.Length; i++)
                if (choiceButtons[i] != null) choiceButtons[i].onClick.RemoveAllListeners();
        }

        private void SetDialoguePortraitVisible(bool visible)
        {
            portraitShown = visible && portraitFrame != null;
            if (portraitFrame != null) portraitFrame.SetActive(portraitShown);
            LayoutDialogueBodyAndChoices();
        }

        private void LayoutDialogueBodyAndChoices()
        {
            float left = portraitShown ? 276f : 40f;
            bool hasChoices = optionGroups != null && page >= 0 && page < optionGroups.Length
                && optionGroups[page] != null && optionGroups[page].Length > 0;
            if (dialogueText != null)
            {
                RectTransform textRect = dialogueText.rectTransform;
                textRect.offsetMin = new Vector2(left, hasChoices ? 124f : 90f);
                textRect.offsetMax = new Vector2(-40f, -24f);
            }
            float available = 1140f - left;
            if (dialogueDivider != null)
            {
                dialogueDivider.anchorMin = new Vector2(0f, 1f);
                dialogueDivider.anchorMax = new Vector2(0f, 1f);
                dialogueDivider.pivot = new Vector2(0f, 1f);
                dialogueDivider.anchoredPosition = new Vector2(left, -50f);
                dialogueDivider.sizeDelta = new Vector2(available, 12f);
            }
            const float gap = 14f;
            float width = (available - gap) * 0.5f;
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i] == null) continue;
                RectTransform rect = choiceButtons[i].GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.zero;
                rect.pivot = Vector2.zero;
                rect.sizeDelta = new Vector2(width, 38f);
                rect.anchoredPosition = new Vector2(left + i * (width + gap), 76f);
            }
        }

        private void InitializeChoiceButtons()
        {
            for (int i = 0; i < 2; i++)
            {
                int index = i;
                Transform existing = dialoguePanel.transform.Find("Dialogue choice " + i);
                GameObject go = existing != null ? existing.gameObject : Instantiate(dialogueNext.gameObject, dialoguePanel.transform);
                go.name = "Dialogue choice " + i;
                var btn = go.GetComponent<Button>();
                btn.onClick = new Button.ButtonClickedEvent();
                btn.onClick.AddListener(() => ChooseOption(index));
                var label = go.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    label.fontStyle = FontStyles.Normal;
                    label.enableAutoSizing = true;
                    label.fontSizeMin = 12f;
                    label.fontSizeMax = 15.5f;
                    label.alignment = TextAlignmentOptions.Center;
                }
                choiceButtons[i] = btn;
                choiceLabels[i] = label;
                go.SetActive(false);
            }
        }

        private void RefreshChoiceButtons()
        {
            LayoutDialogueBodyAndChoices();
            DialogueOption[] row = optionGroups != null && page >= 0 && page < optionGroups.Length ? optionGroups[page] : null;
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i] == null) continue;
                bool active = row != null && i < row.Length;
                choiceButtons[i].gameObject.SetActive(active);
                if (active && choiceLabels[i] != null)
                    choiceLabels[i].text = (i + 1) + ". " + row[i].Prompt;
            }
        }

        private void SetChoiceButtonsActive(bool active)
        {
            for (int i = 0; i < choiceButtons.Length; i++)
                if (choiceButtons[i] != null) choiceButtons[i].gameObject.SetActive(active);
        }

        public void ChooseOption(int index)
        {
            if (!DialogueOpen || flow.Mode != ScreenMode.Paused) return;
            if (revealing) { CompleteReveal(); return; }
            DialogueOption[] row = optionGroups != null && page >= 0 && page < optionGroups.Length ? optionGroups[page] : null;
            if (row == null || index < 0 || index >= row.Length) { NextPage(); return; }
            string reaction = row[index].NpcReaction;
            if (!string.IsNullOrEmpty(reaction) && reactionOverride != reaction)
            {
                reactionOverride = reaction;
                DisplayPage();
                return;
            }
            NextPage();
        }

        private void BeginDialogueStage(Transform npcTransform)
        {
            EndDialogueStage();
            if (rig == null) return;
            savedPlayerRotation = rig.transform.rotation;
            if (rig.Animator != null)
            {
                savedPlayerAnimMode = rig.Animator.updateMode;
                rig.Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                rig.Animator.speed = 1f;
                rig.Animator.SetFloat("Speed", 0f);
            }
            activeNpc = npcTransform;
            if (activeNpc != null)
            {
                savedNpcRotation = activeNpc.rotation;
                Vector3 toNpc = activeNpc.position - rig.transform.position;
                toNpc.y = 0f;
                Vector3 dir = toNpc.sqrMagnitude > 0.001f ? toNpc.normalized : rig.transform.forward;
                playerTargetRot = Quaternion.LookRotation(dir);
                npcTargetRot = Quaternion.LookRotation(-dir);

                activeNpcAnimator = activeNpc.GetComponentInChildren<Animator>(true) ?? coordinatorAnimator;
                if (activeNpcAnimator != null)
                {
                    savedNpcAnimMode = activeNpcAnimator.updateMode;
                    activeNpcAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
                    activeNpcAnimator.speed = 1f;
                    foreach (AnimatorControllerParameter p in activeNpcAnimator.parameters)
                        if (p.name == "Lambai" && p.type == AnimatorControllerParameterType.Trigger)
                        {
                            coordinatorGreetingActive = true;
                            activeNpcAnimator.SetTrigger("Lambai");
                            break;
                        }
                }

                if (rig.ViewCamera != null)
                {
                    cameraBrain = rig.ViewCamera.GetComponent<CinemachineBrain>();
                    brainWasEnabled = cameraBrain != null && cameraBrain.enabled;
                    if (cameraBrain != null) cameraBrain.enabled = false;
                    ownsDialogueCamera = true;
                    camStartPos = rig.ViewCamera.transform.position;
                    camStartRot = rig.ViewCamera.transform.rotation;

                    Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
                    Vector3 eyeNpc = activeNpc.position + Vector3.up * 1.48f;
                    Vector3 desiredPos = rig.transform.position + Vector3.up * 1.62f - dir * 0.95f + right * 0.56f;
                    if (Physics.Linecast(eyeNpc, desiredPos, out RaycastHit hit, 1 << 8, QueryTriggerInteraction.Ignore))
                        desiredPos = hit.point + (eyeNpc - desiredPos).normalized * 0.2f;
                    camTargetPos = desiredPos;
                    camTargetRot = Quaternion.LookRotation((eyeNpc - Vector3.up * 0.14f - desiredPos).normalized);
                }
            }
            stageBlend = 0f;
        }

        private void UpdateDialogueStage()
        {
            if (rig == null || activeNpc == null) return;
            stageBlend = Mathf.Clamp01(stageBlend + Time.unscaledDeltaTime * 3.2f);
            float s = Mathf.SmoothStep(0f, 1f, stageBlend);
            rig.transform.rotation = Quaternion.Slerp(savedPlayerRotation, playerTargetRot, s);
            activeNpc.rotation = Quaternion.Slerp(savedNpcRotation, npcTargetRot, s);
            if (ownsDialogueCamera && rig.ViewCamera != null)
                rig.ViewCamera.transform.SetPositionAndRotation(
                    Vector3.Lerp(camStartPos, camTargetPos, s),
                    Quaternion.Slerp(camStartRot, camTargetRot, s));
        }

        private void EndDialogueStage()
        {
            if (activeNpcAnimator != null)
            {
                if (coordinatorGreetingActive) activeNpcAnimator.ResetTrigger("Lambai");
                activeNpcAnimator.updateMode = savedNpcAnimMode;
                activeNpcAnimator = null;
            }
            coordinatorGreetingActive = false;
            if (activeNpc != null)
            {
                activeNpc.rotation = savedNpcRotation;
                activeNpc = null;
            }
            if (rig != null && rig.Animator != null)
                rig.Animator.updateMode = savedPlayerAnimMode;
            if (ownsDialogueCamera && cameraBrain != null)
                cameraBrain.enabled = brainWasEnabled;
            ownsDialogueCamera = false;
        }

        public void OpenRangerDialogue() => OpenRangerDialogue(null);

        public void OpenRangerDialogue(Transform npcTransform)
        {
            if (flow.Mode != ScreenMode.Playing || DialogueOpen || recovery == null || !recovery.Active) return;
            acceptingBriefing = finishingChapter = false;
            rangerDialogue = true;
            elderBriefing = false;
            rangerStage = recovery.Progress.Stage;
            speaker = DialogueCopy.RangerSpeaker;
            speakerRole = string.Empty;
            SetDialoguePortraitVisible(false);
            pages = rangerStage == RecoveryStage.Briefing ? DialogueCopy.RangerBriefing
                : rangerStage == RecoveryStage.PlantingDebrief ? DialogueCopy.RangerPlantingDebrief(recovery.Progress.WasteGoal)
                : rangerStage == RecoveryStage.EcosystemDebrief ? DialogueCopy.RangerEcosystemDebrief(restored != null)
                : new[] { recovery.Instruction };
            optionGroups = rangerStage == RecoveryStage.Briefing ? DialogueCopy.RangerBriefingOptions : null;
            narration = rangerStage == RecoveryStage.Briefing
                ? "Petugas Hutan menyerahkan sekotak bibit dengan hati-hati."
                : rangerStage == RecoveryStage.PlantingDebrief
                    ? "Petugas Hutan tersenyum melihat barisan pohon muda yang baru disiram."
                    : rangerStage == RecoveryStage.EcosystemDebrief
                        ? "Petugas Hutan menunjuk ke arah sungai yang mulai jernih."
                        : "Petugas Hutan memeriksa persemaian sambil berbicara.";
            page = 0;
            reactionOverride = null;
            flow.Pause();
            view.Pause.SetActive(false);
            BeginDialogueStage(npcTransform != null ? npcTransform : FindFirstObjectByType<RangerDialogue>()?.transform);
            ShowDialogue();
            DisplayPage();
        }

        public void OpenElderDialogue() => OpenElderDialogue(null);

        public void OpenElderDialogue(Transform npcTransform)
        {
            if (flow.Mode != ScreenMode.Playing || DialogueOpen || restored == null || !restored.EnvironmentReady) return;
            rangerDialogue = acceptingBriefing = finishingChapter = false;
            elderBriefing = restored.Progress.Stage == RestoredStage.ElderBriefing;
            speaker = DialogueCopy.ElderSpeaker;
            speakerRole = string.Empty;
            SetDialoguePortraitVisible(false);
            pages = elderBriefing ? DialogueCopy.ElderBriefing : new[] { restored.Instruction };
            optionGroups = elderBriefing ? DialogueCopy.ElderBriefingOptions : null;
            narration = elderBriefing
                ? "Tetua Desa berdiri di tengah desa, matanya berkaca-kaca memandang hutan yang kembali hijau."
                : "Tetua Desa tersenyum dan menepuk bahumu pelan.";
            page = 0;
            reactionOverride = null;
            flow.Pause();
            view.Pause.SetActive(false);
            BeginDialogueStage(npcTransform != null ? npcTransform : FindFirstObjectByType<ElderDialogue>()?.transform);
            ShowDialogue();
            DisplayPage();
        }
    }
}
