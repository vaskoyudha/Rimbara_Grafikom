# Briefing card icon replacement — research

Date: 2026-11-27
Scope: the four `BuildBriefingTargetCard` icons in the volunteer briefing
(`10 TITIK API`, `6 PUING`, `5 BIBIT`, `1–2 ZONA`) in
`Assets/Scripts/UI/HudTheme.cs`.

## Problem

The briefing cards reuse generic kit slots whose meanings do not match the labels:

| Label | Current role | Current sprite | Problem |
|---|---|---|---|
| TITIK API | `UiKit.IconEmber` | `FantasticUI/Sprites/Icons/Feather_Color.png` | a feather for fire |
| PUING | `UiKit.IconCleanup` | `Broom_Color.png` | a broom, not rubble |
| BIBIT | `UiKit.IconBag` | `Backpack.png` | a backpack, not a seedling |
| ZONA | `UiKit.IconCompass` | `Compass.png` | correct meaning, kit style |

The same roles are also used by the HUD `Restoration counters` where
`IconBag` (equipment) and `IconCleanup` (rubble cleaned) are semantically
acceptable, so the fix is to add dedicated briefing roles rather than repoint
the existing ones.

## Chosen source

**game-icons.net** (via Iconify `game-icons` collection), license **CC BY 3.0**.
Monochrome single-path SVGs, tinted with `UiKit.Gold` / `Cream` at runtime —
matches the existing monochrome-tinted HUD icons. Attribution required.

| Slot | Icon | Author | Source |
|---|---|---|---|
| Fire | `flame` | Carl Olsen | https://game-icons.net/1x1/carl-olsen/flame.html |
| Rubble | `brick-pile` | Delapouite | https://game-icons.net/1x1/delapouite/brick-pile.html |
| Seedling | `sprout` | Lorc | https://game-icons.net/1x1/lorc/sprout.html |
| Zone | `compass` | Lorc | https://game-icons.net/1x1/lorc/compass.html |

## Rejected alternatives

- `fire`, `small-fire` — flame reads best at 32 px.
- `stone-pile`, `ancient-ruins`, `castle-ruins` — stone-pile is a rock pile;
  ancient/castle ruins are too architectural. `brick-pile` reads as disaster debris.
- `seedling` (pot), `ground-sprout` (soil), `plant-seed` — `sprout` is cleanest.
- `treasure-map`, `direction-sign`, `flag-objective`, `position-marker` — all
  plausible for "zone", but `compass` preserves the meaning the HUD already had.
- Kenney (CC0) — no attribution burden, but weaker semantic coverage for
  "rubble"; mixing collections would break style consistency.

## Assets

Rasterised 512 → 256 px white-on-transparent so `Image.color` tinting works:

```
Assets/Art/UI/Icons/Icon_Fire.png      (256x256)
Assets/Art/UI/Icons/Icon_Rubble.png
Assets/Art/UI/Icons/Icon_Seedling.png
Assets/Art/UI/Icons/Icon_Zone.png
```

## Implemented

- `UiKit`: added `IconFire`, `IconRubble`, `IconSeedling`, `IconZone`, `IconTree`.
- `FantasticThemeBuilder`: loads the five PNGs via `LoadIcon(...)` so a rebuild
  of the (gitignored) kit asset reproduces the assignments.
- `HudTheme`: briefing target cards use the new roles; HUD counters now show
  rubble (`Counter icon 1`) and flame (`Counter icon 2`); achievement badges use
  quest/fire/tree/rubble/seedling/zone.
- `FromAshes.unity`: `TargetCard icon 0..3`, `Counter icon 1..2` and
  `AchIcon 0..5` repointed to the new sprites and saved.
- `THIRD_PARTY_NOTICES.md`: CC BY 3.0 attribution for all five icons.

## Extra: achievement badge icon

`PENJAGA HUTAN` needed a forest symbol; `pine-tree` (Lorc, CC BY 3.0) was added
as `Assets/Art/UI/Icons/Icon_Tree.png`. The other badges map to
quest/fire/rubble/seedling/zone.

## Verify

- `UiKit.MissingRoles()` empty; all five sprites load at 256x256.
- Scene image references confirmed by name -> sprite.
- EditMode suite: 128/128.

Preview renders: `~/.cache/tmp/opencode/icons/preview_before_after.png`,
`options_incard.png`, `zone_options.png`.
