# Kingsforge UI kit import

RIMBARA uses the paid **Kingsforge — Fantasy RPG UI Kit** by Certainity:
<https://certainity.itch.io/kingsforge-fantasy-rpg-ui-kit>

## Why it is not in the repository

`github.com/vaskoyudha/Rimbara_Grafikom` is public. The Kingsforge licence allows
use in commercial and non-commercial games, but forbids reselling or sharing the
sprites as an asset pack. Committing the kit would redistribute its source files,
so the kit is imported locally and gitignored.

## Required versions

- Unity `6000.3.27f2` (changeset `22188f149405`)
- URP `17.3.0`, uGUI `2.0.0`, TextMeshPro, Input System `1.20.1`

## Import steps

1. Purchase/download `Kingsforge_Fantasy_RPG_UI_Kit.unitypackage`.
2. Open the project in Unity `6000.3.27f2`.
3. `Assets > Import Package > Custom Package…` and select the `.unitypackage`.
4. Wait for the import and the domain reload to finish.
5. Run `RIMBARA/UI/Report Kingsforge Import`. It writes
   `Artifacts/kingsforge-import.json` with the detected kit root and every
   prefab, sprite and font.
6. Run `RIMBARA/UI/Build Kingsforge theme`. It creates
   `Assets/UI/KingsforgeUiKit.asset` and fails loudly if any required role is
   unresolved. Check the Console for the chosen asset path per role.
7. Open `Assets/Scenes/FromAshes.unity` and run
   `RIMBARA/UI/Apply Kingsforge theme to open scene`.
8. Open `Assets/Scenes/Main.unity` and run the same command.
9. Run `RIMBARA/Verify authored project` and
   `RIMBARA/Storyline/Verify Disaster chapter`.

## Fresh clones

A fresh clone does **not** contain the kit. EditMode copy/layout tests still
pass (the kit-dependent tests call `Assert.Ignore`). Opening `FromAshes` or
`Main` without the kit shows missing sprite references, and project verification
fails with:

> Kingsforge UiKit is missing or incomplete. Import the kit and run
> RIMBARA/UI/Build Kingsforge theme.

## Licence summary

- Use in unlimited personal and commercial projects.
- Do not resell or share the sprites as an asset pack.
- Do not use the art to train AI.
- Fonts (Cinzel, Alegreya Sans) are SIL OFL 1.1.
