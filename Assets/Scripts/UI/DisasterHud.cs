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

        private void Refresh()
        {
            if (restored != null && restored.Active)
            {
                RestoredProgress p = restored.Progress;
                view.MissionNumber.text = "ZONA HUTAN PULIH / MISI 05 / 05";
                view.MissionTitle.text = "JANJI UNTUK HUTAN";
                view.MissionDescription.text = restored.Instruction;
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
                view.MissionDescription.text = recovery.Instruction;
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
            view.MissionDescription.text = chapter.Instruction;
            view.Seedlings.text = "PERLENGKAPAN   <b>" + (progress.HasEquipment ? "SIAP" : "BELUM DIAMBIL") + "</b>";
            view.Trees.text = "PUING DIBERSIHKAN   <b>" + progress.Debris + " / " + progress.DebrisGoal + "</b>";
            view.Fires.text = "API PADAM   <b>" + progress.Fires + " / " + progress.FireGoal + "</b>";
            view.Restoration.text = Mathf.RoundToInt(progress.ChapterFraction * 100f) + "% BAB BENCANA";
            view.RestorationFill.fillAmount = progress.ChapterFraction;
            view.BriefingTargets.text = progress.FireGoal + " TITIK API / " + progress.DebrisGoal + " PUING"
                + (recovery == null ? " / ZONA 1" : " / " + recovery.Progress.TreeGoal + " BIBIT / ZONA 1–2");
        }

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
                        if (text.name == "Victory eyebrow") text.text = "ZONA PEMULIHAN / BAB SELESAI";
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
            speaker = "KOORDINATOR MISI";
            elderBriefing = false;
            rangerDialogue = false;
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
                recovery != null ? "Setiap pemulihan dimulai dari satu langkah. Temui Petugas Hutan di persemaian. Ia akan membimbingmu menanam bibit dan memulihkan habitat."
                    : "Kamu telah menyelesaikan bab Zona Bencana. Bab berikutnya masih dalam pengembangan."
            } : new[] { chapter.Instruction };
            page = 0;
            flow.Pause();
            view.Pause.SetActive(false);
            dialoguePanel.SetActive(true);
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
            dialogueText.text = "<b>" + speaker + "</b>\n\n" + pages[page] + "\n\n" + (page + 1) + " / " + pages.Length;
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
            if (coordinatorGreetingActive && coordinatorAnimator != null)
            {
                coordinatorAnimator.ResetTrigger("Lambai");
                coordinatorAnimator.updateMode = previousAnimatorMode;
                coordinatorGreetingActive = false;
            }
            dialoguePanel.SetActive(false);
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
        }

        public void OpenRangerDialogue()
        {
            if (flow.Mode != ScreenMode.Playing || DialogueOpen || recovery == null || !recovery.Active) return;
            acceptingBriefing = finishingChapter = false;
            rangerDialogue = true;
            elderBriefing = false;
            rangerStage = recovery.Progress.Stage;
            speaker = "PETUGAS HUTAN";
            pages = rangerStage == RecoveryStage.Briefing ? new[]
            {
                "Kamu sudah melakukan pekerjaan luar biasa. Api sudah padam dan kawasan ini bersih. Namun, hutan bukan sekadar kumpulan pohon; hutan adalah rumah bagi banyak makhluk hidup.",
                "Kita harus mengembalikan kehidupan itu. Ambil bibit di persemaian. Gali lubang pada lahan bertanda, tanam bibit, lalu siram setiap pohon muda. Beri mereka waktu untuk tumbuh."
            } : rangerStage == RecoveryStage.PlantingDebrief ? new[]
            {
                "Setiap bibit yang kamu tanam adalah janji untuk masa depan. Pohon-pohon muda telah disiram. Rawat mereka dan berikan waktu untuk tumbuh. Suatu hari nanti, tanah ini akan kembali hijau.",
                "Sekarang kita perlu memulihkan sungai dan habitat. Kumpulkan " + recovery.Progress.WasteGoal + " sampah di kedua tepian sungai. Gunakan jembatan untuk menyeberang, lalu pulihkan tempat berlindung dan tanaman penutup pada habitat bertanda."
            } : rangerStage == RecoveryStage.EcosystemDebrief ? new[]
            {
                "Lahan ini tidak lagi kosong. Pohon muda mulai tumbuh, air lebih bersih, dan tanda kehidupan satwa mulai muncul. Upayamu telah memberi hutan ini kesempatan kedua.",
                restored == null ? "Zona Pemulihan selesai. Zona Hutan Pulih masih dalam pengembangan."
                    : "Pemulihan membutuhkan waktu dan perawatan bersama. Musim demi musim, hutan akan tumbuh. Suatu hari, kembalilah dan temui Tetua Desa untuk melihat harapan yang telah kita tanam."
            } : new[] { recovery.Instruction };
            page = 0;
            flow.Pause();
            view.Pause.SetActive(false);
            dialoguePanel.SetActive(true);
            DisplayPage();
        }

        public void OpenElderDialogue()
        {
            if (flow.Mode != ScreenMode.Playing || DialogueOpen || restored == null || !restored.EnvironmentReady) return;
            rangerDialogue = acceptingBriefing = finishingChapter = false;
            elderBriefing = restored.Progress.Stage == RestoredStage.ElderBriefing;
            speaker = "TETUA DESA";
            pages = elderBriefing ? new[]
            {
                "Lihat apa yang telah kita capai. Dulu, hutan ini dipenuhi api dan abu. Desa kami diliputi ketakutan, dan kami khawatir akan kehilangan rumah ini.",
                "Kini pepohonan tumbuh kembali, burung-burung kembali, dan desa kami hidup lagi. Terima kasih. Kamu tidak hanya menyelamatkan hutan, tetapi membantu menghidupkan kembali rumah kami.",
                "Bawalah bibit terakhir ini ke lahan pemulihan desa. Tanam dan siram sebagai janji untuk menjaga hutan. Setelah itu, datangi titik pandang dan lihat apa yang telah kita pulihkan bersama."
            } : new[] { restored.Instruction };
            page = 0; flow.Pause(); view.Pause.SetActive(false); dialoguePanel.SetActive(true); DisplayPage();
        }
    }
}
