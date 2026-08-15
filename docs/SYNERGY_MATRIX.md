# Synergy Matrix

This file records the intended `0.6.3` synergy/discovery surface so review and QA can distinguish deliberate ecology from accidental stacking.

## State model

- `eligible`: the current build/world state says a synergy or hidden choice can apply now
- `discovered`: the character permanently unlocked visibility of a hidden choice
- `selected`: the character actually chose that hidden evolution

Only `discovered` and `selected` belong in persistent history. Semantic tags, pair activity, and triad eligibility are derived from the live build.

## Late discovery policy

- Hidden rank-9 adaptations must be discovered before the character spends tier 3 on that mutation.
- If tier 3 is already spent, discovery progress stops for that hidden adaptation on that character.
- There is no retroactive unlock-selection or respec flow in `0.6.3`.

## Ownership rules

- A named synergy may be shown on more than one mutation page.
- Pair effects remain mutation-local: each mutation class owns the contribution it applies to itself.
- Triads deliberately build on pair relationships; each participating mutation may contribute its own documented mutation-local effect slice.
- A dormant evolving mutation, especially `Carapace Evolution` without vanilla `Carapace`, must not contribute semantic tags, pair eligibility, triad eligibility, passive effects, discovery progress, or reactions.
- `0.6.3` is a static-freeze candidate. Every row below requires a fresh runtime pass on the tested `0.6.3` build even where an earlier release compiled the same relationship.

## Triads

| ID | Required branches | Runtime gate |
| --- | --- | --- |
| `cathedral_organism` | `Carapace Evolution: Fortress` + `Living Crystal: Diamond Lattice` + `Brineborn: Saltglass Bloom` | Carapace Evolution functionally active; stationary/saline conditions vary by owner |
| `breakwater_predator` | `Carapace Evolution: Hunter Shell` + `Living Crystal: Resonant Crystal` + `Brineborn: Scouring Estuary` | Carapace Evolution functionally active; wet/movement/impact conditions vary by owner |
| `prism_estuary` | `Carapace Evolution: Adaptive Carapace` + `Living Crystal: Prismatic Matrix` + `Brineborn: Wellspring Flesh` | Carapace Evolution functionally active; light/saline conditions vary by owner |
| `glass_kiln_bastion` | `Carapace Evolution: Fortress` + `Living Crystal: Diamond Lattice` + `Ash Metabolism: Furnace Skin` | Carapace Evolution functionally active; heat/stillness/contact conditions vary by owner |
| `ember_pursuit_engine` | `Carapace Evolution: Hunter Shell` + `Living Crystal: Resonant Crystal` + `Ash Metabolism: Cinder Gut` | Carapace Evolution functionally active; movement/impact conditions vary by owner |
| `mirage_exuvium` | `Carapace Evolution: Adaptive Carapace` + `Living Crystal: Prismatic Matrix` + `Ash Metabolism: Smoke Organ` | Carapace Evolution functionally active; light/smoke/wet conditions vary by owner |
| `salt_kiln_reliquary` | `Brineborn: Saltglass Bloom` + `Living Crystal: Diamond Lattice` + `Ash Metabolism: Furnace Skin` | saline/heat conditions vary by owner |
| `steam_choir` | `Brineborn: Scouring Estuary` + `Living Crystal: Resonant Crystal` + `Ash Metabolism: Smoke Organ` | wet/smoke conditions vary by owner |
| `ossuary_rampart` | `Carapace Evolution: Fortress` + `Walking Colony: Marrow Hive` + `Living Crystal: Diamond Lattice` | Carapace Evolution functionally active; rooted conditions vary by owner |
| `drift_parliament` | `Carapace Evolution: Hunter Shell` + `Walking Colony: Surveyor Swarm` + `Brineborn: Scouring Estuary` | Carapace Evolution functionally active; wet/movement conditions vary by owner |
| `undertow_furnace` | `Brineborn: Wellspring Flesh` + `Ash Metabolism: Cinder Gut` + `Walking Colony: Marrow Hive` | wet/recovery/heat conditions vary by owner |
| `salt_eclipse` | `Brineborn: Saltglass Bloom` + `Living Crystal: Prismatic Matrix` + `Carapace Evolution: Adaptive Carapace` | Carapace Evolution functionally active; dim/saline conditions vary by owner |
| `bone_kiln_parliament` | `Ash Metabolism: Furnace Skin` + `Walking Colony: Graft Parliament` + `Carapace Evolution: Fortress` | Carapace Evolution functionally active; structural/heat conditions vary by owner |
| `resonant_undertow` | `Living Crystal: Resonant Crystal` + `Brineborn: Wellspring Flesh` + `Walking Colony: Surveyor Swarm` | wet/movement/cadence conditions vary by owner |
| `smoke_reef` | `Ash Metabolism: Smoke Organ` + `Brineborn: Saltglass Bloom` + `Living Crystal: Prismatic Matrix` | smoke/saline conditions vary by owner |
| `chorus_husk` | `Living Crystal: Resonant Crystal` + `Walking Colony: Graft Parliament` + `Carapace Evolution: Adaptive Carapace` | Carapace Evolution functionally active; mutation-local conditions vary by owner |
| `whitewater_ossuary` | `Brineborn: Scouring Estuary` + `Walking Colony: Marrow Hive` + `Carapace Evolution: Fortress` | Carapace Evolution functionally active; wet conditions vary by owner |
| `blackglass_pursuit` | `Ash Metabolism: Cinder Gut` + `Living Crystal: Diamond Lattice` + `Walking Colony: Surveyor Swarm` | movement conditions vary by owner |

## Hidden rank-9 adaptations

All hidden choices below must be discovered while tier 3 is still unspent.

| ID | Mutation / prerequisite path | Discovery condition |
| --- | --- | --- |
| `fractured_choir` | Living Crystal: `Resonant Crystal -> Choral Spines` | sustained high cadence with `Heightened Hearing` |
| `heat_sink_choir` | Living Crystal: `Diamond Lattice -> Faceted Bulwark` | repeated hot-cell play with `Ash Metabolism` and `Flaming Ray` |
| `solar_wake` | Living Crystal: `Prismatic Matrix -> Sunlens Array` | repeated lit-space play with `Light Manipulation` |
| `null_prism` | Living Crystal: `Prismatic Matrix -> Shade Reflector` | repeated dim-space play with `Phasing` |
| `salt_ghost` | Brineborn: `Scouring Estuary -> Brackish Jet` | prolonged saline exposure with `Phasing` |
| `brine_reliquary` | Brineborn: `Saltglass Bloom -> Saltglass Bastion` | stationary saline `Shell Up` play with a crystalline Living Crystal profile |
| `undertow_heart` | Brineborn: `Wellspring Flesh -> Tidal Marrows` | actual wounded Tidal Marrows reserve-spend recovery with `Regeneration` in `Draw Brine` |
| `abyssal_brine` | Brineborn: `Wellspring Flesh -> Cool Sump` | wet or saline `Cool Reserve` play with `Freezing Ray` |
| `porcupine_redoubt` | Carapace Evolution: `Fortress -> Faceted Keep` | repeated rooted live-Carapace turns with `Quills` |
| `estuary_husk` | Carapace Evolution: `Adaptive Carapace -> Mire Sheath` | prolonged live-Carapace amphibious or saline play |
| `skitter_bulwark` | Carapace Evolution: `Hunter Shell -> Ravager Joints` | repeated live-Carapace movement with `Multiple Legs` |
| `hookstorm_frame` | Carapace Evolution: `Hunter Shell -> Spur Lattice` | repeated live-Carapace melee engagement with `Quills` |
| `volcanic_memory` | Ash Metabolism: `Furnace Skin -> Kiln Plating` | repeated hot-cell play with another live `STRUCTURAL` mutation profile |
| `wake_eater` | Ash Metabolism: `Cinder Gut -> Coal Maw` | repeated hot movement while using `Feast Ash` |
| `cenotaph_haze` | Ash Metabolism: `Smoke Organ -> Ash Veil` | repeated smoky `Veil Smoke` play with `Phasing` |
| `cinder_jet` | Ash Metabolism: `Smoke Organ -> Chimney Lungs` | repeated smoky movement with `Multiple Legs` or another live `MOBILE` mutation profile |
| `burrowed_nursery` | Walking Colony: `Marrow Hive -> Bone Nursery` | stationary pressure >= 2 with `Burrowing Claws` |
| `wake_trail` | Walking Colony: `Surveyor Swarm -> Latch Runners` | movement streak >= 3 through hostile traversal |
| `molt_parliament` | Walking Colony: `Graft Parliament -> Borrowed Hands` | `Override Frame` with another live `STRUCTURAL` mutation profile |
| `choir_of_tendons` | Walking Colony: `Graft Parliament -> Nerve Delegation` | stationary pressure >= 2 with `Heightened Hearing` or `Living Crystal: Resonant Crystal` |

## Visible pair-synergy surface

These are the curated pair relationships the current code recognizes. Branch restrictions still apply inside each mutation class.

### Living Crystal

- `Electrical Generation`
- `Light Manipulation`
- `Flaming Ray`
- `Freezing Ray`
- `Phasing`
- `Heightened Hearing`
- `Ash Metabolism`
- `Walking Colony`
- `Brineborn`
- functionally active `Carapace Evolution`

### Brineborn

- `Amphibious`
- `Regeneration`
- `Photosynthetic Skin`
- `Multiple Legs`
- `Electrical Generation`
- `Burrowing Claws`
- `Ash Metabolism`
- `Walking Colony`
- `Living Crystal`
- functionally active `Carapace Evolution`

### Carapace Evolution

All entries require live vanilla `Carapace` because the companion mutation is otherwise dormant.

- vanilla `Carapace`
- `Multiple Legs`
- `Quills`
- `Regeneration`
- `Burrowing Claws`
- `Amphibious`
- `Ash Metabolism`
- `Walking Colony`
- `Living Crystal`
- `Brineborn`

### Ash Metabolism

- `Flaming Ray`
- `Freezing Ray`
- `Light Manipulation`
- `Electrical Generation`
- `Photosynthetic Skin`
- `Phasing`
- `Multiple Legs`
- `Amphibious`
- `Walking Colony`
- `Living Crystal`
- `Brineborn`
- functionally active `Carapace Evolution`

### Walking Colony

- `Regeneration`
- `Multiple Legs`
- `Quills`
- `Burrowing Claws`
- `Amphibious`
- `Heightened Hearing`
- `Phasing`
- vanilla `Carapace`
- `Ash Metabolism`
- `Living Crystal`
- `Brineborn`
- functionally active `Carapace Evolution`

## v0.6.3 QA contract

For the static-freeze playtest:

1. verify every named pair appears only on an intended branch
2. verify dormant `Carapace Evolution` contributes nothing to tags, pair eligibility, triads, discoveries, passives, or reactions
3. verify all eighteen triads require exactly their intended branch combination
4. verify all twenty hidden adaptations require their discovery condition and stay unavailable after tier 3 is spent
5. save/reload and confirm hidden discovery persists while active pair/triad state recomputes from the live build
6. inspect actual stat/verb output rather than assuming a named synergy's flavor text proves its effect fired

Runtime status for the entire matrix is **pending a fresh `0.6.3` compile and behavior pass**. Do not carry forward an older release's compile label as evidence for the current static-freeze commit.
