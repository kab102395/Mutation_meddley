# Mutation Meddley: Deep Qud Design Bible

## Purpose

Mutation Meddley should not become a catalog of stat upgrades. Its identity is systemic mutation design: abilities that change how the player reads terrain, enemies, inventory, body plan, action economy, temperature, liquids, gases, light, sound, phase, disease, food, cybernetics, and other Caves of Qud systems.

The core standard is simple:

> A strong Mutation Meddley branch changes decisions, not only numbers.

If two branches produce the same decisions with different bonuses, they are not meaningfully different branches.

## Design pillars

### 1. Verbs over modifiers

Prefer new actions, reactions, constraints, targeting rules, terrain interactions, timing windows, resource loops, body-plan changes, and emergent combinations.

Weak branch:

- +20 heat resistance

Stronger branch:

- excess heat is stored in the shell and may be discharged through contact attacks

The second effect may still include resistance, but the resistance serves a new gameplay loop.

### 2. Every strong option should imply a weakness, opportunity cost, or changed obligation

A branch does not need a literal penalty line, but it should create a meaningful tradeoff.

Examples:

- heavy armor identity trades mobility or equipment freedom for stability
- pursuit identity sacrifices some passive durability for movement and close-range pressure
- environmental conversion is strongest in favorable terrain and weaker away from it
- high-output temporal power creates delayed action debt

The player should be able to explain what a build is bad at.

### 3. Qud systems are the content surface

Mutation logic should use existing simulation systems whenever practical instead of recreating generic RPG mechanics beside them.

High-value systems include:

- temperature and heat transfer
- liquids, pools, salinity, and fluid contact
- gas and atmosphere
- electricity and charge
- light and darkness
- sound and vibration
- phase and dimensional state
- movement and forced movement
- body parts and equipment slots
- armor and penetration
- food and cooking
- disease, infection, and fungal states
- cybernetics and robots
- reputation and social context
- terrain and environmental hazards
- cooldowns and action economy
- mental effects and psychic interactions

A mutation should feel like it belongs in Qud because it talks to Qud's world model.

### 4. Branches are identities, not upgrade columns

A rank-3 choice should define a tactical identity. Rank-6 choices deepen that identity. Rank-9 capstones should complete the fantasy without erasing the earlier tradeoffs.

Recommended shape:

- rank 3: identity
- rank 6: specialization within that identity
- rank 9: capstone that reinforces the specialization
- rank 10: optional automatic polish, not necessarily another choice

A rank-9 capstone should not simply overwrite the earlier branch with a universally superior behavior.

### 5. Discovery matters

Not every evolution should be visible from character creation or obvious from a build planner.

Future hidden evolutions may depend on:

- possessing mutation tags
- repeated environmental exposure
- body-plan state
- disease or fungal infection
- cybernetics
- specific terrain experience
- unusual damage history
- temporal or dimensional exposure
- long-term use of a mutation in a particular way

The desired player reaction is:

> I have never seen this option before. What did I do differently?

Hidden evolutions must still be deterministic enough to test and document internally.

### 6. Synergy should emerge from semantics, not a giant pairwise switch statement

Mutations should expose semantic tags. Evolutions may query those tags or world state rather than hard-coding every mutation combination.

Example concepts:

- STRUCTURAL + ELECTRICAL may enable piezoelectric behaviors
- CRYSTALLINE + LIGHT may enable refraction or beam interactions
- AQUATIC + METABOLIC may enable saline sustain
- FUNGAL + SYMBIOTIC may unlock mycelial integration

Pair-specific interactions are still allowed when the idea is exceptional, but the default should be tag-driven compatibility.

### 7. No single solved path

Each branch line should have at least one situation in which another line is clearly preferable.

Review questions:

- What character build wants this branch?
- What character build does not want it?
- What terrain or enemy changes its value?
- What equipment does it encourage or discourage?
- What other mutations naturally combine with it?
- What resource or timing constraint keeps it from dominating every encounter?

If those questions do not produce distinct answers, the branch is probably too shallow.

## Gameplay mutation classes

These are design classes, not necessarily C# inheritance types.

### Basic mutations

Low-cost or moderate-cost mutations that provide clear utility while still allowing branching identity.

Examples:

- glands
- sensory organs
- musculature
- skin adaptations
- claws, teeth, joints, gills

Basic mutations should be easy to understand at rank 1 and gain depth later.

### Keystone mutations

High-cost, build-defining mutations that can anchor an entire run.

Examples:

- Living Crystal
- Colonial Organism
- Wrong Geometry
- Walking Colony

A keystone mutation should materially change equipment, routing, tactics, or resource priorities.

### Aberrant mutations

Rare, dangerous, or conceptually unstable mutations that may have unusual drawbacks or rules.

Examples:

- Borrowed Tomorrow
- Wrong Geometry
- Probability Scarring
- Echo Body

These are where Mutation Meddley can become especially strange, but they still need legible rules and testable state.

### Symbiotic mutations

A second organism, colony, microbiome, fungus-like intelligence, or parasitic partner becomes part of the build.

Examples:

- Marrow Colony
- Neural Lichen
- Gut Oracle
- Choir Parasite

Symbiotic mutations should create reciprocal behavior rather than functioning as a renamed buff.

### Environmental mutations

Terrain, liquids, temperature, gases, light, or ecological conditions become part of the mutation's resource loop.

Examples:

- Brineborn
- Dune Lung
- Deep Root
- Ash Metabolism

Environmental mutations should reward routing and positioning without becoming unusable outside their favored biome.

### Body-plan mutations

These alter anatomy, equipment topology, locomotion, or the meaning of body parts.

Examples:

- Radial Symmetry
- Serpentine Lower Body
- Distributed Skull
- Walking Colony

Body-plan mutations should be treated as high compatibility risk and tested extensively with equipment and save/load behavior.

## Capstone standard

A capstone should do at least one of the following:

- introduce a new verb
- transform a resource loop
- change a targeting or movement rule
- unlock a conditional interaction
- alter body-plan behavior
- turn a prior weakness into a situational strength without deleting all tradeoffs

Pure stat capstones are allowed only when the stat change enables a qualitatively different threshold or tactic.

## Failure modes to reject

Reject or redesign branches that are primarily:

- flat percentage damage increases
- flat resistance stacks without a mechanic
- cooldown reductions with no change in use pattern
- three branches where one is mathematically dominant in almost all situations
- effects that duplicate an existing Qud mutation with a different name
- hidden mechanics the player cannot reasonably infer or inspect
- mechanics that require typing during normal gameplay
- effects that bypass Qud systems instead of participating in them without a strong reason

## Content review gate

Before a mutation is promoted from concept to implementation, its design should state:

1. base mutation fantasy
2. mutation cost and intended power class
3. base gameplay loop
4. rank-3 identities
5. rank-6 specializations
6. rank-9 capstones
7. meaningful tradeoffs
8. relevant semantic tags
9. likely synergies
10. systems touched
11. save-state requirements
12. compatibility risks
13. controller interaction requirements
14. at least one scenario where each major branch is preferable
