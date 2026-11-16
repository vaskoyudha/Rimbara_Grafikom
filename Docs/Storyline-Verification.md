# Disaster Zone development slice — verification

**Historical Zone 1 baseline:** this report/evidence applies to the published
`096d8e9` milestone, before Recovery was added. For the current two-zone build,
use [Zone2-Implementation.md](Zone2-Implementation.md) and its new Recovery reports.

Workspace: `vaskoyudha/Rimbara_Grafikom`, branch `feature/from-ashes-zone1`.
Team baseline: `3611552b646e002b22bc903f21e38c3682000a09`.
New scene: `Assets/Scenes/FromAshes.unity`.

**Resource rule:** use EmaceArt resources, **not** its template layout.
FromAshes now contains an original RIMBARA Disaster map, with a custom arrival
road, quiet village, relief post, charred forest and cleanup clearings. Previous
demo-layout reports and screenshots have been replaced by original-map evidence.

This is the first two missions of the approved **From Ashes to Life** plan,
not the complete five-mission game. The original Main, Arunika and
VillageRestoration scenes are unchanged. RIMBARA's separate old local project
was not edited.

## Executed checks

Language: Indonesian (id-ID). Menu, controls, dialogue, HUD, destination display
names, interaction feedback and world markers are localized without renaming
technical objects. Evidence and captures are refreshed for this Indonesian build.
Pre-publication rerun: **16 November 2026**, including the installed Unity Version
Control integration and linked Cloud project. Zone 2 remains design-only.

- Unity **6000.3.6f1**, Windows x64, AMD Radeon RX 6600 / Direct3D11, 1600 × 900.
- **56/56 EditMode tests passed:** 36 original model tests, 11 Disaster chapter
  cases, and 9 Indonesian text cases. Progression, pause, duplicate IDs, invalid
  supply/IDs, reset, UI language, marker translations and controls are covered.
- Storyline scene verification passed: two fires, four distinct debris piles,
  Interaction-layer triggers, wired chapter/UI/coordinator/equipment, no missing
  scripts. The chapter's serialized references were built from team Main.
- **50/50 standalone runtime checks passed**, with zero error/exception/assert
   logs. Tests cover pointer menu/briefing clicks, E dialogue, BERIKUTNYA/LANJUTKAN,
  paused mutation rejection, Escape dismissal, equipment/duplicate rejection,
  both camera-aimed water suppression targets, cleanup gating/range/pause,
  E cleanup of all four distinct piles, chapter completion, replay reset, and the
   presence of the original Disaster map and absence of the copied demo layout.
  Physical motor routes also cover spawn → emergency post and post → first fire,
  without teleporting or disabling obstacles along those routes.
  Two additional checks verify that the post inscription is parented to the board,
  has no Billboard component, keeps its orientation and does not overflow. A
  diagnostic-only close-up confirms the text is visible on the parchment header.
- Scene language audit passed: **53 UI labels, 22 world labels, 0 untranslated
  entries**. Runtime additionally checks Indonesian menu, controls, coordinator
  dialogue, HUD and interaction feedback. Controls, briefing and pause captures
  are preserved with the menu/ending captures.
- The latest Windows x64 build succeeded with **0 errors and 0 reported build
  warnings**. The unchanged vendor source can still emit CS0414 about Farm UI
  `CustomButton.isHeld` during a fresh script compilation.
- Git LFS fsck passes locally. Asset metadata audit found no missing metadata,
  malformed GUIDs, duplicate GUIDs or duplicate scene file IDs.

Reports: [Storyline evidence](Evidence/Storyline/).
Representative captures: [dialogue](Captures/Storyline/coordinator.png),
[post board](Captures/Storyline/post-board.png),
[cleared chapter](Captures/Storyline/clean-start.png), and
[chapter completion](Captures/Storyline/chapter-complete.png).

## Boundaries and outstanding work

The runtime driver physically traverses the arrival/first-fire corridors and
teleports between the remaining isolated target fixtures, then approaches each
with the real motor; E/mouse input and UI pointer events are
real Input System events. No scenery collisions are disabled to pass tests.
This does **not** verify an uninterrupted route through every village obstacle,
every resolution, human session duration or performance on other machines.

The first fixture run found an approach blocked by Default-layer scenery;
approach checks now include those colliders, with valid approach directions.
The clipped dialogue button was corrected and reviewed in captures.
Follow-up runs exposed intermittent one-shot pointer events in the diagnostic
driver. Pointer state is now held across multiple UI update frames; no direct
button callback bypass is used. Use only the final passing report as evidence,
not an earlier failed or superseded diagnostic run.

Only designated debris is interactable; the remaining burnt scenery is not
collectible. Equipment chest, designated timber and fire-damaged trees use
EmaceArt prefabs. The stationary Ch42 coordinator is still a character placeholder.
The story's escort NPC, cinematic opening, Recovery watering/habitat mechanics,
Restored final tree/viewpoint, achievements and final cinematic remain planned.

## Reproduction

Open `Assets/Scenes/FromAshes.unity` and press Play, or follow the commands in
[Storyline-Plan.md](Storyline-Plan.md). The standalone executable is local at
`Builds/Storyline-Windows/RIMBARA-FromAshes.exe`; keep its data/support files beside it.
Builds and raw logs are ignored. Evidence here is new storyline evidence; old
MVP verification counts elsewhere do not validate the new chapter.

Remote Git LFS initially lacked 126 baseline paths. Exact-hash local recovery
made this workspace complete. A later authorized upload and push of `096d8e9`
completed the LFS storage; a fresh GitHub clone and `git lfs fsck` passed.

## Original-map authoring findings

- Individual kit prefabs/models/materials are placed in our own level. No demo
  scene/hierarchy/terrain is opened or copied by DisasterMapBuilder.
- The kit uses a colour atlas, not a tileable terrain texture. Soil derives from
  its earth swatch; blackened/ash variants are generated in our derived assets.
- The new terrain reserves flat arrival/forest corridors and open fire/cleanup
  approaches. Boundary hills and ash-covered clearings are original authored data.
- Source vendor prefabs/materials/scenes and team Main/Arunika/VillageRestoration
  remain untouched. Only FromAshes and new derived assets change.
- The misplaced **POSKO DARURAT** label is now a dark inscription on the front
  header of the mission board, parented to it with no Billboard component. The
  targeted `Tools/Invoke-Storyline.ps1 -Command post-sign` repair edits the existing
  scene without regenerating terrain, village placements or mission objects.
  Scene verification checks attachment, front-face position, orientation and fit.
  The updated standalone build and board close-up were tested after this repair.
- Opening cinematic, villager escort and full three-zone narrative remain future
  work, not features inferred from scenery placement or a passing model test.
