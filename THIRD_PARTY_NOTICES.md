# Third-party assets and licences

This Unity project includes both CC0 and licensed assets. No blanket
open-source licence is applied to all repository content. Third-party rights
remain with their respective owners.

## Unity Starter Assets

- **First Person + Third Person | Character Controllers**, product **196526**,
  version **2.0.2**, by **Unity Technologies**.
- Acquired through the owner's Unity My Assets library and Unity's authenticated
  Asset Store downloader; not sourced from an unofficial GitHub copy.
- Source/assets are preserved under `Assets/Starter Assets`.
- Licence shown by the official listing: **Unity Companion License**.
- The current official licence text is included in
  `Docs/Licenses/Unity-Companion-License.txt` (v1.4, dated 2024-10-29).
- Copyright Unity Technologies; trademarks remain their owners' property.
- [Official listing](https://assetstore.unity.com/packages/3d/characters/first-person-third-person-character-controllers-196526)
- [Official licence](https://unity.com/legal/licenses/unity-companion-license)

## Mixamo / Adobe

- **Y Bot**, **Idle**, **Walking (Male Standard Walk)**, and **Running** from
  [mixamo.com](https://www.mixamo.com/).
- Imported source FBX files are under `Assets/Art/Characters/Mixamo`.
- Walking and running were exported in place, without skin, as FBX for Unity,
  30 FPS, with no keyframe reduction. Y Bot supplies the character skin/avatar.
- These are licensed Mixamo assets, **not open-source or CC0 assets**.
  Use them as part of the game under Adobe's applicable terms. Do not publish
  the raw character/animation files as a standalone redistributable asset pack.
- [Adobe Mixamo FAQ and licensing information](https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html)

## Kenney Nature Kit

- **Nature Kit 2.1**, created/distributed by **Kenney**.
- 32 selected FBX source meshes are under
  `Assets/Art/Environment/KenneyNature`; project-prefab variants/materials are
  generated from those meshes.
- Licence: **Creative Commons Zero (CC0)**. The original licence notice is
  preserved as `Assets/Art/Environment/KenneyNature/License.txt`.
- [Official asset page](https://kenney.nl/assets/nature-kit)
- [CC0](https://creativecommons.org/publicdomain/zero/1.0/)

## Unity packages and TextMeshPro resources

URP, Cinemachine, Input System, uGUI/TextMeshPro, Test Framework, and their
dependencies are resolved through Unity Package Manager. Their package
licences and notices remain authoritative. Package caches are not committed.
TextMeshPro essential resources include their supplied font notices/licences,
which are preserved in the imported `Assets/TextMesh Pro` content.

## Team-added assets (source licence review required)

- Mixamo **Ch42**, its textures and Humanoid import are now also present under
  `Assets/Art/Characters/Mixamo`. The same Adobe terms apply; no CC0 claim is made.
- Additional Kenney Fantasy Town, Nature, Food, Furniture and Survival notices
  are preserved under `Assets/RIMBARA/Licenses`. Kenney UI/font pack notices
  remain under `Assets/External`.
- `Assets/EmaceArt/Slavic World Free` and
  `Assets/maanetorn/Farm Game UI - Simple 2D UI` were added by the team. Retain
  vendor notices; verify their Asset Store/source redistribution terms before
  distributing raw assets. "Free" does not imply public-domain licensing.
- These additions were already in team commit `3611552`; storyline development
  does not assert that their public source redistribution has been authorized.
- The selected environment base is **Slavic Medieval Village Free – Modular
  Environment Kit**, product **167010**, EmacEArt. Its official listing specifies
  **Standard Unity Asset Store EULA**, not CC0. Existing imported content is used;
  no claim is made that it matches the latest Marketplace release.

## MakeHuman / MPFB human NPC graphical assets

- Eight generated human-proportioned NPC identities, authored with MPFB
  **v2.0.18**, using bundled MakeHuman **1.3.0+20240427162744** graphical assets
  from the official `makehuman-official` PPA source distribution.
- Bundled base mesh, targets, rig, eyes, eyebrows, hair, clothing, shoes and skin
  data/output are **CC0 1.0**. This does not relicense the game, existing Mixamo
  animations, Unity assets, or third-party tools.
- Source archive SHA-256:
  `f9b8c4b3167b49fbeb3f231ca3c77e88b8e379bb72ffd0d5f30002e84b9490ae`.
- MakeHuman Team; asset notice also names Data Collection AB, Joel Palmius,
  and Jonas Hauquier. No trademark/endorsement rights are claimed.
- Licences/provenance: `Docs/Licenses/HumanNPCs/` and
  `Docs/Human-NPC-Implementation.md`.
- MakeHuman/MPFB program code is used externally for authoring, not shipped in
  Unity. Its separate AGPL/GPL terms remain unchanged.

## Wikimedia Commons SVG text dividers

The ornamental UI divider sprites under `Assets/Art/UI/Dividers/` were
converted to white-on-transparent PNGs (`ElegantDivider_Ornate.png`,
`ElegantDivider_Slim.png`, `ElegantDivider_Ring.png`,
`ElegantDivider_Flare.png`) from **Public domain** SVG works in
Wikimedia Commons (`Category:SVG_text_dividers`):

- `File:Decorative text divider 5.svg` (`https://upload.wikimedia.org/wikipedia/commons/2/21/Decorative_text_divider_5.svg`) -> `Assets/Art/UI/Dividers/ElegantDivider_Ornate.png`
- `File:Decorative text divider 3.svg` (`https://upload.wikimedia.org/wikipedia/commons/9/9e/Decorative_text_divider_3.svg`) -> `Assets/Art/UI/Dividers/ElegantDivider_Slim.png`
- `File:Decorative text divider.svg` (`https://upload.wikimedia.org/wikipedia/commons/6/63/Decorative_text_divider.svg`) -> `Assets/Art/UI/Dividers/ElegantDivider_Ring.png`
- `File:Decorative text divider - central flare.svg` (`https://upload.wikimedia.org/wikipedia/commons/7/7c/Decorative_text_divider_-_central_flare.svg`) -> `Assets/Art/UI/Dividers/ElegantDivider_Flare.png`

## FreeGameUI CC0 9-slice UI panels

The card inset sprite under `Assets/Art/UI/Panels/` was converted to PNG from a
**CC0 1.0 (Public Domain)** SVG panel asset from Free Game UI (`freegameui.net`):

- `panel_rect_tinted-dark_bracket_03.svg` (`https://cdn.freegameui.net/svg/panel/rect/tinted-dark/panel_rect_tinted-dark_bracket_03.svg`) -> `Assets/Art/UI/Panels/CardInset_DarkGoldBracket.png`

## Original project content

RIMBARA's gameplay scripts, mission/profile setup, scene/UI/effect generators,
and procedural WAV audio are original project content. Their presence here
does not relicense any third-party code, mesh, animation, font, or texture.

Before making this repository public or redistributing source separately,
review the relevant asset terms or replace assets whose source cannot be
redistributed in the intended manner.
