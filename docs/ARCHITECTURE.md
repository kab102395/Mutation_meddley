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
- hooks that concrete mutation classes can override when a branch is chosen

Concrete mutations own gameplay behavior.

The framework deliberately does not know about Carapace, Flaming Ray, Multiple Arms, or any other specific mutation.

## Evolution graph

Each evolution choice has:

- `Id` - stable internal key
- `Name` - player-facing name
- `Description` - player-facing mechanical/design description
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

The proof-of-concept base class stores selected evolution IDs in the stable serialized string field `MutationMeddley_EvolutionState`.

This is intentionally conservative. A string payload can be extended later without immediately changing the serialized field layout of every mutation.

Do not change the type of this field. If the framework eventually needs richer save state, introduce an explicit migration strategy first.

## UI strategy

Version 0.1.0 uses a normal activated ability plus `Popup.AskString` for branch selection.

This is not intended to be the final UI. It is intentionally simple so the framework can be tested without patching Qud's mutation screen.

A future mutation-tree UI should be treated as a separate layer over the same underlying evolution state and rules.

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

### Stage 2 - one real mutation

Implement a real, self-contained mutation using the framework and give each branch actual gameplay behavior.

### Stage 3 - vanilla evolution adapter strategy

Determine the least invasive way to add evolution behavior to selected vanilla mutations. Do not assume this requires Harmony; inspect current classes/events first.

### Stage 4 - content expansion

Add physical, mental, aberrant, and synergy-heavy mutation packs once the framework has survived real playtesting.
