# RIMBARA — 3-developer job split (v4, load-balanced)

Scope contract: `RIMBARA_Game_Design (1).pptx` (VANARO STUDIO — Vasko • Nabila • Rozi).
v4 replaces v3's discipline-only split with **one vertical lead each plus a cross-cutting
specialism**, so the three tracks land at roughly the same number of hours.

Estimates below are planning numbers, not commitments. Week 1 re-estimates every card;
the rule is that no track stays above 115 % of the median after week 2.

---

## 0. Deck contract (unchanged)

| Slide | Commitment |
|---|---|
| S1 | 3D adventure, Unity, PC/Windows, single-player, target 4 weeks |
| S4 | 5 seedlings, 5 trees, 2 fires, 10–15 min per session |
| S5 | 4 missions: First Steps → Seeds of Hope → Stop the Fire → A New Beginning |
| S6 | No score points; progress = `(trees + fires) ÷ 7`; no lose condition |
| S7 | WASD, mouse, Shift, E, LMB, Esc |
| S9 | Menu (optional) → Briefing → Gameplay + HUD → Ending; Esc pause |
| S10 | HUD = mission name/description, seedling + tree counters, fire counter, `[E]` prompt |
| S11 | MVP list; extras = sapling animation, wind/bird/fire audio, animals, menu/pause/volume |
| S11 | Excluded: open world, five chapters, realistic fire sim, weather, animal AI, multiplayer, crafting |

Ship the deck's 4 missions. The existing 5th chapter (river/habitat/restored) stays behind
a flag and does not enter the submission build.

---

## 1. Blocking findings (week-1 list)

| # | Finding | Evidence |
|---|---------|----------|
| A1 | Four parallel scenes from four Editor generators | `Assets/Scenes/*.unity`, `Assets/Editor/*Builder.cs` |
| A2 | Campaign scene `FromAshes` missing from build settings | `ProjectSettings/EditorBuildSettings.asset` |
| A3 | `DisasterHud` + legacy `RimbaraHud` both in `FromAshes` | `Assets/Scenes/FromAshes.unity` |
| A4 | World/tuning/copy are C# constants in Editor scripts | `ProjectBuilder.cs:31-35`, `HudFactory.cs:40-118` |
| A5 | No save; pause screen claims progress is saved | `IndonesianText.cs:41`, `GameFlow.cs:110-116` |
| A6 | Input hardcoded; gamepad has no camera look | `VolunteerInput.cs:38-54` |
| A7 | One `WaterSprayer` for everything; prompt can target behind the player | `WaterSprayer.cs`, `InteractionDetector.cs:33-52` |
| A8 | 130 agent scripts, 25 MB test playground, 97 MB unused kit, orphan meta, Windows-only scripts | repo root, `Assets/maanetorn/`, `Tools/*.ps1` |
| A9 | No PlayMode tests; smoke driver ships in the player | `RuntimeSmokeDriver.cs` |
| A10 | 13 `Update` commits; one unmerged commit on `origin/village-scenery` | `git log` |

---

## 2. Contracts (freeze week 1)

1. `RestorationScore`: seedlings, trees, fires, `(trees+fires)/7`, mission 1–4.
2. Mission order: 4 sequential stages with deck triggers.
3. Save: versioned snapshot at mission boundaries and dialogue completion.
4. Events: `ObjectiveChanged`, `CounterChanged`, `ScoreChanged`, `ScreenRequested`, `Interacted`, `Saved`.
5. UI: view-models in, no `GameState` reads, all strings are localization keys.
6. Input: one `.inputactions` asset, gameplay/UI maps, rebinding, gamepad parity.
7. Ownership: section 4; no hand merges of `.unity`.

---

## 3. Balanced split: three verticals + three specialisms

Each person leads **one playable vertical** (menu → mission → reward, end to end) and owns
**one specialism** used by the other two verticals. That is what makes the load even: the
content work is split by mission, not carried by one person.

| Person | Vertical lead | Specialism (used by everyone) |
|---|---|---|
| **Vasko** | **V1** — Menu → Briefing → Mission 1 (First Steps) → HUD | UI/UX, localization, settings |
| **Nabila** | **V2** — Missions 2–3 (Seeds of Hope, Stop the Fire) + world dressing | Game feel: interaction, tools, VFX, audio |
| **Rozi** | **V3** — Mission 4 (A New Beginning) → Victory/Ending → save/resume | Core systems, build, CI, release |

### 3.1 Hour budget (planning estimate)

| Item | Owner | Hours |
|---|---|---|
| **Vasko — V1 + UI/UX** | | **≈105** |
| UITheme, screen inventory | Vasko | 6 |
| Five screens + `UIRouter` prefabs | Vasko | 30 |
| HUD binding to `RestorationScore` view-model | Vasko | 8 |
| World-anchored prompt (facing check shared with Nabila) | Vasko | 6 |
| Localization service + ID/EN string tables | Vasko | 14 |
| Dialogue presentation (typewriter, next/skip) | Vasko | 10 |
| `SettingsService` + rebinding screen | Vasko | 16 |
| Transitions, victory/ending UI, QA matrix, screenshots | Vasko | 10 |
| Mission 1 content: asset, copy, marker placement | Vasko | 5 |
| **Nabila — V2 + game feel** | | **≈98** |
| Interaction feel: facing, highlight, hold, blocked stream, reticle | Nabila | 18 |
| Missions 2–3 content: assets, placement, copy | Nabila | 12 |
| Sapling→tree, fire/smoke stop, ash/steam transitions | Nabila | 10 |
| VFX library + per-mechanic effects | Nabila | 12 |
| Audio library + triggers (footstep, fire, wind, birds, rumble) | Nabila | 12 |
| Collision/navigation blockers for zones A–C | Nabila | 8 |
| Objective assets/evaluator + checkpoint hooks (co-owned Rozi) | Nabila | 14 |
| Prompt wording + HUD data-contract checks | Nabila | 4 |
| Balance for missions 2–3 with playtesters | Nabila | 8 |
| **Rozi — V3 + systems** | | **≈96** |
| Scene consolidation, one HUD, repo cleanup, version pin | Rozi | 14 |
| `RestorationScore` + event bus | Rozi | 10 |
| `SaveService` (atomic, versioned) | Rozi | 12 |
| `.inputactions` asset + action maps | Rozi | 6 |
| Linux+Windows build script + CI | Rozi | 12 |
| PlayMode suite (happy path, resume, score) | Rozi | 12 |
| Smoke driver behind dev-build guard | Rozi | 2 |
| Mission 4 content + victory/ending flow | Rozi | 10 |
| Navigation/spawn/soft-lock assist | Rozi | 6 |
| Release build, tag, evidence, perf baseline | Rozi | 12 |
| **Total** | | **≈299 h** |

Median ≈ 100 h. Variance is under 10 %.

### 3.2 Why this is even

- Content is split **by mission** (1 / 2–3 / 4), not "all levels belong to one person".
- UI work is heavy only in V1; V2 and V3 consume it rather than rebuild it.
- Systems work is heavy only in V3; V1 and V3's co-owned hooks are small, not parallel systems.
- Game feel is heavy only in V2; the other verticals use the shared interaction/tool contract.
- Every person writes code in three layers (their vertical, their specialism, and one
  co-owned contract), which satisfies the "everyone touches systems" rule without a
  second full-time systems role.

### 3.3 Fallback moves (if a track drifts by week 2)

Pre-agreed, so nobody has to negotiate under pressure:

| If | Move | From → To | Hours |
|---|---|---|---|
| Vasko > 115 % median | Localization string content (not the service) | Vasko → Nabila | 6 |
| Vasko > 115 % median | Mission 1 marker placement | Vasko → Nabila | 3 |
| Nabila > 115 % median | Audio triggers for missions 1 and 4 | Nabila → Vasko | 5 |
| Nabila > 115 % median | Zone C collision fixes | Nabila → Rozi | 5 |
| Rozi > 115 % median | Evidence + screenshot packaging | Rozi → Vasko | 4 |
| Rozi > 115 % median | PlayMode resume test | Rozi → Nabila | 4 |

Cut order first (deck S11): animals, camera occlusion, rumble, EN localization, main menu.
Never cut: movement, one map, planting, fire, win screen, HUD counters.

---

## 4. Ownership boundaries

| Path | Owner | Co-owner |
|---|---|---|
| `Scripts/UI/`, `Scripts/Localization/`, `Scripts/Dialogue/`, `Assets/Prefabs/UI/` | Vasko | Nabila |
| `Scripts/Settings/` | Vasko | Rozi |
| `Scripts/{Interaction,Player,World,Audio}/`, `Assets/Prefabs/{Interaction,Environment,Player,FX}/` | Nabila | Rozi |
| `Scripts/Core/`, `Scripts/Diagnostics/`, `Tools/`, `ProjectSettings/`, `Packages/` | Rozi | Vasko |
| `Scripts/Data/` | Rozi | Nabila |
| Campaign scene | Rozi | — |
| Zone content as prefabs | Nabila | — |
| Release build + evidence | Rozi | Vasko |

Edit only your lock, or a co-owned file with its owner's review. Never merge a scene by hand.

---

## 5. Shared systems: two names, two commits

| System | Owner | Co-owner | Co-owner deliverable |
|---|---|---|---|
| `RestorationScore` | Rozi | Vasko | View-model + HUD binding test |
| `SaveService` | Rozi | Nabila | Checkpoint calls at mission boundaries + resume test |
| `SettingsService` | Vasko | Rozi | Persistence + migration test |
| `MissionDefinition` / objectives | Nabila | Rozi | Four `.asset` definitions + evaluator wiring |
| `.inputactions` + rebinding | Rozi | Vasko | Rebind screen |
| `UIRouter` | Vasko | Nabila | Screen-request events from gameplay |
| Interaction facing/highlight | Nabila | Rozi | Collider layers + tests |
| Build/CI | Rozi | Vasko | Release checklist + evidence script |

---

## 6. Review rotation

| Author | Reviewer | Second |
|---|---|---|
| Vasko | Nabila | Rozi (contracts) |
| Nabila | Rozi | Vasko (UI contract) |
| Rozi | Vasko | Nabila (content contract) |

No self-review, no self-merge. Every PR carries PlayMode output, a 30 s capture for feel,
or screenshots for UI.

---

## 7. Four-week plan

| Week | Vasko (V1) | Nabila (V2) | Rozi (V3) | Gate |
|---|---|---|---|---|
| **1** | UITheme, inventory, HUD prefab, delete `HudFactory` | Mission 2–3 assets + placement, facing cone, interaction feedback | Scene consolidation, one HUD, cleanup, version pin | **Contract gate:** one scene, one HUD, deck counts |
| **2** | Router + Menu/Briefing/Pause/Ending prefabs, ID tables | Sapling→tree, VFX/audio hooks, objective evaluator + checkpoint hooks | Score `/7`, save, input asset, Linux+Windows build | **Playable gate:** 4 missions on new foundation |
| **3** | HUD binding, rebinding, settings, transitions | World polish, collision, balance pass 1 | PlayMode suite, CI, smoke driver out of player | **Content gate:** 10/10 playtests finish unaided |
| **4** | Victory/ending UI, QA matrix, screenshots | Balance pass 2, tuning table, capture | Release build, tag, evidence, perf baseline | **Ship gate:** signed build + evidence |

---

## 8. Load checks at each weekly review

a. Each person shipped ≥1 PR in their vertical and ≥1 in their specialism.
b. Card hours are re-estimated; any track above 115 % of the median triggers a fallback move from 3.3 before new cards are pulled.
c. No subsystem has a single contributor after week 3.
d. No self-review, no self-merge.
e. The demo shows a build, a test, or a capture — not slides.

## 9. Working agreement

a. One card = one branch = one PR with a "Done when" line.
b. No direct commits to `main`; no hand merges of `.unity`.
c. Scope changes update the design deck in the same PR.
d. Two corrections on one problem means the card is recut smaller.
