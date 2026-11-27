using Rimbara.Interaction;
using Rimbara.Mission;
using Rimbara.Player;
using TMPro;
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
        private string speaker = "KOORDINATOR MISI";
        private string narration = string.Empty;
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
            nextRect.anchoredPosition = new Vector2(-152f, 52f);
            nextRect.sizeDelta = new Vector2(240f, 56f);
            if (dialogueNext.GetComponent<ButtonHoverAnimation>() == null)
                dialogueNext.gameObject.AddComponent<ButtonHoverAnimation>();

            dialogueGroup = dialoguePanel.GetComponent<CanvasGroup>();
            if (dialogueGroup == null) dialogueGroup = dialoguePanel.AddComponent<CanvasGroup>();
            dialogueSurface = dialoguePanel.GetComponent<Button>();
            if (dialogueSurface == null) dialogueSurface = dialoguePanel.AddComponent<Button>();
            dialogueSurface.transition = Selectable.Transition.None;
            dialogueSurface.targetGraphic = dialoguePanel.GetComponent<Image>();
            dialogueSurface.onClick.AddListener(NextPage);
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
            view.PlayerDot.anchoredPosition = MapPosition(rig.transform.position);
            bool recovering = recovery != null && recovery.Active;
            Transform target = recovering ? recovery.Destination(rig.transform.position) : chapter.Destination(rig.transform.position);
            view.TargetDot.gameObject.SetActive(target != null);
            string destination = target == null ? (recovering ? recovery.Title : chapter.Title)
                : (recovering ? recovery.DestinationLabel(target) : chapter.DestinationLabel(target));
            view.Waypoint.text = destination.ToUpperInvariant() + (target == null ? string.Empty
                : "   " + Vector3.Distance(rig.transform.position, target.position).ToString("0") + " m");
            if (target != null)
                view.TargetDot.anchoredPosition = MapPosition(target.position);
            if (restored != null && restored.Active)
            {
                target = restored.Destination;
                view.Waypoint.text = restored.DestinationLabel.ToUpperInvariant() + "   " + Vector3.Distance(rig.transform.position, target.position).ToString("0") + " m";
                view.TargetDot.anchoredPosition = MapPosition(target.position);
            }
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
            if (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame
                || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
                NextPage();
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
                view.MissionNumber.text = "ZONA HUTAN PULIH / MISI 05 / 05";
                view.MissionTitle.text = "JANJI UNTUK HUTAN";
                view.MissionDescription.text = KeepPromptTogether(restored.Instruction);
                view.Seedlings.text = "BIBIT TERAKHIR / DIBAWA " + (p.HasSeedling ? "1" : "0");
                view.Trees.text = "DITANAM " + (p.Planted ? "1" : "0") + " / 1   DISIRAM " + (p.Watered ? "1" : "0") + " / 1";
                view.Fires.text = "TITIK PANDANG / " + (p.ViewpointVisited ? "DIKUNJUNGI" : "BELUM DIKUNJUNGI");
                view.Restoration.text = "MISI 05 / " + (p.Watered ? "Saksikan hasil pemulihan" : "Satu janji untuk masa depan");
                view.RestorationFill.fillAmount = ((p.Planted ? 1f : 0f) + (p.Watered ? 1f : 0f) + (p.ViewpointVisited ? 1f : 0f)) / 3f;
                return;
            }
            if (recovery != null && recovery.Active)
            {
                RecoveryProgress p = recovery.Progress;
                view.MissionNumber.text = "ZONA PEMULIHAN / MISI " + (p.Stage >= RecoveryStage.Ecosystem ? "04" : "03") + " / 05";
                view.MissionTitle.text = recovery.Title;
                view.MissionDescription.text = KeepPromptTogether(recovery.Instruction);
                view.Seedlings.text = "BIBIT DIBAWA   <b>" + p.Carried + "</b> / DIAMBIL " + p.Collected + " / " + p.TreeGoal;
                view.Trees.text = "DITANAM   " + p.Planted + " / " + p.TreeGoal + "   DISIRAM   " + p.Watered + " / " + p.TreeGoal;
                view.Fires.text = "SAMPAH   " + p.Waste + " / " + p.WasteGoal + "   HABITAT   " + p.Habitats + " / " + p.HabitatGoal;
                view.Restoration.text = Mathf.RoundToInt(p.Fraction * 100f) + "% BAB PEMULIHAN";
                view.RestorationFill.fillAmount = p.Fraction;
                return;
            }
            DisasterProgress progress = chapter.Progress;
            view.MissionNumber.text = progress.Stage == DisasterStage.Briefing ? "PROLOG / POSKO DARURAT"
                : "ZONA BENCANA    " + (progress.Stage >= DisasterStage.CleanTheWasteland ? "02" : "01") + " / 02";
            view.MissionTitle.text = chapter.Title;
            view.MissionDescription.text = KeepPromptTogether(chapter.Instruction);
            view.Seedlings.text = "PERLENGKAPAN   <b>" + (progress.HasEquipment ? "SIAP" : "BELUM DIAMBIL") + "</b>";
            view.Trees.text = "PUING DIBERSIHKAN   <b>" + progress.Debris + " / " + progress.DebrisGoal + "</b>";
            view.Fires.text = "API PADAM   <b>" + progress.Fires + " / " + progress.FireGoal + "</b>";
            view.Restoration.text = Mathf.RoundToInt(progress.ChapterFraction * 100f) + "% BAB BENCANA";
            view.RestorationFill.fillAmount = progress.ChapterFraction;
            view.BriefingTargets.text = progress.FireGoal + " TITIK API / " + progress.DebrisGoal + " PUING"
                + (recovery == null ? " / ZONA 1" : " / " + recovery.Progress.TreeGoal + " BIBIT / ZONA 1–2");
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

        public void OpenCoordinatorDialogue()
        {
            if (flow.Mode != ScreenMode.Playing || DialogueOpen) return;
            speaker = DialogueCopy.CoordinatorSpeaker;
            elderBriefing = false;
            rangerDialogue = false;
            acceptingBriefing = chapter.Progress.Stage == DisasterStage.Briefing;
            finishingChapter = chapter.Progress.Stage == DisasterStage.RecoveryReady;
            pages = acceptingBriefing ? DialogueCopy.CoordinatorBriefing
                : finishingChapter ? (recovery != null ? DialogueCopy.CoordinatorDebrief
                    : new[] { "Kamu telah menyelesaikan Bab Zona Bencana. Bab berikutnya masih dalam pengembangan." })
                : new[] { chapter.Instruction };
            narration = acceptingBriefing
                ? "Koordinator Misi menunjuk ke arah asap yang masih membubung di balik posko."
                : finishingChapter
                    ? "Koordinator Misi menghela napas lega sambil memandang lahan yang mulai bersih."
                    : "Koordinator Misi mengangguk, lalu mengulang arahan terakhirnya.";
            page = 0;
            flow.Pause();
            view.Pause.SetActive(false);
            ShowDialogue();
            DisplayPage();
            if (coordinatorAnimator != null)
            {
                previousAnimatorMode = coordinatorAnimator.updateMode;
                coordinatorAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
                coordinatorGreetingActive = true;
                coordinatorAnimator.SetTrigger("Lambai");
            }
        }

        private void DisplayPage()
        {
            string header = "<b><size=112%><color=" + SpeakerColor + ">" + speaker + "</color></size></b>";
            string divider = dialogueText != null && dialogueText.spriteAsset != null
                ? "\n<size=62%><sprite name=\"diamond\" tint=1></size>" : string.Empty;
            string beat = page == 0 && !string.IsNullOrEmpty(narration)
                ? "\n<i><size=88%><color=#A89472>" + narration + "</color></size></i>" : string.Empty;
            string progress = pages.Length > 1
                ? "\n\n<size=70%><alpha=#AA>" + (page + 1) + " / " + pages.Length + "<alpha=#FF></size>" : string.Empty;
            dialogueText.text = header + divider + beat + "\n\n" + pages[page] + progress;
            dialogueText.maxVisibleCharacters = 0;
            revealTotal = -1;
            revealElapsed = 0f;
            revealing = true;
            dialogueNext.GetComponentInChildren<TMP_Text>().text = page + 1 == pages.Length ? "LANJUTKAN" : "BERIKUTNYA";
        }

        public void NextPage()
        {
            if (!DialogueOpen || flow.Mode != ScreenMode.Paused) return;
            if (revealing) { CompleteReveal(); return; }
            page++;
            if (page < pages.Length) { DisplayPage(); return; }
            CloseDialogue(true);
        }

        private void ShowDialogue()
        {
            dialogueOpenedAt = Time.unscaledTime;
            if (dialogueGroup != null) dialogueGroup.alpha = 0f;
            dialoguePanel.SetActive(true);
        }

        private void CloseDialogue(bool completed)
        {
            if (coordinatorGreetingActive && coordinatorAnimator != null)
            {
                coordinatorAnimator.ResetTrigger("Lambai");
                coordinatorAnimator.updateMode = previousAnimatorMode;
                coordinatorGreetingActive = false;
            }
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
            if (recovery != null) recovery.Changed -= Refresh;
            if (restored != null) restored.Changed -= Refresh;
            if (chapter != null) chapter.Changed -= Refresh;
            if (flow != null) flow.ModeChanged -= RefreshMode;
            if (dialogueNext != null) dialogueNext.onClick.RemoveListener(NextPage);
            if (dialogueSurface != null) dialogueSurface.onClick.RemoveListener(NextPage);
        }

        public void OpenRangerDialogue()
        {
            if (flow.Mode != ScreenMode.Playing || DialogueOpen || recovery == null || !recovery.Active) return;
            acceptingBriefing = finishingChapter = false;
            rangerDialogue = true;
            elderBriefing = false;
            rangerStage = recovery.Progress.Stage;
            speaker = DialogueCopy.RangerSpeaker;
            pages = rangerStage == RecoveryStage.Briefing ? DialogueCopy.RangerBriefing
                : rangerStage == RecoveryStage.PlantingDebrief ? DialogueCopy.RangerPlantingDebrief(recovery.Progress.WasteGoal)
                : rangerStage == RecoveryStage.EcosystemDebrief ? DialogueCopy.RangerEcosystemDebrief(restored != null)
                : new[] { recovery.Instruction };
            narration = rangerStage == RecoveryStage.Briefing
                ? "Petugas Hutan menyerahkan sekotak bibit dengan hati-hati."
                : rangerStage == RecoveryStage.PlantingDebrief
                    ? "Petugas Hutan tersenyum melihat barisan pohon muda yang baru disiram."
                    : rangerStage == RecoveryStage.EcosystemDebrief
                        ? "Petugas Hutan menunjuk ke arah sungai yang mulai jernih."
                        : "Petugas Hutan memeriksa persemaian sambil berbicara.";
            page = 0;
            flow.Pause();
            view.Pause.SetActive(false);
            ShowDialogue();
            DisplayPage();
        }

        public void OpenElderDialogue()
        {
            if (flow.Mode != ScreenMode.Playing || DialogueOpen || restored == null || !restored.EnvironmentReady) return;
            rangerDialogue = acceptingBriefing = finishingChapter = false;
            elderBriefing = restored.Progress.Stage == RestoredStage.ElderBriefing;
            speaker = DialogueCopy.ElderSpeaker;
            pages = elderBriefing ? DialogueCopy.ElderBriefing : new[] { restored.Instruction };
            narration = elderBriefing
                ? "Tetua Desa berdiri di tengah desa, matanya berkaca-kaca memandang hutan yang kembali hijau."
                : "Tetua Desa tersenyum dan menepuk bahumu pelan.";
            page = 0; flow.Pause(); view.Pause.SetActive(false); ShowDialogue(); DisplayPage();
        }
    }
}
