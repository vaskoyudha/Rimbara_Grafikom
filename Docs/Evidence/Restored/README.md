# Restored evidence — three-zone storyline

Scene: `Assets/Scenes/FromAshes.unity`, local `feature/restored-zone3` working tree,
based on merged team `f4ec160` plus retained Zone 2/river development.

Reports are copied only after fresh runs; earlier Zone 1/2 captures do not
validate the final tree, time transition or ending.

- `editmode-tests.xml`: final domain/regression test run.
  95 passing cases (77 baseline + 18 final-chapter cases).
- `runtime-normal.json`: complete three-zone run with the normal ending.
- `runtime-skip.json`: complete three-zone run with pause and explicit ending skip.
  Normal path: 207 passing checks. Skip path: 206 passing checks.
- `build.json`: tested Windows x64 build result.
- `scene.json`, `restored-scene.json`: reference/object/ending supply audits.
- `language.json`: current serialized Indonesian UI/world audit.
  55 UI labels and 48 world labels; zero untranslated entries.
- `preservation.json`: prior Zone 2/river scene objects retained, previous
  transforms not moved; team source scenes unchanged.

Normal play has no automatic driver. All runtime input fixtures are opt-in;
some targets are reached by teleport plus physical approach. Riverbanks and
final elder/tree/viewpoint corridors are traversed with the real motor.
This is not a human full-map playtest, platform-wide focus-loss validation,
multi-resolution certification or stable performance benchmark.

Restored visuals, elder, ambient NPCs/animals and generated music are documented
in [Zone3-Implementation.md](../../Zone3-Implementation.md). Opening cinematic,
villager escort, achievements and full saves are not claimed.
