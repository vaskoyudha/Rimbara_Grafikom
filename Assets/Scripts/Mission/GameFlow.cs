using System;
using Rimbara.Player;
using Rimbara.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rimbara.Mission
{
    public enum ScreenMode { Menu, ZoneSelect, Settings, Achievements, Briefing, Controls, Playing, Paused, Victory, Transition, Cinematic, Inventory }

    [DefaultExecutionOrder(-40)]
    public sealed class GameFlow : MonoBehaviour
    {
        [SerializeField] private GameState state;
        [SerializeField] private MissionSystem missions;
        [SerializeField] private VolunteerRig rig;
        [SerializeField] private VolunteerInput input;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private OpeningDirector opening;
        private static ScreenMode nextLaunchMode = ScreenMode.Menu;
        private ScreenMode resumeMode = ScreenMode.Playing;
        private bool sessionStarted;
        private DisasterChapter subscribedDisaster;
        private RecoveryChapter subscribedRecovery;
        private RestoredChapter subscribedRestored;
        public ScreenMode Mode { get; private set; } = ScreenMode.Menu;
        public float ElapsedSeconds { get; private set; }
        public OpeningDirector Opening => opening;
        public bool DiagnosticMode => CommandLine.Value("--rimbara-smoke") != null;
        public bool CanContinue => !DiagnosticMode && GameSaveSystem.HasSaveForScene(SceneManager.GetActiveScene().name);
        public void ConfigureOpening(OpeningDirector openingDirector) => opening = openingDirector;
        public event Action<ScreenMode> ModeChanged;

        public void Configure(GameState gameState, MissionSystem missionSystem, VolunteerRig player, VolunteerInput controls)
        {
            state = gameState;
            missions = missionSystem;
            rig = player;
            input = controls;
        }

        private void Awake() { missions.Completed += ShowVictory; }

        private void Start()
        {
            SubscribeChapters();
            ScreenMode initial = nextLaunchMode;
            nextLaunchMode = ScreenMode.Menu;
            SetMode(initial);
        }

        private void SubscribeChapters()
        {
            subscribedDisaster = missions != null ? missions.DisasterChapter : null;
            if (subscribedDisaster != null)
            {
                subscribedDisaster.Changed += SaveCurrentProgress;
                subscribedRecovery = subscribedDisaster.Recovery;
                if (subscribedRecovery != null)
                {
                    subscribedRecovery.Changed += SaveCurrentProgress;
                    subscribedRestored = subscribedRecovery.Restored;
                    if (subscribedRestored != null) subscribedRestored.Changed += SaveCurrentProgress;
                }
            }
        }

        private void Update()
        {
            if (Mode == ScreenMode.Inventory)
            {
                if (input.InventoryPressed || input.PausePressed)
                    SetMode(ScreenMode.Playing);

                return;
            }
            if (Mode == ScreenMode.Playing && input.InventoryPressed && inventoryPanel != null)
            {
                SetMode(ScreenMode.Inventory);
                return;
            }
            if (Mode == ScreenMode.Playing) ElapsedSeconds += Time.deltaTime;
            if (input.PausePressed)
            {
                if (Mode == ScreenMode.Playing || Mode == ScreenMode.Cinematic || Mode == ScreenMode.Transition) Pause();
                else if (Mode == ScreenMode.Paused) Resume();
                else if (Mode == ScreenMode.Briefing || Mode == ScreenMode.Controls
                    || Mode == ScreenMode.ZoneSelect || Mode == ScreenMode.Settings
                    || Mode == ScreenMode.Achievements) ShowMenu();
            }
        }

        public void ShowMenu() { SetMode(ScreenMode.Menu); }
        public void ShowZoneSelect() { SetMode(ScreenMode.ZoneSelect); }
        public void ShowSettings() { SetMode(ScreenMode.Settings); }
        public void ShowAchievements() { SetMode(ScreenMode.Achievements); }
        public void ShowBriefing() { SetMode(ScreenMode.Briefing); }
        public void ShowControls() { SetMode(ScreenMode.Controls); }

        public void Begin()
        {
            if (Mode != ScreenMode.Briefing && Mode != ScreenMode.Menu) return;
            if (!DiagnosticMode) GameSaveSystem.Clear();
            if (opening != null && opening.TryBegin()) return;
            SetMode(ScreenMode.Playing);
        }

        /// <summary>Loads the saved game checkpoint for the active scene and resumes playing.</summary>
        public void Continue()
        {
            if (Mode != ScreenMode.Menu) return;
            string sceneName = SceneManager.GetActiveScene().name;
            if (!GameSaveSystem.TryLoadForScene(sceneName, out SavedProgressData data))
            {
                ShowZoneSelect();
                return;
            }
            if (opening != null) opening.MarkCompleted();
            GameSaveSystem.Apply(data, state, missions, rig);
            ElapsedSeconds = data.elapsedSeconds;
            sessionStarted = true;
            SetMode(ScreenMode.Playing);
        }

        public void BeginOpeningCinematic() => SetMode(ScreenMode.Cinematic);

        public void EndOpeningCinematic()
        {
            if (Mode == ScreenMode.Cinematic || Mode == ScreenMode.Paused)
                SetMode(ScreenMode.Playing);
        }

        public void Pause()
        {
            if (Mode != ScreenMode.Playing && Mode != ScreenMode.Cinematic && Mode != ScreenMode.Transition) return;
            SaveCurrentProgress();
            resumeMode = Mode; SetMode(ScreenMode.Paused);
        }
        public void Resume() { if (Mode == ScreenMode.Paused) SetMode(resumeMode); }
        private void ShowVictory()
        {
            sessionStarted = false;
            if (!DiagnosticMode) GameSaveSystem.Clear();
            SetMode(ScreenMode.Victory);
        }

        public void CompleteDisasterChapter()
        {
            DisasterChapter chapter = missions.DisasterChapter;
            if (Mode == ScreenMode.Playing && chapter != null && chapter.Progress.Stage == DisasterStage.RecoveryReady)
            {
                if (chapter.Recovery != null) chapter.Recovery.Begin();
                else SetMode(ScreenMode.Victory);
            }
        }

        public void CompleteRecoveryChapter()
        {
            RecoveryChapter recovery = missions.DisasterChapter == null ? null : missions.DisasterChapter.Recovery;
            if (Mode == ScreenMode.Playing && recovery != null && recovery.Progress.Stage == RecoveryStage.Complete)
            {
                if (recovery.Restored != null) recovery.Restored.Begin();
                else SetMode(ScreenMode.Victory);
            }
        }
        public void BeginStorySequence(bool ending) { SetMode(ending ? ScreenMode.Cinematic : ScreenMode.Transition); }
        public void EndStorySequence(bool ending)
        {
            RestoredChapter restored = missions.DisasterChapter == null || missions.DisasterChapter.Recovery == null ? null : missions.DisasterChapter.Recovery.Restored;
            if (restored == null) return;
            if (ending && restored.Progress.Stage == RestoredStage.Complete) SetMode(ScreenMode.Victory);
            else if (!ending && restored.Progress.Stage == RestoredStage.ElderBriefing) SetMode(ScreenMode.Playing);
        }

        public void Replay()
        {
            sessionStarted = false;
            if (!DiagnosticMode) GameSaveSystem.Clear();
            nextLaunchMode = ScreenMode.Briefing;
            ReloadScene();
        }

        public void ReturnToMenu()
        {
            SaveCurrentProgress();
            nextLaunchMode = ScreenMode.Menu;
            ReloadScene();
        }

        private static void ReloadScene()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Quit()
        {
            SaveCurrentProgress();
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void SaveCurrentProgress()
        {
            if (DiagnosticMode || !sessionStarted || Mode == ScreenMode.Victory || Mode == ScreenMode.Menu) return;
            GameSaveSystem.Save(SceneManager.GetActiveScene().name, ElapsedSeconds, state, missions, rig);
        }

        private void SetMode(ScreenMode mode)
        {
            Mode = mode;
            if (mode == ScreenMode.Playing && !sessionStarted)
            {
                sessionStarted = true;
                SaveCurrentProgress();
            }

            bool playing = mode == ScreenMode.Playing;
            bool bagOpen = mode == ScreenMode.Inventory;
            bool frozen = mode == ScreenMode.Paused || bagOpen;
            bool gameplayView = playing || frozen || mode == ScreenMode.Victory
                || mode == ScreenMode.Transition || mode == ScreenMode.Cinematic;

            Time.timeScale = frozen ? 0f : 1f;
            state.SetSessionRunning(playing);
            rig.SetControlsEnabled(playing, frozen);
            rig.SetGameplayView(gameplayView);

            Cursor.lockState = playing && !DiagnosticMode
                ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !playing || DiagnosticMode;

            ModeChanged?.Invoke(mode);

            if (inventoryPanel != null)
            {
                if (bagOpen) inventoryPanel.transform.SetAsLastSibling();
                inventoryPanel.SetActive(bagOpen);
            }
        }

        private void OnApplicationFocus(bool focused)
        {
#if !UNITY_EDITOR
            if (!focused && (Mode == ScreenMode.Playing || Mode == ScreenMode.Cinematic || Mode == ScreenMode.Transition) && !DiagnosticMode) Pause();
#endif
        }

        private void OnApplicationQuit()
        {
            SaveCurrentProgress();
        }

        private void OnDestroy()
        {
            if (subscribedDisaster != null) subscribedDisaster.Changed -= SaveCurrentProgress;
            if (subscribedRecovery != null) subscribedRecovery.Changed -= SaveCurrentProgress;
            if (subscribedRestored != null) subscribedRestored.Changed -= SaveCurrentProgress;
            if (missions != null) missions.Completed -= ShowVictory;
            Time.timeScale = 1f;
        }
    }
}
