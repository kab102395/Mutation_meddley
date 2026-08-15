# Mutation Meddley v0.7 Continuous Progression Plan

## Goal

Mutation rank must matter continuously, not only at the rank-3/rank-6/rank-9 evolution gates.

Caves of Qud treats mutation level as a power level: mutation points raise an existing mutation's level, and ordinary mutations generally use that level to improve one or more quantitative effects. Mutation Meddley keeps its qualitative evolution milestones, but now follows the same player-facing contract:

> Every rank makes the mutation stronger. Ranks 3, 6, and 9 additionally change what the mutation can do.

This pass does **not** replace the verb-first evolution model. It makes the baseline and every previously acquired layer mature as the player continues investing mutation points.

## Progression cadence

Normal mutation-point investment is expected to matter through rank 10.

| Rank | Continuous progression event | Evolution event |
| --- | --- | --- |
| 1 | baseline mutation loop | none |
| 2 | first maturity passive | none |
| 3 | retained maturity | choose identity |
| 4 | first verb-output increase | none |
| 5 | second maturity passive | none |
| 6 | retained progression | choose specialization |
| 7 | second verb-output increase | none |
| 8 | third maturity passive | none |
| 9 | retained progression | choose capstone / discovered unusual adaptation |
| 10 | third verb-output increase | none |

Physical rapid advancement can push effective mutation level above 10. The formulas continue beyond 10 instead of hard-clamping at 10, but balance testing is centered on the normal rank-1 through rank-10 mutation-point curve.

## Shared formulas

### Maturity tier

`maturity = floor((Level + 1) / 3)` with rank 1 treated as zero maturity.

The visible increases therefore occur at levels 2, 5, 8, 11, ...

Maturity raises a branch-appropriate persistent passive. It is deliberately modest: it supports the identity instead of replacing equipment, attributes, or the actual verb loop.

### Verb growth

`verbGrowth = floor((Level - 1) / 3)`.

The increases occur at levels 4, 7, 10, 13, ...

Every Mutation Meddley call routed through the shared healing helper or synthetic bonus-damage helper gains this amount. This means a verb learned at rank 3 remains relevant at ranks 6 and 9 instead of remaining frozen at its original magnitude.

Examples:

- a base `1`-point shell retaliation deals 1 at ranks 1-3, 2 at ranks 4-6, 3 at ranks 7-9, and 4 at rank 10
- a base `1`-point recovery reaction follows the same maturity curve
- a stronger base `2`-point effect becomes 2 / 3 / 4 / 5 over the same bands

The branch still controls *when* and *why* the verb fires. Level only matures its magnitude.

## Passive identity scaling

The continuous passive layer is mutation-local and branch-aware. It is applied after the concrete mutation refresh, so it stacks with—but does not replace—the branch, stance, specialization, capstone, synergy, and triad profile.

### Carapace Evolution

Requires live vanilla Carapace as before. Dormant Carapace Evolution receives no progression bonuses.

- unevolved / Fortress: maturity -> Toughness; verb-growth tier -> AV
- Hunter Shell: maturity -> Agility; verb-growth tier -> Quickness
- Adaptive Carapace: maturity -> Willpower; verb-growth tier -> heat/cold resistance

Result: the original shell identity continues to mature after rank 3, and later rank-6/rank-9 choices sit on top of the stronger shell rather than replacing it.

### Living Crystal

- unevolved / Diamond Lattice: maturity -> Toughness; verb-growth tier -> AV
- Prismatic Matrix: maturity -> Agility; verb-growth tier -> DV
- Resonant Crystal: maturity -> Ego; verb-growth tier -> Quickness

Stress, Dawn/Dusk, and Release mechanics keep their existing rules, while their healing/contact/discharge consequences scale through the shared verb helper.

### Brineborn

- unevolved / Wellspring Flesh: maturity -> Toughness; verb-growth tier -> heat/cold resistance
- Saltglass Bloom: maturity -> Toughness; verb-growth tier -> AV
- Scouring Estuary: maturity -> Agility; verb-growth tier -> Quickness

Reserve capacity remains governed by the saline ecology and its explicit synergy rules. Continuous level progression improves what the stored reserve and its derived states accomplish rather than inflating every resource cap automatically.

### Ash Metabolism

- unevolved / Furnace Skin: maturity -> Toughness; verb-growth tier -> AV and heat resistance
- Cinder Gut: maturity -> Agility; verb-growth tier -> Quickness and heat resistance
- Smoke Organ: maturity -> Ego; verb-growth tier -> DV and heat resistance

Kiln, Rush, and Haze keep their branch-specific generation/spend rules. Existing retaliation, contact, and recovery outcomes scale through the shared verb helper.

### Walking Colony

- unevolved / Marrow Hive: maturity -> Toughness; verb-growth tier -> AV
- Surveyor Swarm: maturity -> Agility; verb-growth tier -> Quickness
- Graft Parliament: maturity -> Intelligence; verb-growth tier -> Willpower

Pressure, Stitch, Scout, and Parliament remain distinct resources. Rank increases make the body carrying those resources stronger and mature their direct healing/damage consequences.

## Baseline scaling

Rank 1 remains immediately functional.

At rank 2, every flagship receives a visible maturity increase even though no evolution is yet available. The baseline Carapace Brace and Living Crystal Stress pools also receive one extra pre-branch capacity at rank 2 so their introductory loops visibly mature before the rank-3 identity choice.

Brineborn, Ash Metabolism, and Walking Colony already use larger persistent ecology pools at baseline; those caps remain deliberately branch/resource-defined while their passives and verb output mature with level.

## Preservation rules

1. Rank-3 identity mechanics never stop scaling when rank 6 is selected.
2. Rank-6 mechanics never replace rank-3 progression; they modify the stronger existing loop.
3. Rank-9 capstones/unusual adaptations add another rule or reaction on top of the mature rank-3/rank-6 system.
4. Stance choice remains tactical and does not alter the player's mutation level.
5. Synergies and triads remain additive ecology; they do not replace continuous mutation-level scaling.
6. Dormant evolving mutations receive no continuous passive contribution.
7. No new serialized public field is required. Progression is derived from the live BaseMutation `Level`, so existing saves do not need state migration.
8. Resource keys and evolution-state envelope format remain unchanged.

## Player-facing information

Each mutation's usage/mechanics section must state:

- current mutation rank
- current maturity tier
- current shared verb-growth bonus
- next rank and what kind of progression event it provides through rank 10

The player should be able to inspect the mutation before spending a point and understand why the next rank matters.

## Static acceptance tests

For each of the five gameplay mutations:

1. rank 1 has the existing baseline loop
2. rank 2 shows a new continuous passive versus rank 1
3. rank 3 unlocks its identity without losing the rank-2 maturity passive
4. rank 4 increases a shared heal/bonus-damage verb by one where that branch uses one, and increases the branch's verb-tier support passive
5. rank 5 increases the branch maturity passive
6. rank 6 specialization retains all previous scaling
7. rank 7 increases verb output/support again
8. rank 8 increases maturity again
9. rank 9 capstone retains all previous scaling
10. rank 10 increases verb output/support again
11. retuning stance does not reset or duplicate progression bonuses
12. save/reload preserves Level and therefore recomputes the same progression profile without new serialized state
13. dormant Carapace Evolution receives no rank-scaling stats or reactions
14. synthetic bonus-damage recursion protection still works at increased damage values
15. full-HP healing guards still prevent resource loss solely to attempt impossible healing

## Balance acceptance tests

The first balance pass should compare the same branch at ranks 3, 4, 5, 6, 7, 8, 9, and 10 against representative enemies.

Record:

- passive AV/DV/attributes/resistances
- resource generation cadence
- stored-state spend frequency
- direct mutation bonus damage per proc
- mutation healing per proc
- fight duration and incoming damage
- whether any rank increase feels invisible
- whether level scaling makes a branch dominate alternatives without requiring its intended setup

If a rank feels invisible, fix that rank's progression axis. If scaling is excessive, reduce the formula rather than removing continuous progression.

## Release gate

This work is a `0.7.0` gameplay progression change. Do not call it runtime-ready until:

- repository preflight passes
- Qud compiles the exact `0.7.0` commit
- ranks 1/2/3/4/5 are smoke-tested on at least one mutation
- one offensive branch verifies rank-4 bonus-damage growth
- one sustain branch verifies rank-4 healing growth
- one rank-10 character verifies the full progression summary and final normal mutation-point band
- save/reload and Carapace dormancy still pass
