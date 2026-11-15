# Slavic kit is a resource library, not our level layout

User-selected asset library: **Slavic Medieval Village Free – Modular Environment
Kit**, EmacEArt, Marketplace product **167010**. Imported content remains in
`Assets/EmaceArt/Slavic World Free`.

The user explicitly clarified: use the kit's models, prefabs, textures and
materials to build **our own RIMBARA map**. Do not copy the vendor demo layout,
terrain, hierarchy, village placements, bridges or showcase scenery. Not every
food item/interior prop belongs in a disaster level; all imported resources remain
available, while placed resources are chosen for the storyline.

## Original Zone 1 composition

- A village arrival road with four quiet houses and evacuation supplies.
- An emergency mission post with a shelter, map/mission board, briefing table,
  equipment chest, water reserves and waiting volunteers.
  Its **POSKO DARURAT** inscription is attached to the front header of the board,
  not a floating camera-facing label. Objective beacons remain separate.
- Burnt forest beyond the village: blackened ground, charred standing/fallen
  trees, branches, two marked fires and four cleanup clearings.
- Dim, smoke-tinted lighting, drifting overhead smoke, fire/wind ambience.
- Central walking corridors and reserved open approaches to every objective.
- The map is not already restored: lush showcase greenery and a clean picturesque
  demo river are not carried into Zone 1.

`Assets/Editor/DisasterMapBuilder.cs` places individual EmaceArt resources at
original authored positions and creates a new paintable terrain using a texture
from the kit's field resource. It never opens a demo scene. FromAshes contains
the editable result; gameplay wiring/UI/player are reused, not the template map.

The previous mistaken demo-layout scene is backed up under ignored Backups.
Only this branch's FromAshes scene changes. Team Main/Arunika/VillageRestoration
and vendor scenes/prefabs are preserved. Material overrides are on our instances
or derived assets, never destructive edits to the source kit.

Older demo-layout build/runtime evidence is obsolete for the new map. Reports
are refreshed only after testing the original Disaster layout. The arrival road
and first forest approach are also tested with physical movement, not teleport.

The official listing specifies Standard Unity Asset Store EULA, not CC0.
Existing imported resources are used; no new purchase/download, raw-asset public
redistribution approval, or assertion of latest Marketplace version is implied.
