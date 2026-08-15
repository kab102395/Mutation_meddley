# Architecture

## Runtime shape

Mutation Meddley uses normal Caves of Qud mod loading:

- `manifest.json` identifies the mod.
- `Mutations.xml` registers mutations.
- `.cs` files under `Code/` are compiled by Qud at runtime.
- no precompiled DLL is required for the initial framework.

## Core abstraction

`MutationMeddley_EvolvingMutationBase` derives from Qud's `BaseMutation` and owns only framework concerns:

- selected evolution persistence
- branch prerequisites
- one-choice-per-tier enforcement
- evolution availability checks
- an activated ability for choosing an available evolution
- a controller-friendly option-list picker
- hooks that concrete mutation classes can override when a branch is chosen
- hooks that concrete mutations can use to refresh branch-driven effects when evolution state or level changes

Concrete mutations own gameplay behavior.

The framework deliberately does not know about Carapace, Flaming Ray, Multiple Arms, or any other specific mutation.

## Evolution graph

Each evolution choice has:

- `Id` - stable internal key
- `Name` - player-facing name
- `Description` - player-facing mechanical/design description
- `DetailText` - optional richer picker text
- `RequiredLevel` - mutation rank required
- `Tier` - mutually exclusive choice tier
- `PrerequisiteId` - optional prior evolution required

This is enough to represent a tree without hard-coding a single path.

Example:

```text
rank 3
  Bulwark
    rank 6
      Layered Plates
        rank 9
          Living Fortress
      Reactive Plates
        rank 9
          Counter-Carapace

  Predator
    ...
```

## Persistence

The framework stores mutation state in the stable serialized string field `MutationMeddley_EvolutionState`.

Version 0.5.0 uses that field as a small state envelope:

- selected evolution IDs remain the primary state
- mutation-local metadata such as stance, cadence, or saline reserve are encoded into the same payload
- old semicolon-only saves remain readable and are treated as the pre-envelope form
- new writes stamp a lightweight envelope version so later migrations can distinguish state formats safely

This keeps the save contract extensible without multiplying public serialized fields across every adaptive mutation.

Do not change the type of this field. If the framework eventually needs richer save state, introduce an explicit migration strategy first.

For synergy/discovery content, keep the boundary explicit:

- semantic tags, active pair synergies, and current triad eligibility are derived runtime state
- hidden discovery flags and hidden evolution selections are persistent history
- treat `eligible`, `discovered`, and `selected` as different states rather than collapsing them into one boolean
- hidden rank-gated discoveries must be found before that tier is spent on the current character; discovery is not retroactive respec state

## UI strategy

Version 0.5.0 keeps `Popup.ShowOptionList` for both path selection and mutation-specific stance changes.

This keeps the current UI keyboard, mouse, controller, and handheld friendly without committing yet to a custom full-screen mutation tree.

A future mutation-tree UI should remain a separate presentation layer over the same evolution state and rules.

## Content shape

Version 0.5.0 has four connected layers of content:

- `Evolution Seed [DEV]` remains the regression harness for framework behavior
- `Living Crystal`, `Brineborn`, `Ash Metabolism`, and `Walking Colony` are Mutation Meddley-owned flagship mutations
- `Carapace Evolution` is the first narrow vanilla adapter
- a tag-driven synergy/discovery layer that lets those mutations react to curated vanilla mutation families and to each other through visible pair synergies, branch-locked hidden rank-9 adaptations, and a broader named triad ecology

`Carapace Evolution` intentionally does not replace the base-game `Carapace` class. It is a companion mutation designed to pair with vanilla `Carapace`, and it remains dormant until vanilla `Carapace` is actually present. Dormancy suppresses shell augmentation and stance retuning without deleting the saved evolution path.

The first runtime semantic layer is intentionally small:

- mutation-local gameplay still lives in mutation classes
- the shared framework only provides semantic tags, mutation-presence queries, active-synergy enumeration, hidden-choice gating, and level-text reporting
- exact pair logic is still allowed where a relationship is uniquely specific
- one mutation class owns each gameplay effect even if multiple mutation pages report the same synergy
- hidden discovery remains mutation-local and rank-gated; no retroactive rank-9 respec flow exists in `0.5.0`

## Compatibility strategy

Prefer, in order:

1. standard Qud XML/data definitions
2. `BaseMutation`/parts/events/activated abilities
3. narrowly scoped helper code
4. Harmony only when an otherwise inaccessible game behavior must be changed

Project-owned identifiers should use the `MutationMeddley_` prefix.

## Planned stages

### Stage 1 - framework proof of concept

Validate C# compilation, evolution selection, tier locking, prerequisites, and save/load behavior with `Evolution Seed [DEV]`.

### Stage 2 - flagship content pack

Implement real, self-contained mutations using the framework and give each branch actual gameplay behavior.

### Stage 3 - deeper vanilla adapter strategy

Expand from the `Carapace Evolution` companion pattern only after current game assemblies and in-game behavior justify a more direct integration.

### Stage 4 - content expansion

Add physical, mental, aberrant, and synergy-heavy mutation packs once the framework and first adapter have survived real playtesting.
