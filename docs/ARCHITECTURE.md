# Architecture

## Runtime shape

Mutation Meddley uses normal Caves of Qud mod loading:

- `manifest.json` identifies the mod.
- `Mutations.xml` registers mutations.
- `.cs` files under `Code/` are compiled by Qud at runtime.
- no precompiled DLL is required for the initial framework.

## Core abstraction

`MutationMeddley_EvolvingMutationBase` derives from Qud's `BaseMutation` and owns framework concerns:

- selected evolution persistence
- branch prerequisites
- one-choice-per-tier enforcement
- evolution availability checks
- an activated ability for choosing an available evolution
- a controller-friendly option-list picker
- hooks that concrete mutation classes can override when a branch is chosen
- semantic tags and live mutation/synergy queries
- shared explicit bonus-damage dispatch and recursion protection
- conservative classification of outgoing damage as adjacent melee contact or non-contact for stored-state spends

`MutationMeddley_AdaptiveMutationBase` adds adaptive-mutation concerns:

- mutation-specific stance selection
- mutation-local additive stat shifting
- live mechanics/current-bonus reporting
- a nonserialized moved-this-turn signal used by shared completion logic
- restrained rank-1/rank-2 baseline behavior for the current owned roster
- narrowly scoped completion hooks for stance or capstone behavior that was otherwise mechanically inert

Concrete mutation classes remain the owners of their primary resources, branch identities, discoveries, and core event behavior. The shared v0.6.3 completion layer exists to enforce cross-roster invariants such as "paid mutations work before rank 3" and "a contact meter is not spent by non-contact damage"; it is not intended to become a second giant content engine.

## Evolution graph

Each evolution choice has:

- `Id` - stable internal key
- `Name` - player-facing name
- `Description` - player-facing mechanical/design description
- `DetailText` - optional richer picker text
- `RequiredLevel` - mutation rank required
- `Tier` - mutually exclusive choice tier
- `PrerequisiteId` - optional prior evolution required

Rank 3 remains the first identity split. Rank 6 specializes that identity. Rank 9 supplies a capstone or discovered unusual adaptation.

The v0.6.3 content contract is stronger than the graph alone:

- ranks 1-2 provide a modest baseline passive and reaction/resource loop
- rank 3 creates the primary gameplay verb
- both stances must alter reachable behavior
- rank 6 changes the branch's state generation, spending, timing, or consequence
- rank 9 changes that loop again or adds a visible event reaction

## Persistence

The framework stores mutation state in the stable serialized string field `MutationMeddley_EvolutionState`.

Version 0.6.3 retains the same envelope contract used by 0.6.2:

- selected evolution IDs remain the primary state
- mutation-local metadata such as stance, cadence, reserve, embers, pressure, and short-lived branch meters are encoded into the same payload
- old semicolon-only saves remain readable and are treated as the pre-envelope form
- new writes stamp a lightweight envelope version so later migrations can distinguish state formats safely

No public serialized field was added for the v0.6.3 static-freeze pass. The newly introduced event-contact context and shared moved-this-turn flag are explicitly `[NonSerialized]` and are reconstructed naturally from later events.

For synergy/discovery content, keep the boundary explicit:

- semantic tags, active pair synergies, and current triad eligibility are derived runtime state
- hidden discovery flags and hidden evolution selections are persistent history
- treat `eligible`, `discovered`, and `selected` as different states rather than collapsing them into one boolean
- hidden rank-gated discoveries must be found before that tier is spent on the current character; discovery is not retroactive respec state

## Contact-qualified offensive verbs

The owned mutations listen to Qud's broad damage events because those are supported mutation/part hooks, but their fiction frequently describes a close-contact action such as shell slam, crystal edge, estuary wake, cinder rush, or pursuit-line strike.

Version 0.6.3 therefore adds a conservative shared contact classifier instead of guessing a weapon-source API.

When an outgoing damage event asks for its target, the framework classifies it as contact only when:

- the defender resolves
- attacker and defender occupy adjacent cells
- the mutation bearer is currently engaged in melee

For a rejected outgoing event:

- stored-state consumption is refused
- synthetic Mutation Meddley bonus damage is refused
- DEV tracing reports a contact-gate rejection

The classification is transient and is reset after each event. This is intentionally a static-safe approximation. The adjacent-ranged-while-engaged edge case is part of the runtime test matrix; if it misclassifies, fix it only after a verified current-Qud weapon/source signal is identified.

## Bonus-damage dispatch

Synthetic mutation follow-up damage uses the shared path in `MutationMeddley_EvolvingMutationBase`:

1. resolve the target/source explicitly from the event
2. construct a `Damage` object
3. construct a `TakeDamage` event
4. set `Damage`, `Owner`, `Attacker`, and `Message`
5. fire that event on the target
6. separately record event dispatch, event continuation, and observed HP loss

A static shared dispatch-depth guard prevents Mutation Meddley's synthetic bonus-damage event from recursively starting another Mutation Meddley proc chain while the nested event is active.

The `FireEvent()` boolean is not treated as proof that damage happened; it is retained separately as event-continuation state.

## Rank-1/rank-2 baseline layer

The first branch remains locked until rank 3, but the paid mutation is no longer mechanically empty before then.

The baseline layer deliberately reuses existing resource keys:

- Carapace Evolution: Brace
- Living Crystal: Stress
- Brineborn: Reserve
- Ash Metabolism: Embers
- Walking Colony: Colony Pressure

Its effects are intentionally smaller than rank-3 identities and disappear into the chosen branch's normal machinery once an evolution exists.

Carapace Evolution still respects functional dormancy: without vanilla Carapace, shared baseline and completion hooks exit without granting stats or reactions.

## Stance and specialization completion

The static-freeze pass closes previously inert displayed choices without replacing the concrete branch systems.

Examples:

- Brineborn Cool Reserve can proactively bank Reserve into Mend; Cool Sump can recycle Reserve under hostile environmental pressure.
- Walking Colony Bank Scars can build Stitch before Scar Feeders exists.
- Graft Parliament receives a weaker solo fallback so Delegate Load and Override Frame function without requiring another mutation, while compatible anatomy still improves the normal branch logic.
- Thermal Baffles preserve stance-matched attunement under environmental pressure.
- Mire Sheath gains and spends Mire through wet/saline pressure and close contact.

Rank-9 completion follows the same rule: preserve existing passive bonuses, but add a meter refund, generation rule, retaliation, environmental conversion, or direct contact consequence where a capstone previously existed mostly as another stat line.

## Environment predicates

Light uses the real `Cell.IsLit()` predicate, and wet detection first checks the cell's open liquid volume. Saline, hot, and smoky detection still partly use the string representation of the cell or liquid.

Those heuristics are a known runtime-validation boundary. Version 0.6.3 does not replace them with guessed APIs. When `Evolution Seed [DEV]` is present, the mechanics text exposes live `lit`, `wet`, `saline`, `hot`, and `smoky` booleans so actual representative Qud cells can be measured and any mismatch can be fixed from evidence.

## UI strategy

Version 0.6.3 keeps `Popup.ShowOptionList` for both path selection and mutation-specific stance changes.

This keeps the current UI keyboard, mouse, controller, and handheld friendly without committing yet to a custom full-screen mutation tree.

A future mutation-tree UI should remain a separate presentation layer over the same evolution state and rules.

## Content shape

Version 0.6.3 has four connected layers of content:

- `Evolution Seed [DEV]` remains the regression harness and now also helps expose environmental predicate state
- `Living Crystal`, `Brineborn`, `Ash Metabolism`, and `Walking Colony` are Mutation Meddley-owned flagship mutations
- `Carapace Evolution` is the narrow vanilla adapter and remains dormant without live vanilla Carapace
- a tag-driven synergy/discovery layer that connects those mutations to curated vanilla mutation families and to each other through visible pair synergies, branch-locked hidden rank-9 adaptations, and named triads

The semantic layer remains intentionally constrained:

- exact mutation-specific gameplay still belongs primarily in the mutation class
- shared semantic tags are derived, not authoritative saved state
- functionally inactive evolving mutations are excluded from semantic-tag queries
- one mutation class owns each mutation-local synergy effect even if several mutation pages report the same named relationship
- triad participants may each apply their documented mutation-local contribution
- hidden discovery remains mutation-local and rank-gated; no retroactive rank-9 respec flow exists

## Compatibility strategy

Prefer, in order:

1. standard Qud XML/data definitions
2. `BaseMutation`/parts/events/activated abilities
3. narrowly scoped helper code
4. Harmony only when an otherwise inaccessible game behavior must be changed

Project-owned identifiers should use the `MutationMeddley_` prefix.

## Static-freeze boundary

The issue-by-issue completion and acceptance tests live in `docs/STATIC_FREEZE_PLAN.md` and `docs/TESTING.md`.

Once v0.6.3 compiles in the installed Qud build and passes those behavioral gates, the next phase should be playtesting, not another architecture rewrite. Subsequent changes should primarily answer observed questions of balance, cadence, clarity, environmental reliability, controller usability, and whether the branches are fun in normal runs.
