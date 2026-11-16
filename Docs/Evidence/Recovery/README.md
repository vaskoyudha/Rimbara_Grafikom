# Recovery evidence — original two-zone map

Historical two-zone milestone. Current three-zone work is validated separately
under `Docs/Evidence/Restored`; these counts do not cover the M05 ending.

Applies to the local `feature/recovery-zone2` working tree from team merge
`f4ec160`, not the previous `096d8e9` Zone 1 build. Scene: `Assets/Scenes/FromAshes.unity`.

- `editmode-tests.xml`: 77 passing cases (56 baseline + 18 Recovery + 3 river).
- `runtime.json`: 181 checks through Disaster and Recovery/river, with no runtime errors.
- `build.json`: current Windows x64 build result, not a redistributed executable.
- `recovery-scene.json`: four seedlings/beds, ten trash targets, two habitats.
- `river.json`: original 27 m channel, 10 unique dry-bank targets and 2 solid footbridges.
- `scene.json`: Disaster reference supply and no missing scripts.
- `language.json`: serialized UI/world Indonesian coverage.
  53 UI labels, 44 world labels, zero untranslated entries.
- `preservation.json`: no Zone 1 baseline objects deleted; only the terrain origin
  is lowered with height compensation, and channel sculpting stays local.
  Original team scenes are preserved. Audit compares pre/post sculpt world heights.

Captures under `Docs/Captures/Recovery` show the reviewed run. Auxiliary close-up
cameras are only installed in opt-in diagnostics; they don't alter normal play.
Local interaction fixtures use teleport between many targets plus actual physical
approach/E/mouse. The route from the ranger along both riverbanks and across the
north footbridge visits all ten trash targets without teleport. Other selected
corridors are physically walked. This is not a full-map
human navigation test, performance certification, or proof of Zone 3 completion.

Models, water and habitat details remain stylized/placeholders as documented in
[Zone2-Implementation.md](../../Zone2-Implementation.md). No mature forest growth,
final tree/viewpoint, ending cinematic or wildlife AI is claimed.
