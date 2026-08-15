# Synergy Matrix

This file records the intended `0.4.0` synergy/discovery surface so review and QA can track what is deliberate.

## State model

- `eligible`: current build/world state says the synergy or hidden choice could apply right now
- `discovered`: the character permanently unlocked visibility of a hidden choice
- `selected`: the character actually chose the hidden evolution in the mutation tree

Only `discovered` and `selected` belong in persistent save state. Tags, active synergies, and triad eligibility are derived at runtime.

## Late discovery policy

- Hidden rank-9 adaptations must be discovered before the character spends tier 3 on that mutation.
- If tier 3 is already spent, discovery progress stops for that hidden adaptation on that character.
- There is no retroactive unlock-selection or respec flow in `0.4.0`.

## Ownership rules

- A visible synergy may be shown by multiple mutation pages.
- One mutation class owns the actual gameplay effect for each synergy.
- Triads build on pair relationships deliberately; they are not intended to double-apply the same effect from multiple owners.

## Triads

| ID | Mutations / branches | Owner | Hidden | Trigger / eligibility | Discovery key | Test status |
| --- | --- | --- | --- | --- | --- | --- |
| `cathedral_organism` | `Carapace Evolution: Fortress` + `Living Crystal: Diamond Lattice` + `Brineborn: Saltglass Bloom` | all three display, mutation-local effects per owner | no | all three branches present; `Carapace Evolution` must also be functionally active via vanilla `Carapace` | n/a | compile verified |
| `breakwater_predator` | `Carapace Evolution: Hunter Shell` + `Living Crystal: Resonant Crystal` + `Brineborn: Scouring Estuary` | all three display, mutation-local effects per owner | no | all three branches present; `Carapace Evolution` must also be functionally active via vanilla `Carapace` | n/a | compile verified |
| `prism_estuary` | `Carapace Evolution: Adaptive Carapace` + `Living Crystal: Prismatic Matrix` + `Brineborn: Wellspring Flesh` | all three display, mutation-local effects per owner | no | all three branches present; `Carapace Evolution` must also be functionally active via vanilla `Carapace` | n/a | compile verified |
| `glass_kiln_bastion` | `Carapace Evolution: Fortress` + `Living Crystal: Diamond Lattice` + `Ash Metabolism: Furnace Skin` | all three display, mutation-local effects per owner | no | all three branches present; `Carapace Evolution` must also be functionally active via vanilla `Carapace` | n/a | pending runtime verify |
| `ember_pursuit_engine` | `Carapace Evolution: Hunter Shell` + `Living Crystal: Resonant Crystal` + `Ash Metabolism: Cinder Gut` | all three display, mutation-local effects per owner | no | all three branches present; `Carapace Evolution` must also be functionally active via vanilla `Carapace` | n/a | pending runtime verify |
| `mirage_exuvium` | `Carapace Evolution: Adaptive Carapace` + `Living Crystal: Prismatic Matrix` + `Ash Metabolism: Smoke Organ` | all three display, mutation-local effects per owner | no | all three branches present; `Carapace Evolution` must also be functionally active via vanilla `Carapace` | n/a | pending runtime verify |
| `salt_kiln_reliquary` | `Brineborn: Saltglass Bloom` + `Living Crystal: Diamond Lattice` + `Ash Metabolism: Furnace Skin` | all three display, mutation-local effects per owner | no | all three branches present | n/a | pending runtime verify |
| `steam_choir` | `Brineborn: Scouring Estuary` + `Living Crystal: Resonant Crystal` + `Ash Metabolism: Smoke Organ` | all three display, mutation-local effects per owner | no | all three branches present | n/a | pending runtime verify |
| `ossuary_rampart` | `Carapace Evolution: Fortress` + `Walking Colony: Marrow Hive` + `Living Crystal: Diamond Lattice` | all three display, mutation-local effects per owner | no | all three branches present; `Carapace Evolution` must also be functionally active via vanilla `Carapace` | n/a | pending runtime verify |
| `drift_parliament` | `Carapace Evolution: Hunter Shell` + `Walking Colony: Surveyor Swarm` + `Brineborn: Scouring Estuary` | all three display, mutation-local effects per owner | no | all three branches present; `Carapace Evolution` must also be functionally active via vanilla `Carapace` | n/a | pending runtime verify |

## Hidden adaptations

| ID | Mutation | Relevant branch | Owner | Hidden | Trigger / eligibility | Discovery key | Test status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `fractured_choir` | `Living Crystal` | `Resonant Crystal -> Choral Spines` | `MutationMeddley_LivingCrystal` | yes | sustained high cadence while also carrying `Heightened Hearing`; appears at rank 9 after discovery | `lc_hidden_choir` | compile verified |
| `heat_sink_choir` | `Living Crystal` | `Diamond Lattice -> Faceted Bulwark` | `MutationMeddley_LivingCrystal` | yes | repeated thermal-shock play while also carrying `Ash Metabolism` and `Flaming Ray`; appears at rank 9 after discovery | `lc_hidden_heatsink` | pending runtime verify |
| `salt_ghost` | `Brineborn` | `Scouring Estuary -> Brackish Jet` | `MutationMeddley_Brineborn` | yes | prolonged saline exposure while also carrying `Phasing`; appears at rank 9 after discovery | `brine_hidden_saltghost` | compile verified |
| `brine_reliquary` | `Brineborn` | `Saltglass Bloom -> Saltglass Bastion` | `MutationMeddley_Brineborn` | yes | repeated saline fortification while also carrying a crystalline profile; appears at rank 9 after discovery | `brine_hidden_reliquary` | pending runtime verify |
| `porcupine_redoubt` | `Carapace Evolution` | `Fortress -> Faceted Keep` | `MutationMeddley_CarapaceEvolution` | yes | repeated rooted shell turns while also carrying `Quills`; appears at rank 9 after discovery | `carapace_hidden_porcupine` | compile verified |
| `estuary_husk` | `Carapace Evolution` | `Adaptive Carapace -> Mire Sheath` | `MutationMeddley_CarapaceEvolution` | yes | prolonged amphibious or saline shell play with live vanilla `Carapace`; appears at rank 9 after discovery | `carapace_hidden_estuary` | pending runtime verify |
| `volcanic_memory` | `Ash Metabolism` | `Furnace Skin -> Kiln Plating` | `MutationMeddley_AshMetabolism` | yes | repeated high-heat exposure while carrying another structural mutation profile; appears at rank 9 after discovery | `ash_hidden_volcanic` | pending runtime verify |
| `wake_eater` | `Ash Metabolism` | `Cinder Gut -> Coal Maw` | `MutationMeddley_AshMetabolism` | yes | repeated hot-ground pursuit while using `Feast Ash`; appears at rank 9 after discovery | `ash_hidden_wake` | pending runtime verify |
| `burrowed_nursery` | `Walking Colony` | `Marrow Hive -> Bone Nursery` | `MutationMeddley_WalkingColony` | yes | repeated recovery-routing while also carrying `Burrowing Claws`; appears at rank 9 after discovery | `colony_hidden_burrowed` | pending runtime verify |
| `wake_trail` | `Walking Colony` | `Surveyor Swarm -> Latch Runners` | `MutationMeddley_WalkingColony` | yes | repeated high-cadence movement through hostile terrain; appears at rank 9 after discovery | `colony_hidden_waketrail` | pending runtime verify |
| `molt_parliament` | `Walking Colony` | `Graft Parliament -> Borrowed Hands` | `MutationMeddley_WalkingColony` | yes | sustained structural strain while using `Override Frame`; appears at rank 9 after discovery | `colony_hidden_molt` | pending runtime verify |

## Visible pair synergies

### Living Crystal

| ID | Other mutation | Relevant branch | Owner | Hidden | Trigger / eligibility | Discovery key | Test status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `electrical_generation` | `Electrical Generation` | strongest on `Diamond Lattice` / `Resonant Crystal` | `MutationMeddley_LivingCrystal` | no | both mutations present | n/a | compile verified |
| `light_manipulation` | `Light Manipulation` | `Prismatic Matrix` | `MutationMeddley_LivingCrystal` | no | both mutations present | n/a | compile verified |
| `flaming_ray` | `Flaming Ray` | strongest on `Diamond Lattice` / lit `Prismatic Matrix` | `MutationMeddley_LivingCrystal` | no | both mutations present | n/a | compile verified |
| `freezing_ray` | `Freezing Ray` | strongest on `Diamond Lattice` / dark `Prismatic Matrix` | `MutationMeddley_LivingCrystal` | no | both mutations present | n/a | compile verified |
| `phasing` | `Phasing` | `Prismatic Matrix` / `Resonant Crystal` | `MutationMeddley_LivingCrystal` | no | both mutations present | n/a | compile verified |
| `heightened_hearing` | `Heightened Hearing` | `Resonant Crystal` | `MutationMeddley_LivingCrystal` | no | both mutations present | n/a | compile verified |
| `brineborn_pair` | `Brineborn` | all three crystal identities reinterpret it differently | `MutationMeddley_LivingCrystal` | no | both mutations present | n/a | compile verified |
| `carapace_pair` | `Carapace Evolution` / `Carapace` | all three crystal identities reinterpret it differently | `MutationMeddley_LivingCrystal` | no | both mutations present | n/a | compile verified |

### Brineborn

| ID | Other mutation | Relevant branch | Owner | Hidden | Trigger / eligibility | Discovery key | Test status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `amphibious` | `Amphibious` | all, especially reserve sustain | `MutationMeddley_Brineborn` | no | both mutations present | n/a | compile verified |
| `regeneration` | `Regeneration` | `Wellspring Flesh` | `MutationMeddley_Brineborn` | no | both mutations present | n/a | compile verified |
| `photosynthetic_skin` | `Photosynthetic Skin` | lit saline play | `MutationMeddley_Brineborn` | no | both mutations present | n/a | compile verified |
| `multiple_legs` | `Multiple Legs` | `Scouring Estuary` | `MutationMeddley_Brineborn` | no | both mutations present | n/a | compile verified |
| `electrical_generation` | `Electrical Generation` | saline conductive tradeoff | `MutationMeddley_Brineborn` | no | both mutations present | n/a | compile verified |
| `burrowing_claws` | `Burrowing Claws` | dry-ground routing / reserve retention | `MutationMeddley_Brineborn` | no | both mutations present | n/a | compile verified |
| `living_crystal_pair` | `Living Crystal` | all three saline identities reinterpret it differently | `MutationMeddley_Brineborn` | no | both mutations present | n/a | compile verified |
| `carapace_pair` | `Carapace Evolution` | all three saline identities reinterpret it differently | `MutationMeddley_Brineborn` | no | both mutations present | n/a | compile verified |

### Carapace Evolution

| ID | Other mutation | Relevant branch | Owner | Hidden | Trigger / eligibility | Discovery key | Test status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `vanilla_carapace` | `Carapace` | all | `MutationMeddley_CarapaceEvolution` | no | vanilla `Carapace` present and companion mutation active | n/a | compile verified |
| `multiple_legs` | `Multiple Legs` | `Hunter Shell` | `MutationMeddley_CarapaceEvolution` | no | both mutations present and vanilla `Carapace` active | n/a | compile verified |
| `quills` | `Quills` | `Fortress` / `Hunter Shell` | `MutationMeddley_CarapaceEvolution` | no | both mutations present and vanilla `Carapace` active | n/a | compile verified |
| `regeneration` | `Regeneration` | `Fortress` | `MutationMeddley_CarapaceEvolution` | no | both mutations present and vanilla `Carapace` active | n/a | compile verified |
| `burrowing_claws` | `Burrowing Claws` | `Hunter Shell` / `Adaptive Carapace` | `MutationMeddley_CarapaceEvolution` | no | both mutations present and vanilla `Carapace` active | n/a | compile verified |
| `amphibious` | `Amphibious` | `Adaptive Carapace` | `MutationMeddley_CarapaceEvolution` | no | both mutations present and vanilla `Carapace` active | n/a | compile verified |
| `living_crystal_pair` | `Living Crystal` | all three shell identities reinterpret it differently | `MutationMeddley_CarapaceEvolution` | no | both mutations present and vanilla `Carapace` active | n/a | compile verified |
| `brineborn_pair` | `Brineborn` | all three shell identities reinterpret it differently | `MutationMeddley_CarapaceEvolution` | no | both mutations present and vanilla `Carapace` active | n/a | compile verified |
