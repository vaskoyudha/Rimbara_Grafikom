using Rimbara.Audio;
using Rimbara.Interaction;
using Rimbara.Mission;
using Rimbara.Player;
using UnityEngine;

namespace Rimbara.UI
{
    [DefaultExecutionOrder(200)]
    public sealed class RimbaraHud : MonoBehaviour
    {
        [SerializeField] private HudReferences view;
        [SerializeField] private GameState state;
        [SerializeField] private MissionSystem missions;
        [SerializeField] private GameFlow flow;
        [SerializeField] private VolunteerActor actor;
        [SerializeField] private InteractionDetector detector;
        [SerializeField] private WaterSprayer sprayer;
        [SerializeField] private WorldGuide guide;
        [SerializeField] private VolunteerRig rig;
        [SerializeField] private GameAudio audioSystem;
        private float toastUntil;
        private WaypointArrow3D waypointArrow;
        private TMPro.TMP_Text waypointDistance;
        public HudReferences View => view;

        public void Configure(HudReferences references, GameState gameState, MissionSystem missionSystem,
            GameFlow gameFlow, VolunteerActor volunteer, InteractionDetector interaction, WaterSprayer water,
            WorldGuide worldGuide, VolunteerRig player, GameAudio sounds)
        {
            view = references;
            state = gameState;
            missions = missionSystem;
            flow = gameFlow;
            actor = volunteer;
            detector = interaction;
            sprayer = water;
            guide = worldGuide;
            rig = player;
            audioSystem = sounds;
        }

        private void Awake()
        {
            state.ProgressChanged += RefreshProgress;
            missions.MissionChanged += RefreshMission;
            flow.ModeChanged += RefreshMode;
            actor.ActionPerformed += ShowAction;
            view.Start.onClick.AddListener(flow.ShowZoneSelect);
            if (view.Continue != null) view.Continue.onClick.AddListener(flow.Continue);
            view.Begin.onClick.AddListener(flow.Begin);
            if (view.ZoneABegin != null) view.ZoneABegin.onClick.AddListener(flow.ShowBriefing);
            view.OpenControls.onClick.AddListener(flow.ShowControls);
            if (view.OpenSettings != null) view.OpenSettings.onClick.AddListener(flow.ShowSettings);
            if (view.OpenAchievements != null) view.OpenAchievements.onClick.AddListener(flow.ShowAchievements);
            view.ControlsBack.onClick.AddListener(flow.ShowMenu);
            view.BriefingBack.onClick.AddListener(flow.ShowZoneSelect);
            if (view.SettingsBack != null) view.SettingsBack.onClick.AddListener(flow.ShowMenu);
            if (view.AchievementsBack != null) view.AchievementsBack.onClick.AddListener(flow.ShowMenu);
            if (view.ZoneSelectBack != null) view.ZoneSelectBack.onClick.AddListener(flow.ShowMenu);
            view.Resume.onClick.AddListener(flow.Resume);
            view.PauseMenu.onClick.AddListener(flow.ReturnToMenu);
            view.Replay.onClick.AddListener(flow.Replay);
            view.VictoryMenu.onClick.AddListener(flow.ReturnToMenu);
            foreach (var button in view.Quit) button.onClick.AddListener(flow.Quit);
            view.Volume.SetValueWithoutNotify(audioSystem.Volume);
            view.Volume.onValueChanged.AddListener(val =>
            {
                audioSystem.SetVolume(val);
                if (view.MenuScreens != null) view.MenuScreens.SyncVolumeFromPause(val);
            });
            if (view.MenuScreens != null) view.MenuScreens.Bind(audioSystem, view.Volume);
            if (view.Waypoint != null && view.Waypoint.transform.parent != null)
            {
                HudTheme.ApplyReadableShadow(view.Waypoint, 0.25f);
                waypointArrow = view.Waypoint.transform.parent.GetComponentInChildren<WaypointArrow3D>(true);
                Transform dist = view.Waypoint.transform.parent.Find("Waypoint distance");
                if (dist != null)
                {
                    waypointDistance = dist.GetComponent<TMPro.TMP_Text>();
                    HudTheme.ApplyReadableShadow(waypointDistance, 0.25f);
                }
            }
        }

        private void Start()
        {
            RefreshProgress(state.Snapshot);
            RefreshMission(missions.Stage);
            RefreshMode(flow.Mode);
        }

        private void Update()
        {
            if (flow.Mode != ScreenMode.Playing && flow.Mode != ScreenMode.Paused) return;
            string prompt = detector.Prompt;
            if (string.IsNullOrEmpty(prompt)) prompt = sprayer.AimHint;
            view.Interaction.text = prompt;
            view.Interaction.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(prompt));
            if (guide.Destination != null)
            {
                Vector3 toTarget = guide.Destination.position - actor.transform.position;
                toTarget.y = 0f;
                Vector3 forward = rig.ViewCamera.transform.forward;
                forward.y = 0f;
                float angle = Vector3.SignedAngle(forward, toTarget, Vector3.up);
                string direction = angle > 35f ? "KANAN" : angle < -35f ? "KIRI" : "DEPAN";
                if (Mathf.Abs(angle) > 140f) direction = "BELAKANG";
                view.Waypoint.text = guide.DestinationName.ToUpperInvariant() + "  /  " + direction;
                if (waypointDistance != null) waypointDistance.text = guide.Distance.ToString("0") + " M";
            }
            if (waypointArrow != null)
                waypointArrow.UpdateDirection(actor.transform.position, rig.ViewCamera != null ? rig.ViewCamera.transform : null, guide.Destination);
            view.PlayerDot.anchoredPosition = MapPosition(actor.transform.position);
            view.TargetDot.gameObject.SetActive(guide.Destination != null);
            if (guide.Destination != null) view.TargetDot.anchoredPosition = MapPosition(guide.Destination.position);
            bool aimed = sprayer.AimedFire != null && missions.Stage == MissionStage.StopTheFire && !sprayer.AimedFire.Extinguished;
            view.FireProgress.SetActive(aimed);
            if (aimed) view.FireFill.fillAmount = sprayer.AimedFire.SuppressionFraction;
            view.Toast.transform.parent.gameObject.SetActive(Time.unscaledTime < toastUntil);
        }

        private static Vector2 MapPosition(Vector3 world)
        {
            return new Vector2(Mathf.Clamp(world.x / 60f, -1f, 1f) * 77f,
                Mathf.Clamp((world.z - 22f) / 65f, -1f, 1f) * 82f);
        }

        private void RefreshProgress(ProgressSnapshot progress)
        {
            if (missions.DisasterChapter != null) return;
            ApplyScore(BuildView(progress, missions.Stage));
        }

        private void RefreshMission(MissionStage stage)
        {
            if (missions.DisasterChapter != null) return;
            RestorationScoreView model = BuildView(state.Snapshot, stage);
            ApplyScore(model);
            if (flow.Mode == ScreenMode.Playing)
                ShowToast("MISI " + model.MissionNumber + "  /  " + model.MissionTitle, true);
        }

        private RestorationScoreView BuildView(ProgressSnapshot snapshot, MissionStage stage)
        {
            return RestorationScoreView.From(snapshot, stage, missions.Title, missions.Instruction, flow.ElapsedSeconds);
        }

        public void ApplyScore(RestorationScoreView model)
        {
            WriteCounters(view, model);
            WriteMission(view, model);
        }

        public static void WriteCounters(HudReferences target, RestorationScoreView model)
        {
            RestorationScore score = model.Score;
            target.Seedlings.text = "BIBIT   <b>" + score.Seedlings + " / " + score.SeedlingGoal + "</b>"
                + "  <color=#8A6242>(dibawa " + score.CarriedSeedlings + ")</color>";
            target.Trees.text = "POHON DITANAM   <b>" + score.Trees + " / " + score.TreeGoal + "</b>";
            target.Fires.text = "API PADAM   <b>" + score.Fires + " / " + score.FireGoal + "</b>";
            target.Restoration.text = score.Percent + "% DIPULIHKAN";
            target.RestorationFill.fillAmount = score.Fraction;
            target.BriefingTargets.text = score.SeedlingGoal + " BIBIT LOKAL   /   " + score.TreeGoal + " POHON   /   " + score.FireGoal + " TITIK API";
        }

        public static void WriteMission(HudReferences target, RestorationScoreView model)
        {
            target.MissionNumber.text = "MISI LAPANGAN    " + model.MissionNumber.ToString("00") + " / " + model.ChapterCount.ToString("00");
            target.MissionTitle.text = model.MissionTitle;
            target.MissionDescription.text = model.MissionInstruction;
        }

        private void RefreshMode(ScreenMode mode)
        {
            view.Gameplay.SetActive(mode == ScreenMode.Playing || mode == ScreenMode.Paused);
            view.Menu.SetActive(mode == ScreenMode.Menu);
            if (view.ZoneSelect != null) view.ZoneSelect.SetActive(mode == ScreenMode.ZoneSelect);
            if (view.Settings != null) view.Settings.SetActive(mode == ScreenMode.Settings);
            if (view.Achievements != null) view.Achievements.SetActive(mode == ScreenMode.Achievements);
            view.Briefing.SetActive(mode == ScreenMode.Briefing);
            view.Controls.SetActive(mode == ScreenMode.Controls);
            view.Pause.SetActive(mode == ScreenMode.Paused);
            view.Victory.SetActive(mode == ScreenMode.Victory);
            view.Dimmer.SetActive(mode != ScreenMode.Playing);
            if (view.MenuScreens != null)
            {
                if (mode == ScreenMode.Menu) view.MenuScreens.RefreshContinueState(flow.CanContinue);
                RestorationScore s = RestorationScore.From(state.Snapshot, 1);
                view.MenuScreens.SyncGameplayBadges(s.Fires, s.FireGoal, s.Trees, mode == ScreenMode.Playing, mode == ScreenMode.Victory);
                if (mode == ScreenMode.Achievements) view.MenuScreens.RefreshAchievements();
            }
            if (mode == ScreenMode.ZoneSelect && view.MenuScreens != null)
            {
                float frac = missions.DisasterChapter != null
                    ? missions.DisasterChapter.Progress.ChapterFraction
                    : RestorationScore.From(state.Snapshot, 1).Fraction;
                view.MenuScreens.RefreshZoneProgress(frac);
            }
            if (mode == ScreenMode.Victory)
            {
                RestorationScoreView model = BuildView(state.Snapshot, missions.Stage);
                int minutes = Mathf.FloorToInt(flow.ElapsedSeconds / 60f);
                int seconds = Mathf.FloorToInt(flow.ElapsedSeconds % 60f);
                view.VictoryResult.text = "<b>" + model.Score.Trees + " pohon ditanam   /   " + model.Score.Fires + " titik api dipadamkan</b>\n"
                    + "Seluruh misi pemulihan selesai\n\nWaktu di lapangan  " + minutes.ToString("00") + ":" + seconds.ToString("00");
            }
        }

        private void ShowAction(InteractionResult result) { ShowToast(result.Message, result.Success); }

        public void ShowToast(string text, bool success = true)
        {
            view.Toast.text = text;
            view.Toast.color = success ? new Color32(0x2F, 0x6B, 0x1F, 0xFF) : new Color32(0xA3, 0x2B, 0x22, 0xFF);
            toastUntil = Time.unscaledTime + 5.5f;
            view.Toast.transform.parent.gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            if (state != null) state.ProgressChanged -= RefreshProgress;
            if (missions != null) missions.MissionChanged -= RefreshMission;
            if (flow != null) flow.ModeChanged -= RefreshMode;
            if (actor != null) actor.ActionPerformed -= ShowAction;
        }
    }
}
