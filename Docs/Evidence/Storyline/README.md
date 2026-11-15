# Storyline evidence

These reports apply to the Disaster Zone slice in `Assets/Scenes/FromAshes.unity`
on local branch `feature/from-ashes-zone1`, based on team commit `3611552`.

- `editmode-tests.xml`: 56/56 passing tests (36 legacy + 11 Disaster + 9 language).
- `scene-verification.json`: chapter references, supply and interaction checks.
- `build.json`: final local Windows build result and warning/error count.
- `runtime.json`: passing 50-check Indonesian original Disaster map run; not a full-map
  navigation test. See [verification boundaries](../../Storyline-Verification.md).
- `slavic-base.json`: kit-resource provenance and original-layout checks.
- `indonesian.json`: scene-language coverage (53 UI and 22 world labels).
- `post-sign.json`: board attachment, fixed orientation and front-header fit.

Earlier failed diagnostic attempts are not represented as passing evidence.
Earlier Kenney and copied-demo evidence must not validate the newly authored map.
The retained evidence and screenshots now come from the original-map run.
The full raw logs and build output remain local in ignored Artifacts/Builds.
