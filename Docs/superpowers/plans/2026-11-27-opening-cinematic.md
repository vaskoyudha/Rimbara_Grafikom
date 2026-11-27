# Opening Cinematic & Premise Dialogue Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement Rimbara's 4-shot Indonesian opening cinematic (`OpeningDirector` + `DialogueCopy` + `GameFlow` + `OpeningCinematicBuilder` scene installer) so clicking **MASUK KE ARUNIKA** plays the arrival premise montage and hands control to the player at the arrival road.

**Architecture:** Mirror `EndingDirector`'s `CinemachineBrain` handoff, 4-pose camera lerp, fade overlay, TMP subtitle display, and `Esc` → `LEWATI SINEMATIK` skip flow inside a dedicated `Rimbara.UI.OpeningDirector`. Keep `OpeningDirector` UI isolated (`Opening cinematic overlay` and `Skip opening cinematic`) so it never collides with `EndingDirector.LateUpdate()`, and bypass opening playback automatically when `flow.DiagnosticMode` (`--rimbara-smoke`) is active.

**Tech Stack:** Unity 6 (`6000.3.x`), C# (`Rimbara.asmdef`, `Rimbara.Editor.asmdef`, `Rimbara.Tests.asmdef`), Unity Input System / Cinemachine 3, TextMeshPro, NUnit EditMode tests.

## Global Constraints

- Player-facing language MUST be **Bahasa Indonesia (`id-ID`)** per `Docs/Bahasa-Game.md`; no English/placeholder leaks (`TODO`, `TBD`, `Lorem`).
- Every string in `DialogueCopy` must be non-empty and end with `.`, `!`, or `?` (`DialogueCopyTests`).
- `IndonesianText.Validate()` must return empty (`IndonesianTextTests`), and `"Skip opening cinematic label"` must map to `"LEWATI SINEMATIK"`.
- Technical hierarchy names, object IDs, and existing `FromAshes.unity` disaster/recovery/restored gameplay remain unchanged; do not regenerate `Main` or `FromAshes`.
- `OpeningDirector.TryBegin()` must return `false` when `flow.DiagnosticMode` is `true` so existing `--rimbara-smoke` checks stay green.

---

### File Structure

| File | Responsibility |
| :--- | :--- |
| `Assets/Scripts/UI/DialogueCopy.cs` | Stores `NarratorSpeaker`, `VillagerSpeaker`, `OpeningSpeakers`, and `OpeningLines` (included in `AllPages`). |
| `Assets/Scripts/UI/IndonesianText.cs` | Adds `"Skip opening cinematic label": "LEWATI SINEMATIK"` to `Labels`. |
| `Assets/Scripts/UI/OpeningDirector.cs` | Drives the 30s 4-shot opening camera pan, rich-text Indonesian speaker + dialogue subtitles, fade overlay, `Esc` pause, and `LEWATI SINEMATIK` skip. |
| `Assets/Scripts/Mission/GameFlow.cs` | References `OpeningDirector`, starts opening cinematic on `Begin()` if `opening.TryBegin()`, exposes `BeginOpeningCinematic()` and `EndOpeningCinematic()`. |
| `Assets/Editor/OpeningCinematicBuilder.cs` | Menu item `RIMBARA/Storyline/Install opening cinematic` + `VerifyOpeningCinematic()` authoring poses, overlay, and wiring into `Assets/Scenes/FromAshes.unity`. |
| `Assets/Tests/EditMode/DialogueCopyTests.cs` | Unit tests verifying opening speakers, lines, timing/shot helper, and `IndonesianText` label coverage. |

---

### Task 1: Indonesian Opening Premise & Dialogue Copy (`DialogueCopy` & `IndonesianText`)

**Files:**
- Modify: `Assets/Scripts/UI/DialogueCopy.cs`
- Modify: `Assets/Scripts/UI/IndonesianText.cs`
- Test: `Assets/Tests/EditMode/DialogueCopyTests.cs`

**Interfaces:**
- Produces in `Rimbara.UI.DialogueCopy`:
  - `public const string NarratorSpeaker = "NARATOR";`
  - `public const string VillagerSpeaker = "WARGA DESA";`
  - `public static readonly string[] OpeningSpeakers` (4 items)
  - `public static readonly string[] OpeningLines` (4 items)
  - `public static string FormatOpeningSubtitle(int shotIndex)`
- Produces in `Rimbara.UI.IndonesianText`:
  - `Labels["Skip opening cinematic label"] == "LEWATI SINEMATIK"`

- [ ] **Step 1: Write the failing EditMode tests in `Assets/Tests/EditMode/DialogueCopyTests.cs`**

```csharp
        [Test]
        public void OpeningDialogueHasFourIndonesianShotsWithSpeakers()
        {
            Assert.That(DialogueCopy.NarratorSpeaker, Is.EqualTo("NARATOR"));
            Assert.That(DialogueCopy.VillagerSpeaker, Is.EqualTo("WARGA DESA"));
            Assert.That(DialogueCopy.OpeningSpeakers.Length, Is.EqualTo(4));
            Assert.That(DialogueCopy.OpeningLines.Length, Is.EqualTo(4));
            for (int i = 0; i < 4; i++)
            {
                string formatted = DialogueCopy.FormatOpeningSubtitle(i);
                Assert.That(formatted, Does.Contain(DialogueCopy.OpeningSpeakers[i]));
                Assert.That(formatted, Does.Contain(DialogueCopy.OpeningLines[i]));
            }
            Assert.That(IndonesianText.Labels["Skip opening cinematic label"], Is.EqualTo("LEWATI SINEMATIK"));
        }
```

- [ ] **Step 2: Run EditMode tests to verify failure**

Run (with Unity closed):
`~/Unity/Hub/Editor/6000.3.27f2/Editor/Unity -batchmode -nographics -projectPath /home/vyns/UnityProjects/Rimbara_Grafikom -runTests -testPlatform EditMode -testResults /home/vyns/UnityProjects/Rimbara_Grafikom/Artifacts/editmode-tests.xml -logFile /home/vyns/UnityProjects/Rimbara_Grafikom/Artifacts/editmode-tests.log`
Expected: FAIL compilation (`DialogueCopy` does not contain `NarratorSpeaker`, `OpeningSpeakers`, `OpeningLines`, `FormatOpeningSubtitle`).

- [ ] **Step 3: Implement opening speakers, lines, and skip label**

Add to `Assets/Scripts/UI/DialogueCopy.cs`:

```csharp
        public const string NarratorSpeaker = "NARATOR";
        public const string VillagerSpeaker = "WARGA DESA";

        public static readonly string[] OpeningSpeakers =
        {
            NarratorSpeaker,
            VillagerSpeaker,
            CoordinatorSpeaker,
            NarratorSpeaker
        };

        public static readonly string[] OpeningLines =
        {
            "Kebakaran hebat telah melanda Hutan Arunika. Yang tersisa kini hanya abu, batang hangus, dan titik api yang masih menolak padam.",
            "Asapnya belum benar-benar hilang di ujung hutan... Banyak warga terpaksa meninggalkan rumah, berharap ada yang datang membantu.",
            "Relawan baru sudah tiba? Bagus. Segera datangi posko darurat di depan sana—kita harus memadamkan sisa api sebelum menyebar lagi.",
            "Kamu datang bukan untuk berperang, melainkan untuk memulihkan. Setiap langkah kecil berarti. Setiap pohon menyimpan cerita."
        };

        public static string FormatOpeningSubtitle(int shotIndex)
        {
            int index = shotIndex < 0 ? 0 : (shotIndex >= OpeningLines.Length ? OpeningLines.Length - 1 : shotIndex);
            return "<color=#C99A3F><b>" + OpeningSpeakers[index] + "</b></color>\n" + OpeningLines[index];
        }
```

And inside `DialogueCopy.AllPages(int wasteGoal)`:

```csharp
            foreach (string line in OpeningLines) yield return line;
```

And in `Assets/Scripts/UI/IndonesianText.cs` inside `Labels`:

```csharp
            { "Skip cinematic label", "LEWATI SINEMATIK" },
            { "Skip opening cinematic label", "LEWATI SINEMATIK" }
```

- [ ] **Step 4: Run EditMode tests to verify they pass**

Run EditMode tests again.
Expected: PASS (all `DialogueCopyTests` and `IndonesianTextTests` pass).

---

### Task 2: `OpeningDirector` Component & Pure Shot-Timing Logic

**Files:**
- Create: `Assets/Scripts/UI/OpeningDirector.cs`
- Test: `Assets/Tests/EditMode/DialogueCopyTests.cs` (add timing helper tests)

**Interfaces:**
- Consumes:
  - `DialogueCopy.FormatOpeningSubtitle(int shotIndex)`
  - `DisasterChapter.Progress.Stage` (`DisasterStage.Briefing`)
  - `GameFlow.BeginOpeningCinematic()`, `GameFlow.EndOpeningCinematic()`, `GameFlow.Mode`, `GameFlow.DiagnosticMode`
- Produces in `Rimbara.UI.OpeningDirector`:
  - `public const float TotalSeconds = 30f;`
  - `public static int ShotForTime(float seconds, out float normalizedShotProgress)`
  - `public bool IsOpening { get; private set; }`
  - `public bool HasPlayed { get; private set; }`
  - `public float SequenceSeconds { get; private set; }`
  - `public Button SkipButton => skip;`
  - `public void Configure(DisasterChapter owner, GameFlow gameFlow, Camera view, GameObject panel, Image tint, TMP_Text text, Button skipButton, Transform[] poses)`
  - `public bool TryBegin()`
  - `public void SkipOpening()`

- [ ] **Step 1: Write failing EditMode tests for `OpeningDirector.ShotForTime`**

Add to `Assets/Tests/EditMode/DialogueCopyTests.cs`:

```csharp
        [Test]
        public void OpeningShotTimingMapsAllFourBeats()
        {
            Assert.That(OpeningDirector.ShotForTime(0f, out float p0), Is.EqualTo(0));
            Assert.That(p0, Is.EqualTo(0f).Within(0.001f));
            Assert.That(OpeningDirector.ShotForTime(8.0f, out float p1), Is.EqualTo(1));
            Assert.That(p1, Is.EqualTo(0f).Within(0.001f));
            Assert.That(OpeningDirector.ShotForTime(15.0f, out float p2), Is.EqualTo(2));
            Assert.That(p2, Is.EqualTo(0f).Within(0.001f));
            Assert.That(OpeningDirector.ShotForTime(22.0f, out float p3), Is.EqualTo(3));
            Assert.That(p3, Is.EqualTo(0f).Within(0.001f));
            Assert.That(OpeningDirector.ShotForTime(30.0f, out float pEnd), Is.EqualTo(3));
            Assert.That(pEnd, Is.EqualTo(1f).Within(0.001f));
        }
```

- [ ] **Step 2: Run EditMode tests to verify failure**

Expected: FAIL compilation (`The name 'OpeningDirector' does not exist in the current context`).

- [ ] **Step 3: Create `Assets/Scripts/UI/OpeningDirector.cs`**

```csharp
using System.Collections;
using Rimbara.Mission;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace Rimbara.UI
{
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
```

- [ ] **Step 4: Run EditMode tests to verify `OpeningShotTimingMapsAllFourBeats` passes**

Expected: PASS.

---

### Task 3: Integrate `OpeningDirector` with `GameFlow`

**Files:**
- Modify: `Assets/Scripts/Mission/GameFlow.cs`

**Interfaces:**
- Consumes: `Rimbara.UI.OpeningDirector` (`TryBegin()`)
- Produces in `Rimbara.Mission.GameFlow`:
  - `public OpeningDirector Opening => opening;`
  - `public void ConfigureOpening(OpeningDirector openingDirector)`
  - `public void BeginOpeningCinematic()`
  - `public void EndOpeningCinematic()`

- [ ] **Step 1: Update `Assets/Scripts/Mission/GameFlow.cs`**

Add `using Rimbara.UI;` at top, add serialized field and property:

```csharp
        [SerializeField] private OpeningDirector opening;
        public OpeningDirector Opening => opening;
        public void ConfigureOpening(OpeningDirector openingDirector) => opening = openingDirector;
```

Update `Begin()` and add `BeginOpeningCinematic()` / `EndOpeningCinematic()`:

```csharp
        public void Begin()
        {
            if (Mode != ScreenMode.Briefing && Mode != ScreenMode.Menu) return;
            if (opening != null && opening.TryBegin()) return;
            SetMode(ScreenMode.Playing);
        }

        public void BeginOpeningCinematic() => SetMode(ScreenMode.Cinematic);

        public void EndOpeningCinematic()
        {
            if (Mode == ScreenMode.Cinematic || Mode == ScreenMode.Paused)
                SetMode(ScreenMode.Playing);
        }
```

- [ ] **Step 2: Run EditMode tests to verify clean compile & all tests pass**

Expected: PASS with zero compiler errors.

---

### Task 4: Editor Scene Installer & Verification (`OpeningCinematicBuilder.cs` + `FromAshes.unity`)

**Files:**
- Create: `Assets/Editor/OpeningCinematicBuilder.cs`
- Modify (via menu execution): `Assets/Scenes/FromAshes.unity`

**Interfaces:**
- Consumes: `OpeningDirector`, `GameFlow`, `DisasterChapter`, `RimbaraHud`, `VolunteerRig`, `IndonesianText`
- Produces:
  - `[MenuItem("RIMBARA/Storyline/Install opening cinematic")]` `OpeningCinematicBuilder.Install()`
  - `OpeningCinematicBuilder.VerifyOpeningCinematic()`

- [ ] **Step 1: Create `Assets/Editor/OpeningCinematicBuilder.cs`**

Authors idempotently in `Assets/Scenes/FromAshes.unity`:
- Backs up `FromAshes.unity` to `Backups/`.
- Creates or updates `Opening Cinematic` root object with `OpeningDirector` and `01 - Opening camera poses` (`Opening shot 1` .. `Opening shot 4` framed over the burned forest, `Evacuated villager at relief post`, `Coordinator`, and behind `VolunteerRig` spawn).
- Creates `Opening cinematic overlay` (`Opening subtitle` TMP_Text styled consistently with `Ending subtitle`) and `Skip opening cinematic` button cloned from `hud.View.Resume`, positioned at `(0.5f, 0.1f)`, caption named `"Skip opening cinematic label"` = `"LEWATI SINEMATIK"`.
- Wires `OpeningDirector.Configure(...)` and `GameFlow.ConfigureOpening(director)`, runs `IndonesianText.ApplyStaticLabels(...)`, marks scene dirty, saves `FromAshes.unity`, and calls `VerifyOpeningCinematic()`.

- [ ] **Step 2: Run `OpeningCinematicBuilder.Install()` and `VerifyOpeningCinematic()` in Unity**

Verify via Unity batchmode (`-executeMethod Rimbara.Editor.OpeningCinematicBuilder.Install`) or Unity MCP `execute_menu_item`, then re-run EditMode tests to confirm all tests pass after scene modification.

---

### Self-Review Checklist
1. **Spec coverage:** Opening speakers & lines (`Task 1`), skip label (`Task 1`), 30s 4-shot `OpeningDirector` with pause/skip/fade/handoff (`Task 2`), `GameFlow` integration & `--rimbara-smoke` bypass (`Task 3`), scene authoring & verification (`Task 4`). All spec items covered.
2. **Placeholders:** None — full code blocks provided for Tasks 1–3 and exact authoring behavior for Task 4.
3. **Type consistency:** `OpeningDirector.ShotForTime`, `TryBegin`, `BeginOpeningCinematic`, `EndOpeningCinematic`, `FormatOpeningSubtitle`, and `"Skip opening cinematic label"` match across tasks.
