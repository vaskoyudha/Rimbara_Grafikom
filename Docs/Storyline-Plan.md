# From Ashes to Life — team development plan

Approved source: `Rimbaraku_Storyline_From_Ashes_to_Life.docx`, provided by the
team on 15 November 2026. RIMBARA remains the technical project name pending
confirmation of the document's RIMBARAKU branding.

Player-facing language is **Indonesian (id-ID)**, including future zones. See
[Bahasa-Game.md](Bahasa-Game.md). Technical object/file names remain unchanged.

## Narrative contract

| Area | Mission | Required actions |
| --- | --- | --- |
| Prologue / Arrival | Meet the villager; reach the emergency post | Introduction, NPC guidance, coordinator briefing |
| 1 / Disaster | 01 Stop the Flames | Collect firefighting equipment; extinguish marked fires |
| 1 / Disaster | 02 Clean the Wasteland | Clear designated burnt debris and prepare planting locations |
| 2 / Recovery | 03 Plant the Future | Collect seedlings; plant; water each young tree |
| 2 / Recovery | 04 Bring Life Back | Clean water source and restore designated habitat |
| 3 / Restored | 05 A Promise to the Forest | Plant/water final tree; reach viewpoint; ending cinematic |

Connected areas in one scene are preferred over three unrelated maps. World
changes follow completed player actions. Mature forest growth needs a narrative
time transition, not a claim of instant real-world recovery. Wildlife feeding
is optional in the source document; animal AI, damage, fire spread and timers
are not implicitly required.

## First milestone: Disaster Zone vertical slice

- Work branch: `feature/from-ashes-zone1`.
- Team baseline: `3611552b646e002b22bc903f21e38c3682000a09`.
- New editable scene: `Assets/Scenes/FromAshes.unity`, an original RIMBARA layout
  assembled from the EmaceArt asset library. Neither vendor demo layout nor team
  VillageRestoration layout is copied. See [the resource rule](Slavic-Base.md).
- Coordinator's staged dialogue, equipment pickup, actual water/fire interaction,
  four designated debris piles, and a Recovery-ready chapter completion screen.
- Equipment appears on the player only after pickup. Smoky fog/dim lighting
  becomes lighter after containment; this is a stylized progress cue, not a
  weather or physically simulated smoke system. Birds do not return merely
  because the fire counter increased.
- Zone 1 environment follows the source: quiet village/evacuation, emergency
  post, ash-dark terrain, charred standing/fallen trees and active forest fires.
  The resource library is retained in full, but the scene is not an exhibition
  of every kitchen prop, nor a copy of the demo's picturesque restored village.
- The player cannot plant before extinguishing and cleaning. Existing MVP systems
  remain available in their original scenes. No automatic scene regeneration.
- Two fires and four debris piles are **provisional tuning**, not counts specified
  by the story. Legacy planting placeholders are hidden pending milestone two.
- This first slice is not the full five-mission game. NPC escort, opening/ending
  cinematics, plant watering, habitat restoration and final tree are later work.

## Follow-up milestones

Zone 2 source-document review and proposed acceptance criteria:
[Zone2-Concept.md](Zone2-Concept.md). This is a design study, not a claim that
Recovery gameplay is already implemented.

Recovery implementation is now in progress on `feature/recovery-zone2` from the
team's merged `f4ec160`. See [Zone2-Implementation.md](Zone2-Implementation.md)
for executed features and current validation; the concept remains the design source.

Zone 3 requirements, visual transformation, M05 gates, time transition and ending
plan: [Zone3-PRD.md](Zone3-PRD.md). This PRD is a draft for team review; no Restored
gameplay is implemented by writing it.

The user subsequently authorized building Zone 3. Work is local on
`feature/restored-zone3`; see [Zone3-Implementation.md](Zone3-Implementation.md)
for actual features and test status. No publication/merge is implied.

1. Recovery: update inventory/targets, implement per-tree watering, clean source
   and habitat interactions, and progress-driven lighting/audio/vegetation.
2. Restored: reserve inventory for the final tree; require watering and viewpoint
   before victory; add time transition, elder dialogue and ending camera sequence.
3. Narrative polish: villager escort, opening cinematic, achievements, localization
   and human playtests. Preserve both source dialogue and concise interactive text.

## Repository/import findings

The first clone returned Git LFS 404 for baseline assets (including Idle.fbx).
126 checked-out paths were restored locally from the original MVP only after
matching their recorded SHA-256. No team asset was substituted. Local LFS fsck
passes. The missing upstream objects still need to be uploaded by a maintainer;
local recovery does not repair the remote repository.

The team URP global settings have schema version 10, newer than the schema 9
supported by the pinned editor. The storyline build creates an additional
`Assets/Settings/StorylineURPGlobalSettings.asset` using Unity's current API and
selects it in Graphics Settings; it does not fake a downgrade of the original
asset's version. The new asset uses the installed URP's supported default Render
Graph path; the original global settings remain available for comparison.

Use Unity **6000.3.6f1**. Original Docs/Evidence results are historical MVP evidence,
not tests of this storyline. New tests/results must identify the tested scene
and commit. Review asset redistribution licences before distributing the public
repository or builds; do not silently change repository visibility.

## Reproduction commands

With this project closed in Unity:

```powershell
# Creation is one-time only and authors the original Disaster map from kit resources.
# Existing converted workspaces: skip create/slavic and edit FromAshes normally.
powershell -ExecutionPolicy Bypass -File Tools/Invoke-Storyline.ps1 -Command create
powershell -ExecutionPolicy Bypass -File Tools/Test-EditMode.ps1
powershell -ExecutionPolicy Bypass -File Tools/Invoke-Storyline.ps1 -Command verify
powershell -ExecutionPolicy Bypass -File Tools/Invoke-Storyline.ps1 -Command build
powershell -ExecutionPolicy Bypass -File Tools/Test-StorylineRuntime.ps1
```

Run the graphical runtime test by itself, not in parallel with another Unity
Editor/import process that can take window focus or contend with input frames.

Runtime fixtures exercise actual E/mouse input, pointer clicks through the
EventSystem, water raycasts and the official player motor. They teleport between
isolated interaction fixtures, then physically approach each target. They are
not evidence that every route through the village is navigable, nor a human
playtest, performance benchmark or complete five-mission test. They run only
with `--rimbara-smoke`; normal play creates no automatic actions or fixtures.

## Remaining acceptance work

- A human must walk the complete route from spawn through every target without
  teleporting. Original road/corridors reserve interaction clearance; scenery
  collision must not be disabled to claim route coverage.
- Review camera framing near the emergency post and dialogue text on multiple
  screen sizes. World labels are resized for the Slavic village. The first slice
  reuses existing UI wiring rather than importing binary Arunika scene UI.
- Equipment chest and wood piles use EmaceArt prefabs; coordinator reuses Ch42 as a stationary
  placeholder. NPC escort and a distinct coordinator appearance are later polish.
- The source already contains a CS0414 warning in Farm UI `CustomButton.isHeld`.
  Keep vendor source unchanged unless a separate vendor maintenance task is agreed.
