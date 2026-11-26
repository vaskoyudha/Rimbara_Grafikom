# Fantastic UI Starter Pack import

RIMBARA uses the free **FANTASTIC UI STARTER PACK** by MyIndie.Games:

<https://assetstore.unity.com/packages/2d/gui/icons/fantastic-ui-starter-pack-123733>

The pack contains 126 named, 9-sliced sprites (panels, buttons, labels, slots,
bars, widgets, icons, silhouettes), ready uGUI prefabs, and small helper scripts.
It targets Unity 6 and URP.

## Why it is not in the repository

`github.com/vaskoyudha/Rimbara_Grafikom` is public. The Unity Asset Store EULA
allows use in games but forbids redistributing the asset files, so the kit is
imported locally and gitignored at `Assets/ThirdParty/FantasticUI/`.

## Required versions

- Unity `6000.3.27f2` (changeset `22188f149405`)
- URP `17.3.0`, uGUI `2.0.0`, TextMeshPro, Input System `1.20.1`

## Import steps

1. In the Asset Store, open the pack and choose **Add to My Assets** (it is free).
2. Open the project in Unity `6000.3.27f2`.
3. Use **Window > Package Manager > My Assets**, find **Fantastic UI Starter Pack**,
   and choose **Download** then **Import**.
   Alternatively use **Assets > Import Package > Custom Package…** and select the
   downloaded `.unitypackage`.
4. Wait for the import and the domain reload to finish. Import TMP Essential
   Resources if Unity prompts for it.
5. Run `RIMBARA/UI/Report Fantastic Import`. It writes
   `Artifacts/fantastic-import.json` with the detected kit root and every prefab,
   sprite and font.
6. Run `RIMBARA/UI/Build Fantastic theme`. It creates
   `Assets/UI/FantasticUiKit.asset` and fails loudly if any required role is
   unresolved. Check the Console for the chosen asset path per role.
7. Run `RIMBARA/UI/Apply Fantastic theme to open scene` once for each scene that
   contains the `UI - RIMBARA` canvas: `Main`, `FromAshes`, `Arunika`, and
   `VillageRestoration`. The command skins the open scene and saves it.
9. Run `RIMBARA/Verify authored project` and
   `RIMBARA/Storyline/Verify Disaster chapter`.

## Fresh clones

A fresh clone does **not** contain the kit. EditMode copy/layout tests still
pass (the kit-dependent tests call `Assert.Ignore`). Opening `FromAshes` or
`Main` without the kit shows missing sprite references, and project verification
fails with:

> Fantastic UiKit is missing or incomplete. Import the kit and run
> RIMBARA/UI/Build Fantastic theme.

## Licence summary

- Standard Unity Asset Store EULA; the pack is free.
- Use in unlimited personal and commercial projects.
- Do not resell or redistribute the sprite/prefab files as an asset pack.
- The kit ships no fonts. RIMBARA falls back to TextMeshPro's LiberationSans SDF,
  which is covered by the Unity Companion License.
