using System.Collections;
using Rimbara.Mission;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace Rimbara.UI
{
    public sealed class EndingDirector : MonoBehaviour
    {
        [SerializeField] private RestoredChapter chapter;
        [SerializeField] private RestoredEnvironment environment;
        [SerializeField] private GameFlow flow;
        [SerializeField] private Camera cameraView;
        [SerializeField] private GameObject overlay;
        [SerializeField] private Image fade;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField] private Button skip;
        [SerializeField] private AudioSource music;
        [SerializeField] private Transform[] cameraPoses;
        private CinemachineBrain brain;
        private bool brainWasEnabled;
        private bool ownsCamera;
        private bool musicPaused;
        private Coroutine sequence;
        public float SequenceSeconds { get; private set; }
        public bool IsEnding => chapter.Progress.Stage == RestoredStage.Ending;
        public Button SkipButton => skip;
        public void Configure(RestoredChapter owner, RestoredEnvironment world, GameFlow gameFlow, Camera view,
            GameObject panel, Image tint, TMP_Text text, Button skipButton, AudioSource track, Transform[] poses)
        { chapter = owner; environment = world; flow = gameFlow; cameraView = view; overlay = panel; fade = tint; subtitle = text; skip = skipButton; music = track; cameraPoses = poses; }
        private void Awake()
        {
            if (chapter == null || environment == null || flow == null || cameraView == null || overlay == null || fade == null
                || subtitle == null || skip == null || music == null || music.clip == null || cameraPoses == null || cameraPoses.Length != 4)
                throw new System.InvalidOperationException("Ending requires its complete authored UI, music and four camera poses.");
            brain = cameraView.GetComponent<CinemachineBrain>();
            overlay.SetActive(false); skip.gameObject.SetActive(false);
            skip.onClick.AddListener(SkipEnding);
        }
        public void BeginTransition()
        {
            if (sequence != null || chapter.Progress.Stage != RestoredStage.TimeTransition) return;
            flow.BeginStorySequence(false);
            sequence = StartCoroutine(Transition());
        }
        private IEnumerator Transition()
        {
            SequenceSeconds = 0f;
            subtitle.text = "WAKTU BERLALU\n\nHutan dirawat, musim demi musim.\nKamu kembali menyaksikan harapan yang tumbuh.";
            bool applied = false;
            while (SequenceSeconds < 6f)
            {
                if (flow.Mode != ScreenMode.Paused)
                {
                    SequenceSeconds += Time.unscaledDeltaTime;
                    float alpha = SequenceSeconds < 1f ? SequenceSeconds : SequenceSeconds > 5f ? 6f - SequenceSeconds : 1f;
                    fade.color = new Color(0.025f, 0.045f, 0.03f, Mathf.Clamp01(alpha));
                    if (!applied && SequenceSeconds >= 2.5f) { environment.Apply(); applied = true; }
                }
                yield return null;
            }
            chapter.Progress.CompleteTransition(flow.Mode == ScreenMode.Transition);
            sequence = null; overlay.SetActive(false); flow.EndStorySequence(false);
        }
        public void BeginEnding()
        {
            if (sequence != null || !IsEnding) return;
            flow.BeginStorySequence(true);
            brainWasEnabled = brain != null && brain.enabled;
            if (brain != null) brain.enabled = false;
            ownsCamera = true;
            music.Play(); sequence = StartCoroutine(Ending());
        }
        private IEnumerator Ending()
        {
            SequenceSeconds = 0f;
            string[] narration =
            {
                "Dulu, tempat ini diliputi api.\nHutan terdiam, satwa menjauh, dan desa hidup dalam ketakutan.",
                "Melalui kepedulian dan banyak langkah kecil,\nkehidupan menemukan jalan untuk kembali.",
                "Pepohonan tumbuh. Sungai kembali jernih. Desa menemukan harapan.\nKamu datang sebagai relawan, dan kini menjadi bagian dari cerita hutan ini.",
                "DARI ABU MENUJU KEHIDUPAN\nSetiap Pohon Menyimpan Cerita.\n\nTERIMA KASIH, RELAWAN."
            };
            while (SequenceSeconds < 36f)
            {
                if (flow.Mode != ScreenMode.Paused)
                {
                    SequenceSeconds += Time.unscaledDeltaTime;
                    int shot = Mathf.Min(3, (int)(SequenceSeconds / 9f));
                    float t = Mathf.SmoothStep(0f, 1f, (SequenceSeconds - shot * 9f) / 9f);
                    int next = Mathf.Min(3, shot + 1);
                    cameraView.transform.SetPositionAndRotation(Vector3.Lerp(cameraPoses[shot].position, cameraPoses[next].position, t),
                        Quaternion.Slerp(cameraPoses[shot].rotation, cameraPoses[next].rotation, t));
                    subtitle.text = narration[shot];
                    fade.color = new Color(0.02f, 0.045f, 0.03f, shot == 3 ? Mathf.Clamp01((SequenceSeconds - 31f) / 5f) : 0.08f);
                }
                yield return null;
            }
            Finish(false);
        }
        private void LateUpdate()
        {
            bool cinematic = (flow.Mode == ScreenMode.Cinematic || flow.Mode == ScreenMode.Transition) && sequence != null;
            overlay.SetActive(cinematic);
            skip.gameObject.SetActive(flow.Mode == ScreenMode.Paused && IsEnding);
            if (flow.Mode == ScreenMode.Paused && music.isPlaying) { music.Pause(); musicPaused = true; }
            else if (flow.Mode == ScreenMode.Cinematic && IsEnding && musicPaused) { music.UnPause(); musicPaused = false; }
        }
        public void SkipEnding()
        {
            if (!IsEnding || (flow.Mode != ScreenMode.Paused && flow.Mode != ScreenMode.Cinematic)) return;
            if (sequence != null) StopCoroutine(sequence);
            Finish(true);
        }
        private void Finish(bool skipped)
        {
            if (!chapter.Progress.FinishEnding(IsEnding, skipped)) return;
            sequence = null; music.Stop(); musicPaused = false; overlay.SetActive(false); skip.gameObject.SetActive(false);
            ReleaseCamera(); flow.EndStorySequence(true);
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
            if (skip != null) skip.onClick.RemoveListener(SkipEnding);
        }
    }
}
