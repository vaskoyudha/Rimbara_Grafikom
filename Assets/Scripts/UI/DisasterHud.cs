using Rimbara.Interaction;
using Rimbara.Mission;
using Rimbara.Player;
using TMPro;
using UnityEngine;
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
        private string[] pages;
        private int page;
        private bool acceptingBriefing;
        private bool finishingChapter;
        [SerializeField] private Vector2 mapCenter = new Vector2(0f, 22f);
        [SerializeField] private Vector2 mapHalfSize = new Vector2(60f, 65f);
        public void ConfigureMap(Vector2 center, Vector2 halfSize) { mapCenter = center; mapHalfSize = halfSize; }
        public bool DialogueOpen => dialoguePanel.activeSelf;
        public Button DialogueNext => dialogueNext;
        public int DialoguePage => page;

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
        }

        private void Start() { Refresh(); }

        private void Update()
        {
            if (DialogueOpen)
            {
                if (flow.Mode != ScreenMode.Paused) CloseDialogue(false);
                else view.Pause.SetActive(false);
            }
            if (flow.Mode != ScreenMode.Playing && flow.Mode != ScreenMode.Paused) return;
            view.Toast.color = Color.white;
            view.PlayerDot.anchoredPosition = MapPosition(rig.transform.position);
            Transform target = chapter.Destination(rig.transform.position);
            view.TargetDot.gameObject.SetActive(target != null);
            view.Waypoint.text = target == null ? chapter.Title : chapter.DestinationLabel(target).ToUpperInvariant() + "   "
                + Vector3.Distance(rig.transform.position, target.position).ToString("0") + " m";
            if (target != null)
                view.TargetDot.anchoredPosition = MapPosition(target.position);
            bool aimed = chapter.CanExtinguish && sprayer.AimedFire != null && !sprayer.AimedFire.Extinguished;
            view.FireProgress.SetActive(aimed);
            if (aimed) view.FireFill.fillAmount = sprayer.AimedFire.SuppressionFraction;
        }

        private void Refresh()
        {
            DisasterProgress progress = chapter.Progress;
            view.MissionNumber.text = progress.Stage == DisasterStage.Briefing ? "PROLOG / POSKO DARURAT"
                : "ZONA BENCANA    " + (progress.Stage >= DisasterStage.CleanTheWasteland ? "02" : "01") + " / 02";
            view.MissionTitle.text = chapter.Title;
            view.MissionDescription.text = chapter.Instruction;
            view.Seedlings.text = "PERLENGKAPAN   <b>" + (progress.HasEquipment ? "SIAP" : "BELUM DIAMBIL") + "</b>";
            view.Trees.text = "PUING DIBERSIHKAN   <b>" + progress.Debris + " / " + progress.DebrisGoal + "</b>";
            view.Fires.text = "API PADAM   <b>" + progress.Fires + " / " + progress.FireGoal + "</b>";
            view.Restoration.text = Mathf.RoundToInt(progress.ChapterFraction * 100f) + "% BAB BENCANA";
            view.RestorationFill.fillAmount = progress.ChapterFraction;
            view.BriefingTargets.text = progress.FireGoal + " TITIK API  /  " + progress.DebrisGoal + " TUMPUKAN PUING  /  ZONA 1";
        }

        private Vector2 MapPosition(Vector3 point) => new Vector2(Mathf.Clamp((point.x - mapCenter.x) / Mathf.Max(1f, mapHalfSize.x), -1f, 1f) * 77f,
            Mathf.Clamp((point.z - mapCenter.y) / Mathf.Max(1f, mapHalfSize.y), -1f, 1f) * 82f);

        private void RefreshMode(ScreenMode mode)
        {
            Refresh();
            if (mode == ScreenMode.Victory)
                view.VictoryResult.text = "<b>" + chapter.Progress.Fires + " titik api dipadamkan / " + chapter.Progress.Debris
                    + " tumpukan puing dibersihkan</b>\nZona Bencana selesai.\n\nBab Pemulihan dan Hutan Pulih masih dalam pengembangan.\nIni belum akhir dari rangkaian lima misi.";
        }

        public void OpenCoordinatorDialogue()
        {
            if (flow.Mode != ScreenMode.Playing || DialogueOpen) return;
            acceptingBriefing = chapter.Progress.Stage == DisasterStage.Briefing;
            finishingChapter = chapter.Progress.Stage == DisasterStage.RecoveryReady;
            pages = acceptingBriefing ? new[]
            {
                "Selamat datang di posko darurat. Terima kasih sudah datang membantu.",
                "Kami menemukan beberapa titik api aktif di sekitar hutan. Misi pertamamu adalah memadamkan seluruh titik api yang tersisa.",
                "Ambil perlengkapan di samping posko. Dekati api yang ditandai, arahkan bidikan, lalu tahan KLIK KIRI untuk menyemprot. Setelah kawasan aman, kita bisa memulai pemulihan."
            } : finishingChapter ? new[]
            {
                "Lahan akhirnya bersih. Meski masih terlihat gersang, setiap upaya kecil membawa kita semakin dekat menuju pemulihan hutan.",
                "Setiap pemulihan dimulai dari satu langkah. Bab berikutnya akan menghadirkan bibit baru, air yang lebih bersih, dan habitat yang pulih.\n\nKamu telah menyelesaikan bab Zona Bencana dalam versi pengembangan ini."
            } : new[] { chapter.Instruction };
            page = 0;
            flow.Pause();
            view.Pause.SetActive(false);
            dialoguePanel.SetActive(true);
            DisplayPage();
        }

        private void DisplayPage()
        {
            dialogueText.text = "<b>KOORDINATOR MISI</b>\n\n" + pages[page] + "\n\n" + (page + 1) + " / " + pages.Length;
            dialogueNext.GetComponentInChildren<TMP_Text>().text = page + 1 == pages.Length ? "LANJUTKAN" : "BERIKUTNYA";
        }

        public void NextPage()
        {
            if (!DialogueOpen || flow.Mode != ScreenMode.Paused) return;
            page++;
            if (page < pages.Length) { DisplayPage(); return; }
            CloseDialogue(true);
        }

        private void CloseDialogue(bool completed)
        {
            dialoguePanel.SetActive(false);
            if (flow.Mode == ScreenMode.Paused) flow.Resume();
            if (completed && acceptingBriefing) chapter.AcceptBriefing();
            if (completed && finishingChapter) flow.CompleteDisasterChapter();
            acceptingBriefing = finishingChapter = false;
            Refresh();
        }

        private void OnDestroy()
        {
            if (chapter != null) chapter.Changed -= Refresh;
            if (flow != null) flow.ModeChanged -= RefreshMode;
            if (dialogueNext != null) dialogueNext.onClick.RemoveListener(NextPage);
        }
    }
}
