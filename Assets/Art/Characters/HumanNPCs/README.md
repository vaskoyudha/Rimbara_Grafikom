# RIMBARA human NPCs

Eight generated CC0 character identities. Source `.blend` files are under
`SourceArt/HumanNPCs` (outside Unity's import tree). These FBX models are not
copies of the Ch42 player and do not include newly downloaded Mixamo content.

`manifest.json` records body settings, height, triangle counts and material
bindings. `source-hashes.json` records selected MakeHuman graphical inputs.

Licences/provenance: `Docs/Licenses/HumanNPCs` and
`Docs/Human-NPC-Implementation.md`. Existing game animations remain separately
licensed; CC0 on these models does not apply to the whole project.

Install/verify using `Tools/Invoke-Storyline.ps1 -Command human-npcs` /
`-Command verify-npcs`, with the Unity project closed. Installation targets
visual-only containers in `FromAshes`, not original vendor/team scenes.
After an existing model export changes, use `-Command npc-imports` to refresh
materials/avatars without replacing scene containers again.
