using System.Collections;
using Rimbara.Mission;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace Rimbara.UI
{
    /// <summary>
    /// Drives the 4-shot Indonesian prologue opening cinematic and hands control to gameplay.
    /// </summary>
    public sealed class OpeningDirector : MonoBehaviour
    {
        public const float TotalSeconds = 30f;
        private static readonly float[] ShotDurations = { 8f, 7f, 7f, 8f };

        [SerializeField] private DisasterChapter chapter;
        [SerializeField] private GameFlow flow;
        [SerializeField] private Camera cameraView;
        [SerializeField] private GameObject overlay;
        [SerializeField] private Image fade;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField] private Button skip;
        [SerializeField] private Transform[] cameraPoses;

        private CinemachineBrain brain;
        private bool brainWasEnabled;
        private bool ownsCamera;
        private Coroutine sequence;

        public bool IsOpening { get; private set; }
        public bool HasPlayed { get; private set; }
        public float SequenceSeconds { get; private set; }
        public Button SkipButton => skip;

        public void Configure(DisasterChapter owner, GameFlow gameFlow, Camera view,
            GameObject panel, Image tint, TMP_Text text, Button skipButton, Transform[] poses)
        {
            chapter = owner;
            flow = gameFlow;
            cameraView = view;
            overlay = panel;
            fade = tint;
            subtitle = text;
            skip = skipButton;
            cameraPoses = poses;
        }

        public static int ShotForTime(float seconds, out float normalizedProgress)
        {
            float clamped = Mathf.Clamp(seconds, 0f, TotalSeconds);
            float start = 0f;
            for (int i = 0; i < ShotDurations.Length; i++)
            {
                float duration = ShotDurations[i];
                if (clamped < start + duration || i == ShotDurations.Length - 1)
                {
                    normalizedProgress = Mathf.Clamp01((clamped - start) / duration);
                    return i;
                }
                start += duration;
            }
            normalizedProgress = 1f;
            return ShotDurations.Length - 1;
        }

        private void Awake()
        {
            if (chapter == null || flow == null || cameraView == null || overlay == null
                || fade == null || subtitle == null || skip == null || cameraPoses == null || cameraPoses.Length != 4)
                throw new System.InvalidOperationException("Opening cinematic requires its complete authored UI and four camera poses.");
            brain = cameraView.GetComponent<CinemachineBrain>();
            overlay.SetActive(false);
            skip.gameObject.SetActive(false);
            skip.onClick.AddListener(SkipOpening);
        }

        public bool TryBegin()
        {
            if (sequence != null || IsOpening || HasPlayed || flow.DiagnosticMode) return false;
            if (chapter.Progress == null || chapter.Progress.Stage != DisasterStage.Briefing) return false;
            IsOpening = true;
            brainWasEnabled = brain != null && brain.enabled;
            if (brain != null) brain.enabled = false;
            ownsCamera = true;
            flow.BeginOpeningCinematic();
            sequence = StartCoroutine(PlayOpening());
            return true;
        }

        private IEnumerator PlayOpening()
        {
            SequenceSeconds = 0f;
            while (SequenceSeconds < TotalSeconds)
            {
                if (flow.Mode != ScreenMode.Paused)
                {
                    SequenceSeconds += Time.unscaledDeltaTime;
                    int shot = ShotForTime(SequenceSeconds, out float rawT);
                    float t = Mathf.SmoothStep(0f, 1f, rawT);
                    int next = Mathf.Min(3, shot + 1);
                    cameraView.transform.SetPositionAndRotation(
                        Vector3.Lerp(cameraPoses[shot].position, cameraPoses[next].position, t),
                        Quaternion.Slerp(cameraPoses[shot].rotation, cameraPoses[next].rotation, t));
                    subtitle.text = DialogueCopy.FormatOpeningSubtitle(shot);
                    float alpha = SequenceSeconds < 1.2f
                        ? Mathf.Lerp(0.85f, 0.14f, SequenceSeconds / 1.2f)
                        : SequenceSeconds > TotalSeconds - 1.5f
                            ? Mathf.Lerp(0.14f, 0f, (SequenceSeconds - (TotalSeconds - 1.5f)) / 1.5f)
                            : 0.14f;
                    fade.color = new Color(0.02f, 0.03f, 0.025f, Mathf.Clamp01(alpha));
                }
                yield return null;
            }
            Finish();
        }

        private void LateUpdate()
        {
            bool showing = flow.Mode == ScreenMode.Cinematic && sequence != null && IsOpening;
            overlay.SetActive(showing);
            skip.gameObject.SetActive(flow.Mode == ScreenMode.Paused && IsOpening);
        }

        public void SkipOpening()
        {
            if (!IsOpening || (flow.Mode != ScreenMode.Paused && flow.Mode != ScreenMode.Cinematic)) return;
            if (sequence != null) StopCoroutine(sequence);
            Finish();
        }

        private void Finish()
        {
            if (!IsOpening) return;
            IsOpening = false;
            HasPlayed = true;
            sequence = null;
            overlay.SetActive(false);
            skip.gameObject.SetActive(false);
            ReleaseCamera();
            flow.EndOpeningCinematic();
        }

        private void ReleaseCamera()
        {
            if (ownsCamera && brain != null) brain.enabled = brainWasEnabled;
            ownsCamera = false;
        }

        private void OnDestroy()
        {
            if (sequence != null) StopCoroutine(sequence);
            ReleaseCamera();
            if (skip != null) skip.onClick.RemoveListener(SkipOpening);
        }
    }
}
