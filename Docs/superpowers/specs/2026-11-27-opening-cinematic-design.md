# Design Spec: Opening Cinematic & Premise Dialogue (`Prolog / Kedatangan`)

**Project:** RIMBARA: Dari Abu Menuju Kehidupan (`Rimbara_Grafikom`)  
**Scene:** `Assets/Scenes/FromAshes.unity`  
**Language:** Bahasa Indonesia (`id-ID`) per [Docs/Bahasa-Game.md](../../Bahasa-Game.md)  
**Date:** 2026-11-27

---

## 1. Purpose & Scope

Implement the unimplemented **Prologue / Arrival opening cinematic** (`Prolog / Kedatangan`) immediately after the player clicks **MASUK KE ARUNIKA** on the Briefing screen (`GameFlow.Begin()`), establishing the wildfire premise, showing the evacuated villager and emergency post, and handing control cleanly to the player at the arrival road (`DisasterStage.Briefing`).

### In Scope
- Indonesian opening premise narration and character dialogue stored in `Rimbara.UI.DialogueCopy`.
- `Rimbara.UI.OpeningDirector` component mirroring `EndingDirector`'s camera-pose lerp, `CinemachineBrain` handoff, fade overlay, TMP subtitle display, `Esc` pause, and `LEWATI SINEMATIK` skip behavior.
- `GameFlow` integration (`BeginOpeningCinematic` / `EndOpeningCinematic`, plus `--rimbara-smoke` bypass so existing Disaster/Recovery/Restored smoke tests remain deterministic unless explicitly testing the opening).
- Editor scene wiring/installer script (`Assets/Editor/OpeningCinematicBuilder.cs` with menu item `RIMBARA/Storyline/Install opening cinematic` and verifier) that authors the 4 opening camera poses, overlay, and skip button into `Assets/Scenes/FromAshes.unity` without regenerating the map.
- EditMode unit tests in `Assets/Tests/EditMode/DialogueCopyTests.cs` (and opening state/copy checks).

### Out of Scope
- Voice-over audio files, lip-sync, or full NPC escort pathfinding (`villager escort` remains separate future work).
- Modifying `EndingDirector` timing or `CoordinatorBriefing` / `RangerBriefing` / `ElderBriefing` copy.

---

## 2. Opening Premise & Dialogue Script (Bahasa Indonesia)

Total duration: **30.0 seconds** across **4 camera beats**. Each beat uses rich-text speaker header formatting (`<color=#C99A3F><b>SPEAKER</b></color>\n<line>`), consistent with `DialogueCopy` / `DisasterHud` (`#C99A3F` gold accent), and ends with standard punctuation so `DialogueCopyTests` checks pass.

| Shot | Time Window | Camera Framing | Speaker | Indonesian Subtitle / Dialogue |
| :--- | :--- | :--- | :--- | :--- |
| **1. Hutan yang Terluka** | `0.0s – 8.0s` | Wide elevated pan over the burned Arunika forest (`HUTAN TERBAKAR`) and low drifting wildfire smoke | `NARATOR` | `Kebakaran hebat telah melanda Hutan Arunika. Yang tersisa kini hanya abu, batang hangus, dan titik api yang masih menolak padam.` |
| **2. Suara Warga** | `8.0s – 15.0s` | Medium framing toward `Evacuated villager at relief post` (`ResidentWoman`) near the relief area | `WARGA DESA` | `Asapnya belum benar-benar hilang di ujung hutan... Banyak warga terpaksa meninggalkan rumah, berharap ada yang datang membantu.` |
| **3. Panggilan dari Posko** | `15.0s – 22.0s` | Dolly toward `POSKO DARURAT` and `KOORDINATOR MISI` (`Coordinator`) | `KOORDINATOR MISI` | `Relawan baru sudah tiba? Bagus. Segera datangi posko darurat di depan sana—kita harus memadamkan sisa api sebelum menyebar lagi.` |
| **4. Langkah Pertama** | `22.0s – 30.0s` | Smooth descend behind the player volunteer (`VolunteerRig`) on the arrival road facing `POSKO DARURAT`, fading into gameplay | `NARATOR` | `Kamu datang bukan untuk berperang, melainkan untuk memulihkan. Setiap langkah kecil berarti. Setiap pohon menyimpan cerita.` |

### `DialogueCopy` Additions
Add to `Assets/Scripts/UI/DialogueCopy.cs`:
- `public const string NarratorSpeaker = "NARATOR";`
- `public const string VillagerSpeaker = "WARGA DESA";`
- `public static readonly string[] OpeningSpeakers = { NarratorSpeaker, VillagerSpeaker, CoordinatorSpeaker, NarratorSpeaker };`
- `public static readonly string[] OpeningLines = { ... };` (the 4 Indonesian strings above)
- Include `OpeningLines` inside `DialogueCopy.AllPages(...)` so `DialogueCopyTests.EveryPageIsNonEmptyAndPunctuated` validates every opening line.

---

## 3. Architecture & Data Flow

### Components & Responsibilities

1. **`OpeningDirector` (`Assets/Scripts/UI/OpeningDirector.cs`)**
   - Serialized fields:
     - `DisasterChapter chapter`
     - `GameFlow flow`
     - `Camera cameraView`
     - `GameObject overlay` (dedicated `Opening cinematic overlay` panel with `Image fade` and `TMP_Text subtitle`)
     - `Image fade`
     - `TMP_Text subtitle`
     - `Button skip` (dedicated `Skip opening cinematic` button labeled `LEWATI SINEMATIK`, visible only when `flow.Mode == ScreenMode.Paused && IsOpening`)
     - `Transform[] cameraPoses` (length `4`: `Opening shot 1` .. `Opening shot 4`)
   - State:
     - `public bool IsOpening { get; private set; }`
     - `public bool HasPlayed { get; private set; }`
     - `public float SequenceSeconds { get; private set; }`
     - `public Button SkipButton => skip;`
   - Lifecycle:
     - `Awake()`: validates required references (`chapter`, `flow`, `cameraView`, `overlay`, `fade`, `subtitle`, `skip`, `cameraPoses.Length == 4`), caches `CinemachineBrain`, hides `overlay` and `skip`, binds `skip.onClick.AddListener(SkipOpening)`.
     - `TryBegin()`: returns `false` if already running, `HasPlayed` is `true`, `chapter.Progress.Stage != DisasterStage.Briefing`, or `flow.DiagnosticMode`. Otherwise sets `IsOpening = true`, disables `CinemachineBrain` (`ownsCamera = true`), calls `flow.BeginOpeningCinematic()`, and starts the `Opening()` coroutine.
     - `Opening()` coroutine (`30f` total, split across shot durations `[8f, 7f, 7f, 8f]`):
       - Advances `SequenceSeconds += Time.unscaledDeltaTime` only while `flow.Mode != ScreenMode.Paused`.
       - Computes active shot index `0..3` and normalized `SmoothStep` `t` within that shot's window, lerping `cameraView.transform` between `cameraPoses[shot]` and `cameraPoses[Mathf.Min(3, shot + 1)]`.
       - Updates `subtitle.text` to `<color=#C99A3F><b>{speaker}</b></color>\n{line}` using `DialogueCopy.OpeningSpeakers[shot]` and `DialogueCopy.OpeningLines[shot]`.
       - Controls `fade.color`: brief fade-in from dark (`0f–1.2f`), subtle dark vignette (`0.12f` alpha) during shots for subtitle contrast, and gentle fade-to-clear (`28.5f–30f`) before releasing camera.
       - Calls `Finish()`.
     - `LateUpdate()`: mirrors `EndingDirector` — keeps `overlay.SetActive((flow.Mode == ScreenMode.Cinematic || flow.Mode == ScreenMode.Paused) && sequence != null && IsOpening)`? Note: during `ScreenMode.Paused`, `RimbaraHud` displays `view.Pause`, so keeping `overlay.SetActive(flow.Mode == ScreenMode.Cinematic && sequence != null && IsOpening)` ensures the pause menu stays cleanly on top, while `skip.gameObject.SetActive(flow.Mode == ScreenMode.Paused && IsOpening)` shows the skip button during pause!
     - `SkipOpening()`: stops coroutine if `IsOpening && (flow.Mode == ScreenMode.Paused || flow.Mode == ScreenMode.Cinematic)` and calls `Finish()`.
     - `Finish()`: sets `IsOpening = false`, `HasPlayed = true`, `sequence = null`, hides `overlay` and `skip`, restores `CinemachineBrain.enabled`, and calls `flow.EndOpeningCinematic()`.
     - `OnDestroy()`: stops coroutine, releases camera, unsubscribes `skip.onClick`.

2. **`GameFlow` (`Assets/Scripts/Mission/GameFlow.cs`)**
   - Add `[SerializeField] private OpeningDirector opening;` and `public OpeningDirector Opening => opening;` plus `public void ConfigureOpening(OpeningDirector openingDirector) => opening = openingDirector;`.
   - In `Begin()`:
     - Keep guard `if (Mode != ScreenMode.Briefing && Mode != ScreenMode.Menu) return;`.
     - If `opening != null && opening.TryBegin()`, let `OpeningDirector` enter `ScreenMode.Cinematic` and return (do not immediately jump to `ScreenMode.Playing`).
     - Otherwise `SetMode(ScreenMode.Playing)` as before.
   - Add:
     - `public void BeginOpeningCinematic() => SetMode(ScreenMode.Cinematic);`
     - `public void EndOpeningCinematic() { if (Mode == ScreenMode.Cinematic || Mode == ScreenMode.Paused) SetMode(ScreenMode.Playing); }`
   - Keep `EndStorySequence(bool ending)` untouched for `EndingDirector`.

3. **Dedicated UI Isolation (`Opening` vs `Ending`)**
   - `OpeningDirector` uses its own `Opening cinematic overlay` and `Skip opening cinematic` button under the HUD Canvas (with `CanvasGroup` / `raycastTarget = true` during cinematic, hidden when inactive) instead of sharing `EndingDirector`'s `overlay` and `skip` instances, preventing `LateUpdate()` conflicts between the two directors.
   - Both directors still satisfy `IndonesianText` because the skip button caption uses `"Skip opening cinematic label"` mapped in `IndonesianText.Labels` (or keeps `"LEWATI SINEMATIK"`). We will add `"Skip opening cinematic label": "LEWATI SINEMATIK"` to `IndonesianText.Labels` so `IndonesianText.ApplyStaticLabels` and `IndonesianText.Validate()` cover it.

4. **Scene Authoring (`Assets/Editor/OpeningCinematicBuilder.cs`)**
   - Adds a non-destructive editor installer `[MenuItem("RIMBARA/Storyline/Install opening cinematic")]` and `VerifyOpeningCinematic()` (can also be executed cleanly via Unity batch/menu or MCP `execute_menu_item`).
   - Saves a scene backup in `Backups/` before modifying `Assets/Scenes/FromAshes.unity`.
   - Creates `Opening Cinematic` hierarchy under `Storyline` (or scene root) with 4 `Transform` camera poses dynamically positioned relative to the actual authored scene targets (`Hutan Terbakar` / wildfire area, `Evacuated villager at relief post`, `Coordinator`, and `VolunteerRig` spawn) so framing is grounded in actual scene coordinates.
   - Creates `Opening cinematic overlay` (`Opening subtitle` TMP_Text) and `Skip opening cinematic` (`Button` cloned from `hud.View.Resume` just like `RestoredMapBuilder.cs`), assigns references to `OpeningDirector` and `GameFlow`, runs `IndonesianText.ApplyStaticLabels`, and saves `FromAshes.unity`.

---

## 4. Error Handling & Edge Cases

- **Missing scene references:** `OpeningDirector.Awake()` throws `InvalidOperationException` on missing serialized fields or `cameraPoses.Length != 4`, matching `EndingDirector.Awake()`.
- **Pause (`Esc`) mid-opening:** `GameFlow.Update()` already pauses `ScreenMode.Cinematic` to `ScreenMode.Paused` (`Time.timeScale = 0f`), `Opening()` stops advancing `SequenceSeconds`, and `LateUpdate()` reveals `Skip opening cinematic`. Pressing `Esc` or `LANJUTKAN MISI` resumes `ScreenMode.Cinematic` cleanly without enabling player movement.
- **Skip (`LEWATI SINEMATIK`):** `SkipOpening()` immediately stops the coroutine, restores `CinemachineBrain`, hides overlay/skip, sets `HasPlayed = true`, and transitions `GameFlow` to `ScreenMode.Playing`.
- **Replay / Return to Menu:** Reloading the scene resets `HasPlayed = false`, so replaying from Briefing plays the opening again unless bypassed.
- **Automated Smoke Driver (`--rimbara-smoke`):** `flow.DiagnosticMode` is `true` under `--rimbara-smoke`, so `OpeningDirector.TryBegin()` returns `false` and `GameFlow.Begin()` enters `ScreenMode.Playing` immediately — preserving all existing smoke-test assertions.

---

## 5. Verification Plan

1. **EditMode Unit Tests (`Assets/Tests/EditMode/DialogueCopyTests.cs` & new opening tests):**
   - Verify `OpeningLines` and `OpeningSpeakers` have length `4`, are non-empty, Indonesian, and punctuated.
   - Verify `IndonesianText.Validate()` passes with `"Skip opening cinematic label"` included.
   - Verify `OpeningDirector` shot index/timing helper logic (pure helper method) maps `0..30s` to shots `0..3` monotonically.
2. **Scene Wiring Verification (`OpeningCinematicBuilder.VerifyOpeningCinematic()`):**
   - Confirm `OpeningDirector` exists in `FromAshes.unity`, all serialized references are non-null, `cameraPoses.Length == 4`, and `GameFlow.Opening` is assigned.
3. **Runtime / Unity Play Check:**
   - Run EditMode tests (`Tools/Test-EditMode.ps1` or Unity Test Runner via MCP/CLI) and project verification (`RIMBARA > Verify authored project` / storyline verify) after implementation.
