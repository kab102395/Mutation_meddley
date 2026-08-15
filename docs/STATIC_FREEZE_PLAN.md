# Mutation Meddley v0.6.3 Static Freeze Plan

This document defines the correctness pass required before balance and feel testing becomes the primary source of truth.

The goal is not to add more mutation content. The goal is to make every currently shipped gameplay mutation useful before rank 3, make every branch and stance mechanically actionable, prevent false contact procs, and ensure rank-6/rank-9 choices alter an existing gameplay loop rather than existing only as passive numbers.

## Acceptance contract

A mutation is ready for the static freeze when all of the following are true:

1. ranks 1-2 have a modest passive identity and at least one observable reactive/resource loop
2. rank 3 establishes a distinct build/spend or environment/reaction verb
3. both displayed stances have reachable gameplay behavior; a stance cannot be a decorative mode label
4. rank 6 modifies generation, spending, timing, or consequence of the rank-3 verb
5. rank 9 modifies that loop again or supplies an observable capstone reaction
6. offensive stored-state spends only trigger from qualifying close contact and preserve state on non-contact damage
7. passive effects refresh while their movement/environment condition is actually true
8. no resource is spent at full HP solely to attempt a heal that cannot occur
9. dormant companion mutations do not provide gameplay tags, stats, discoveries, or reactions
10. persistent public fields remain save-compatible unless a migration is explicitly introduced

## Issue 1: rank-1/rank-2 dead zone

### Problem

The paid flagship mutations cost 3-5 mutation points but previously did little or nothing mechanically until the first evolution choice at rank 3.

### Fix

Keep rank 3 as the first identity split, but give each mutation a restrained pre-branch loop using its existing state vocabulary.

- Carapace Evolution: holding ground builds a small Brace pool; Brace can close a wound when damage lands; modest AV/Toughness support. Still requires live vanilla Carapace.
- Living Crystal: stillness or close pressure builds a small Stress pool; Stress can close a wound; modest AV and thermal-resistance support.
- Brineborn: existing Reserve generation remains active; Reserve provides weather resistance and may close a wound.
- Ash Metabolism: existing Ember generation remains active; Embers provide heat resistance and may cauterize a wound.
- Walking Colony: existing Colony Pressure generation remains active; Pressure firms the body and may close a wound.

No new serialized public field is required; the baseline loops reuse existing envelope keys.

### Acceptance test

At mutation ranks 1 and 2, each mutation must visibly alter either the character sheet or a resource and must produce one observable reaction when its baseline resource is available.

## Issue 2: generic outgoing damage could spend contact meters

### Problem

`AttackerDealtDamage` covers damage broadly. A shell slam, crystal contact release, estuary wake, cinder rush, or surveyor pursuit effect should not be consumed merely because the player dealt non-contact damage.

### Fix

Add a shared conservative contact gate in `MutationMeddley_EvolvingMutationBase`.

A qualifying contact currently requires:

- a resolved defender
- player and defender in adjacent cells
- the player currently engaged in melee

`MutationMeddley_GetOutgoingDamageTarget()` classifies the current damage event. When it classifies the event as non-contact:

- `MutationMeddley_ConsumeStateInt()` refuses to consume stored state
- `MutationMeddley_TryBonusDamage()` refuses to dispatch the synthetic follow-up
- DEV damage tracing reports that the contact gate rejected the event

This is deliberately conservative. No unverified Qud weapon-source API is guessed. Adjacent ranged attacks made while also engaged in melee remain an explicit runtime edge case to test.

### Acceptance test

A normal adjacent melee hit can consume the relevant meter and produce its follow-up. A non-adjacent ranged or mutation-damage hit must not consume the meter or dispatch the follow-up.

## Issue 3: Brineborn movement-sensitive passives refreshed too late

### Problem

Brineborn set its moved flag on `EnteredCell` but did not refresh passive effects until later, after its turn processor could reset that flag. Movement-sensitive pair/triad bonuses could therefore miss the state they depended on.

### Fix

The adaptive base now performs a final passive refresh on `EnteredCell` after the concrete mutation handler. This keeps Brineborn movement passives synchronized and also provides a common safety net for future shared behavior.

### Acceptance test

Immediately after moving, Brineborn level text/current bonuses must reflect movement-sensitive bonuses before EndTurn clears the movement flag.

## Issue 4: Cool Reserve was not a complete stance

### Problem

Wellspring Flesh described Cool Reserve as banking recovery/weather protection, but only Draw Brine had a concrete Mend-generation loop.

### Fix

Cool Reserve can now convert Reserve into a short-lived Mend buffer while stationary or in wet, saline, or hot conditions. Cool Sump additionally recovers Reserve when hostile environmental pressure lands while Cool Reserve is active.

### Acceptance test

A healthy Wellspring character in Cool Reserve can deliberately create Mend without first taking a wound. Damage later consumes Mend through the existing Wellspring pressure response.

## Issue 5: Bank Scars was inert before Scar Feeders

### Problem

Marrow Hive exposed Bank Scars at rank 3, but its active bank behavior was effectively reserved for the later Scar Feeders specialization.

### Fix

Before Scar Feeders is selected, stationary Bank Scars may convert Colony Pressure into Stitch. Scar Feeders retains its stronger native implementation and does not receive the fallback twice.

### Acceptance test

A rank-3 Marrow Hive character can switch between Knit Flesh and Bank Scars and observe two different resource loops before rank 6.

## Issue 6: Graft Parliament could be inert without another body-plan mutation

### Problem

Delegate Load and Override Frame depended strongly on other BODY_PART_INTERACTION or STRUCTURAL mutations. A solo Graft Parliament purchase could therefore expose stances whose state never meaningfully generated.

### Fix

Keep cross-mutation anatomy as the stronger ecology, but add a solo fallback:

- stationary Delegate Load may bank Parliament when no other body-part-interaction mutation is present
- moving Override Frame may convert Colony Pressure into Parliament when no other structural mutation is present

### Acceptance test

A Graft Parliament character with no supporting mutation can build and spend Parliament in both stances. Adding compatible anatomy should still improve the loop through the existing concrete synergy code.

## Issue 7: several rank-6 specializations remained mostly passive

### Problem

Some specializations changed stats but not the resource/reaction engine.

### Fix

- Thermal Baffles: environmental pressure reinforces the attunement favored by the current veil stance.
- Mire Sheath: wet/saline pressure builds Mire, and wet/saline close contact can spend Mire for a membrane follow-up.
- Cool Sump: environmental pressure in Cool Reserve can recover Reserve.

Other rank-6 paths already alter state generation/spending in their concrete classes.

### Acceptance test

For every rank-6 selection, removing its passive-stat lines mentally must still leave a state-generation, state-spend, refund, recovery, or event consequence that distinguishes it from its sibling.

## Issue 8: passive-only or weak rank-9 capstones

### Problem

A subset of rank-9 choices had evocative names but mostly appended AV/DV/Quickness/resistance bonuses.

### Fix

Add mutation-local event consequences without removing their existing passives.

Carapace Evolution:

- Porcupine Redoubt: stationary Quill-backed retaliation on ordinary incoming damage
- Skitter Bulwark: moving Multiple Legs contact refunds Impact
- Hookstorm Frame: Quill-backed contact follow-up
- Estuary Husk: wet/saline pressure reinforces Mire
- Storm Carapace: environmental damage reinforces the currently weakest attunement

Brineborn:

- Whitewater Predator: dry-ground melee contact refunds Wake
- Abyssal Brine: wet/saline pressure restores Reserve

Ash Metabolism:

- Glasshouse Carapace: hot pressure restores Kiln
- Ember Halo: lit pressure restores Kiln
- Wakefeast: moving melee contact restores Rush
- Overdraft Heart: hot melee contact restores Rush
- Crematory Mirage: smoky pressure restores Haze
- Blackdraft Engine: moving smoky melee contact restores Haze
- Cinder Jet: moving smoky melee contact adds a small direct follow-up and restores Haze

Walking Colony:

- Ossuary Bloom: pressure regrows Stitch while colony mass remains
- Burrowed Nursery: rooted Burrowing Claws pressure regrows extra Stitch
- Pack Pursuit: moving melee contact refunds Scout
- Distributed Verdict: ordinary incoming damage can throw a small direct retaliation while Colony Pressure remains
- Colony Interface: compatible body-plan anatomy regenerates Parliament under pressure
- Choir of Tendons: resonance-friendly anatomy regenerates Parliament under pressure

Living Crystal's current rank-9 paths already participate in Stress, Dawn/Dusk, or Release generation/spending and therefore did not need a blanket supplement.

### Acceptance test

Every rank-9 selection must change a meter, a spend/refund rule, an event reaction, or a direct combat consequence in addition to any passive statistics.

## Issue 9: environment predicates are heuristic

### Problem

Wet detection has a real open-liquid check, and light uses `Cell.IsLit()`, but saline/hot/smoky behavior still partly depends on string representations of the cell or liquid. Static compilation cannot prove that every real Qud fire, smoke, steam, gas, or brine tile maps to the intended predicate.

### Fix

Do not replace the heuristics with an invented API. Instead, when `Evolution Seed [DEV]` is present, each adaptive mutation's mechanics text exposes the live booleans:

- lit
- wet
- saline
- hot
- smoky

Use those diagnostics against actual game cells. Replace a heuristic only after a current Qud API or observed runtime object representation is verified.

### Acceptance test

Record predicate values on ordinary floor, open fresh liquid, brine/salt liquid, actual fire/lava, smoke/gas/steam, and dark/lit cells. Any false positive or false negative becomes a targeted environment-sensor bug with concrete runtime evidence.

## Issue 10: save compatibility and dormant leakage

### Problem

This pass touches shared event routing and resource behavior. It must not destabilize existing saves or reintroduce the earlier Carapace dormancy/tag leaks.

### Fix

- no public serialized field was added or removed
- baseline/capstone completion uses existing envelope keys
- transient contact/movement context is explicitly `[NonSerialized]`
- semantic tag helpers continue to ignore functionally inactive evolving mutations
- shared completion hooks exit when `MutationMeddley_IsFunctionallyActive()` is false

### Acceptance test

Load a v0.6.2 save, verify paths/stances/resources, save under v0.6.3, reload, remove/re-add vanilla Carapace, and confirm no dormant stats/tags/discoveries or reactions leak through.

## Issue 11: Spiteful Wall consumed Brace without an exchange

### Problem

The existing Fortress turn processor removed one Brace simply for remaining engaged in `Spiteful Wall`. That could erase the resource that the stance needs for its actual retaliation before an enemy applied pressure, recreating the earlier self-sabotaging stance problem.

### Fix

When the character held position and remained engaged in Spiteful Wall, the shared completion pass restores the Brace point that the old turn processor removed. Moving can still lose Brace; rooted engagement now preserves it for an actual incoming-pressure spend.

### Acceptance test

Build Brace in Spiteful Wall while stationary beside an enemy. Ending the turn must not drain Brace merely because the enemy is adjacent. When the enemy actually damages the character, Brace can then be spent into the spiteful retaliation.

## Issue 12: incoming capstones used a transient movement flag after EndTurn

### Problem

The shared moved-this-turn flag is intentionally cleared at EndTurn. Incoming enemy attacks commonly happen after that point, so using the transient flag to decide whether Porcupine Redoubt or Burrowed Nursery was rooted would incorrectly treat a character as stationary after every EndTurn.

### Fix

Use state that survives the turn boundary for incoming rooted checks:

- Porcupine Redoubt reads Carapace Evolution's persisted `carapace_stationary` state
- Burrowed Nursery reads Walking Colony's persisted stride streak (`0` means the preceding colony turn was stationary)

The transient moved flag remains appropriate for within-turn outgoing contact/refund checks.

### Acceptance test

Move and end the turn, then allow an enemy to hit. Rooted-only Porcupine/Burrowed behavior must not fire for that moved turn. Spend a full turn stationary and repeat; the rooted reaction may then fire when its other requirements are met.

## Static-freeze exit criteria

Do not add another flagship mutation before these are tested.

The code is a static-freeze candidate when:

- repository preflight passes
- Qud's runtime C# compiler reports no errors
- each rank-1/2 baseline loop fires
- every rank-3 branch can build and spend its own state
- both stances of every branch have reachable behavior
- every rank-6 specialization changes that loop
- every rank-9 choice changes that loop or produces a visible event consequence
- non-contact damage does not consume contact meters
- Brine movement bonuses update while moved
- rooted-only post-turn reactions respect the preceding movement state
- environment diagnostics match real representative cells
- save/reload and Carapace dormancy remain correct

After those gates pass, further changes should be driven primarily by balance, clarity, frequency, and fun observed in normal runs rather than by another architecture redesign.
