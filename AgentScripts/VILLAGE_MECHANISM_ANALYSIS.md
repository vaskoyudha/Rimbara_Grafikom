# RIMBARA village update — what gameplay it actually adds

Read-only analysis of the friends' pushed commits (`origin/main` = `397f359`)
against the local base commit `8e5e8e5`. No Unity, scene or asset edits were
made. The user's current map is `Assets/Scenes/Arunika.unity`.

## Verdict

**The village update adds no new gameplay mechanism.** It is decorative
scenery plus solid collision, bundled with a re-authored `Main.unity` map
layout. There are no new scripts, no new MonoBehaviours, no new triggers, no
new interactables, and no new runtime/build evidence. The four missions and
the three existing trigger volumes are unchanged.

If the user saw "gameplay" in their build, it is either the existing RIMBARA
mission loop (collect → plant → spray → return) or the relocated fires
described below — not anything interactive in the village.

## Commits inspected

| Commit | Date (+07) | Content |
| --- | --- | --- |
| `2972b03` | 2026-11-11 20:49 | Adds `Assets/RIMBARA/` (Kenney Fantasy Town / nature / props), `AgentScripts/BuildRimbaraEnvironment.cs` (537 lines) and `BuildVillageHouses.cs` (249 lines), edits `WaterSpray.prefab`, rewrites `Assets/Scenes/Main.unity` |
| `bf2666b` | 2026-11-11 21:09 | README clone instructions only |
| `397f359` | 2026-11-11 21:17 | 5-line generator placement fix (overlap/collider-size filter, Shed_2 moved) + `Main.unity` re-save |

`git diff --stat 8e5e8e5 origin/main -- Assets/Scripts/` is **empty** — no
runtime C# changed. Only editor-time generators were added.

## `RIMBARA_Village` object census (`origin/main:Assets/Scenes/Main.unity`)

Root `RIMBARA_Village` (Transform id 38533574) children:

| Group | Children |
| --- | --- |
| Houses | 8 (prefab instances: House_A…F, Shed_1, Shed_2) |
| Plaza | 28 (roads, fountain-round/center, 4 pillars, 4 lanterns, 2 banners, roof-gable) |
| Trail | 8 (road pieces + lanterns + signpost) |
| Props | 1 (barrel) |
| Fences | 3 |
| Vegetation | 58 (palms, dark trees, bushes, flowers, grass) |

Census of the whole subtree (115 Transforms):

- 100 MeshFilter + 100 MeshRenderer
- **13 BoxColliders, all `m_IsTrigger: 0`** (lantern ×4, fence ×3, pillar-wood ×4, fountain-round, barrel)
- 0 Capsule/Sphere/Mesh colliders (the generated vegetation capsule colliders are not present in the saved scene)
- **0 triggers**
- **0 MonoBehaviours** (no `IInteractable`, no custom script, no `AudioSource`, `Light` or `ParticleSystem`)

The eight house/shed prefabs are equally inert: each has 15–27 GameObjects,
**0 `m_Script` entries**, and 6 non-trigger BoxColliders. They are solid
architecture, nothing more.

The generator can only add colliders, never behaviour:
`BuildRimbaraEnvironment.cs:418` (`AddComponent<BoxCollider>`) and `:424`
(`AddComponent<CapsuleCollider>`). No `AddComponent` of any gameplay type
exists in either added AgentScript.

## Whole-scene gameplay comparison (base → origin/main)

| Signal | `8e5e8e5` (base) | `origin/main` | Change |
| --- | --- | --- | --- |
| Scripted MonoBehaviour instances | 5 SeedlingPickup, 5 PlantingPoint, 2 FirePoint, 2 MissionZone, 1 MissionBoard, 1 InteractionDetector, 1 WaterSprayer, 1 VolunteerActor … | identical counts | **none** (only extra script GUID is URP `UniversalAdditionalLightData`, a package component) |
| Trigger colliders | 3 | 3 | **none** (Inspection marker, Volunteer mission board, Volunteer post return zone) |
| Total colliders | 18 | 219 | +201, all scenery/obstacle colliders, 0 triggers |
| Interactable prefab instances | 5 NativeSeedling, 5 PlantingBed, 2 RemainingFire | identical | **none** |
| Seedling / planting-bed world positions | — | byte-identical | **none** |
| Fire positions | (11, 58) and (−8, 49), Zone C | (11, −16) and (−10, −12), Zone A | **relocated** |
| Zone labels | A volunteer post/damaged forest, B seedling nursery, C remaining fires | A burned forest/volunteer post, B forest restoration/nursery, C recovered forest | renamed |

So the commit re-authors the map (fires now burn beside the volunteer post /
burned forest, village sits at x≈23–47, z≈−19…12) but the mission logic,
counts, interaction range and victory conditions are untouched.

## Build evidence: none for the village

- `git diff --stat 8e5e8e5 origin/main -- Docs/` is **empty**. No JSON, XML,
  capture or verification text changed in any of the three commits.
- `Docs/Evidence/full-runtime.json` and `week1-runtime.json` are timestamped
  **2026-11-07** (17:09Z / 17:10Z), four days before the village commits.
  They still report 56 scene checks / 36 EditMode tests / 79 full-MVP checks /
  69 Week 1 checks for the same four missions.
- `git grep -i village origin/main -- Docs/` matches only unrelated text
  ("Fire 2 extinguished…"); there are no village, house, signpost or fountain
  checks anywhere.
- README (`bf2666b`) only edits clone instructions; the stated gameplay is
  still "5 seedlings / 5 trees / 2 fires, four ordered missions".

## Other runtime asset change

`Assets/Prefabs/FX/WaterSpray.prefab`: particle `startSizeY` / `startSizeZ`
`minMaxState` changed `0 → 3` (fixed size → random min/max). Visual only.

## What reproducing it in `Arunika.unity` requires

1. **Gameplay: nothing.** Arunika already contains the same systems
   (5 `SeedlingPickup`, 5 `PlantingPoint`, 2 `FirePoint`, 2 `MissionZone`,
   mission board, return zone, `MissionSystem`/`GameFlow`). Note the scene is
   binary-serialized (Unity 6000.3.6f1), so this was verified from embedded
   type/name strings, not YAML.
2. **Assets.** `Assets/RIMBARA/` is **absent from the local tree**; it must be
   copied/imported from `origin/main` (Kenney CC0 models, materials, the eight
   house/shed prefabs, licenses — roughly 200 files).
3. **Placement.** Either place the eight prefabs and plaza/trail/fence/prop
   sets by hand, or adapt `AgentScripts/BuildRimbaraEnvironment.cs`. That
   script is editor-only, needs an active `Terrain`, deletes every root whose
   name starts with `RIMBARA_` (`:82`), and uses absolute world XZ tuned to
   `Main` (village x≈23–47 / z≈−19…12, fountain 36.75/−3.75, trail to z≈−22,
   camp x≈10–19 / z≈13–19). On Arunika those coordinates land wherever they
   land relative to the terrain and existing mission content, so they must be
   re-authored, not just re-run.
4. **Collision clearance.** Every village piece is a solid obstruction used by
   `InteractionDetector`'s linecast and by `WaterSprayer`; the 13 prop
   colliders plus 6 per house (≈61 total) can block the 2.5 m interaction
   radius or the water stream. `397f359` exists precisely because of overlap
   bugs (it now ignores colliders < 0.75 m³ and moves `Shed_2`). Keep the 5
   seedlings, 5 planting beds, 2 fires, mission board and the three triggers
   clear, and do not overlap the ~22 existing obstacles.
5. **Only if he wants real interaction** (enter houses, talk to villagers,
   fetch/trade) it must be written from scratch: a subclass of
   `InteractableBehaviour`/`IInteractable`, a trigger `Collider` on the
   interaction layer, and registration/wiring into `GameState` and the mission
   sequence. None of that exists in these commits.

## Caveat / open item

The only player-affecting changes in the pushed work are (a) solid colliders
as obstacles, and (b) the two fires moved from the far zone to the burned
forest beside the volunteer post. If the user remembers a distinct mechanic,
get a screenshot or clip plus the build folder it came from — the commits
contain nothing else to reproduce.
