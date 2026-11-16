# Editable NPC authoring sources

These `.blend` files are generated character authoring sources, **not Unity
import assets**. Open them with Blender 4.5 LTS and MPFB 2.0.18 to adjust the
body phenotype/clothing. Textures are packed into each source file.

The reproducible pipeline is `Tools/Generate-HumanNPCs.py`. Keep MakeHuman/MPFB
tool code and the complete downloaded graphical library outside the project.
Selected Unity-ready exports belong in `Assets/Art/Characters/HumanNPCs`.

Bundled graphical assets/output are CC0; see `Docs/Licenses/HumanNPCs` and
`Docs/Human-NPC-Implementation.md`. This source folder does not contain the
game's separately licensed Mixamo animation files.
