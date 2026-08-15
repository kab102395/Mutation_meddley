# Mutation Taxonomy and Semantic Tags

## Purpose

Mutation Meddley should eventually support hidden evolutions, cross-mutation synergies, content discovery, and build identity without hard-coding every possible pair of mutations.

This document defines a semantic vocabulary for mutation design. It is a design contract first. Runtime representation can come later.

## Mutation class tags

These tags describe the broad gameplay role of a mutation.

- BASIC — understandable low-complexity mutation with room to deepen
- KEYSTONE — expensive, build-defining mutation
- ABERRANT — intentionally strange, unstable, or high-risk mutation
- SYMBIOTIC — another organism or colony participates in the mutation loop
- ENVIRONMENTAL — terrain or world conditions are a central resource
- BODY_PLAN — anatomy or equipment topology changes materially

A mutation may have more than one class tag when justified, but avoid tagging everything as everything.

## Biological and material tags

- BIOLOGICAL
- STRUCTURAL
- CHITINOUS
- CRYSTALLINE
- FUNGAL
- COLONIAL
- PARASITIC
- METABOLIC
- REGENERATIVE
- PREDATORY
- SENSORY
- GLANDULAR
- AQUATIC
- SALINE
- PHOTOSYNTHETIC

## Energy and elemental tags

- THERMAL
- CRYOGENIC
- ELECTRICAL
- ACIDIC
- TOXIC
- RADIANT
- SONIC
- RESONANT
- KINETIC
- MAGNETIC
- RADIATION

## Mental and reality tags

- PSIONIC
- TELEPATHIC
- EMOTIVE
- TEMPORAL
- DIMENSIONAL
- PROBABILISTIC
- PHASED
- MEMETIC

## Movement and tactical tags

- MOBILE
- BURROWING
- AERIAL
- AQUATIC_MOVEMENT
- PURSUIT
- RETALIATORY
- CONTROL
- AREA_DENIAL
- SUMMONING
- FORCED_MOVEMENT
- STEALTH

## Interaction tags

- LIQUID_INTERACTION
- GAS_INTERACTION
- TERRAIN_INTERACTION
- LIGHT_INTERACTION
- SOUND_INTERACTION
- CYBERNETIC_INTERACTION
- ROBOT_INTERACTION
- FOOD_INTERACTION
- DISEASE_INTERACTION
- FUNGAL_INTERACTION
- REPUTATION_INTERACTION
- BODY_PART_INTERACTION

## Tag design rules

### Tags should describe mechanics, not flavor alone

`CRYSTALLINE` is useful because it can participate in resonance, light, brittleness, conductivity, refraction, and material interactions.

A flavor-only tag such as `COOL_LOOKING` is not useful.

### Tags should be stable concepts

A tag should survive individual balance revisions. Avoid tags tied to one exact implementation detail.

### Tags should not replace explicit prerequisites when the relationship is unique

If an evolution only makes sense with one exact mutation, a direct prerequisite is clearer than inventing a tag used once.

### Tags should enable additive discovery

A future mutation should be able to gain immediate compatibility with old systems by participating in existing tags.

Example:

A new `Quartz Blood` mutation tagged `CRYSTALLINE`, `BIOLOGICAL`, and `RESONANT` could become eligible for older resonance interactions without those older mutations knowing its name.

## Example tag profiles

### Carapace

Base candidate tags:

- BIOLOGICAL
- STRUCTURAL
- CHITINOUS
- BODY_PART_INTERACTION

Branch-derived tags might include:

- Fortress: RETALIATORY
- Hunter Shell: PREDATORY, PURSUIT, MOBILE
- Adaptive Carapace: ENVIRONMENTAL, TERRAIN_INTERACTION

### Living Crystal

Base candidate tags:

- KEYSTONE
- CRYSTALLINE
- STRUCTURAL
- BIOLOGICAL

Potential branch tags:

- defensive lattice: KINETIC
- prismatic matrix: RADIANT, LIGHT_INTERACTION
- resonant lattice: RESONANT, SONIC

### Brineborn

Base candidate tags:

- ENVIRONMENTAL
- AQUATIC
- SALINE
- LIQUID_INTERACTION
- METABOLIC

Potential branch tags:

- sustain: REGENERATIVE
- crystallization: CONTROL, STRUCTURAL
- hostile-environment conversion: TERRAIN_INTERACTION, METABOLIC

## Hidden-evolution query language concept

Future implementation may support conditions conceptually similar to:

- has tag ELECTRICAL
- has any tag FUNGAL or SYMBIOTIC
- has tags CRYSTALLINE and RESONANT
- has tag SALINE and current terrain contains qualifying liquid
- has direct mutation X
- has world-state condition Y

This does not prescribe a serialization format yet.

## Anti-combinatorial-explosion rule

Do not create one branch for every possible pair of mutation tags.

Prefer broad interaction families:

- structural conductor
- fungal integration
- saline metabolism
- radiant refraction
- thermal storage
- kinetic resonance

Only create a named hidden evolution when the resulting play pattern is genuinely distinct.
