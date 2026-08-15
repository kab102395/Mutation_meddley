# Hidden Evolutions and Discovery Rules

## Goal

Hidden evolutions exist to make different runs reveal different possibilities. They should reward unusual character state, repeated behavior, and world interaction without turning the system into arbitrary secret-checklist design.

A hidden evolution is successful when the player can eventually understand why it appeared and wants to experiment with reproducing or avoiding that condition in another run.

## Principles

### Hidden does not mean random nonsense

A hidden evolution should have a cause that is internally coherent with the mutation and with Qud's simulation.

Good examples:

- repeated electrical exposure changes a structural mutation
- a fungal infection unlocks a mycelial symbiosis branch
- sustained time spent in saline liquid unlocks a Brineborn specialization
- a crystalline mutation exposed to sonic trauma develops resonance behavior

Weak examples:

- 1% chance on level-up with no thematic or mechanical relationship
- an unlock tied to an unrelated arbitrary item because it is rare

### Discovery should come from play

Prefer conditions based on accumulated experience or current build state over requirements that force external wiki use.

Potential discovery inputs:

- damage-type exposure history
- time spent in terrain or liquids
- mutation tags present
- body-plan state
- disease/fungal status
- cybernetics installed
- repeated mutation use patterns
- phase, temporal, or dimensional incidents
- diet/cooking state where relevant

### Conditions should be testable

Every hidden evolution needs an internal design record containing:

- exact trigger
- whether progress persists
- when eligibility is checked
- whether the option remains available after the trigger condition ends
- save/load expectations
- debug/testing method

### Hidden paths should not become mandatory optimization

Secret content should create alternate identities, not mandatory superior versions of visible branches.

If a hidden path is strictly stronger than the ordinary path, the discovery system collapses into a spoiler tax.

## Eligibility timing

Preferred behavior:

1. world behavior accumulates or creates an eligibility state
2. the next appropriate evolution milestone checks eligibility
3. the hidden option appears alongside ordinary eligible choices
4. once presented or unlocked, policy is explicit about whether it remains permanently known for that character

Avoid interrupting normal gameplay constantly with unsolicited evolution popups unless a future feature is specifically designed around dramatic metamorphosis.

## Example hidden evolutions

### Carapace: Piezoelectric Carapace

Potential requirements:

- Carapace or adapter state
- ELECTRICAL-compatible mutation/tag or substantial electrical exposure
- structural branch compatibility

Identity:

- impacts and forced movement generate charge
- stored charge can power retaliation or defensive reactions

Tradeoff:

- electrical overload or EMP-like interactions may become more consequential

### Carapace: Mycochitin

Potential requirements:

- qualifying fungal state
- adaptive branch compatibility

Identity:

- shell and fungus become one ecological layer
- fungal interactions become beneficial or controllable in new ways

Tradeoff:

- fire, cleansing, or anti-fungal effects may carry new costs

### Living Crystal: Fractured Choir

Potential requirements:

- resonance branch compatibility
- repeated sonic/kinetic trauma

Identity:

- fractures become resonant nodes rather than pure injury
- damage history affects sonic behavior

Tradeoff:

- certain frequencies or impacts can produce dangerous feedback

### Brineborn: Salt Ghost

Potential requirements:

- prolonged saline exposure plus a phase/dimensional condition

Identity:

- saline pools become anchors for unusual movement or presence

Tradeoff:

- power drops sharply away from saline environments

## Discovery messaging

When a hidden option first appears, the UI should indicate that it is unusual without fully explaining every implementation detail.

Possible presentation:

`UNUSUAL ADAPTATION`

Then show a normal selectable branch with concise flavor/mechanical text.

A details view may reveal a hint such as:

- "Your shell remembers repeated electrical trauma."
- "The fungal lattice has become inseparable from your tissues."

The player should receive enough information to connect cause and effect.

## Persistence guidance

Do not add casual public serialized fields for every hidden unlock.

Prefer encoding durable hidden-evolution state into the framework's existing stable serialized evolution-state mechanism or a future explicitly versioned state payload.

If accumulated counters are required, design their persistence and migration before implementation.

## Content gate

A proposed hidden evolution should answer all of these:

1. What observable player behavior or state causes it?
2. Why does that cause make sense fictionally and mechanically?
3. How can QA force or reproduce it?
4. Is it an alternate identity rather than a strict upgrade?
5. What does the player give up by choosing it?
6. What information will the UI expose?
7. What happens after save/load?
8. Does the condition interact safely with removing the source mutation/state?
