# Balance Framework

Mutation Meddley should create more viable build paths, not simply increase player power without opportunity cost.

## Mutation point budgets

These are design targets, not rigid formulas.

### 1-point mutations

- narrow utility or supplemental mechanics
- useful but rarely build-defining alone
- branches may create niche synergies rather than large raw bonuses

### 2-point mutations

- meaningful build components
- tactical effects with noticeable opportunity cost
- should support at least two distinct use patterns after evolution

### 3-point mutations

- strong build-shaping mechanics
- can justify equipment/stat decisions around them
- evolution branches should feel materially different in play

### 4-5 point mutations

- character-defining systems
- high impact must be paired with meaningful constraints, cooldowns, positioning requirements, resource costs, or opportunity cost
- capstones may be dramatic, but should not erase counterplay

## Continuous mutation-rank budget

Mutation points spent between evolution milestones must never feel like dead investment.

The current `0.7.0` cadence separates **quantitative maturation** from **qualitative evolution**:

- ranks 2, 5, and 8 raise a branch-appropriate maturity passive
- ranks 4, 7, and 10 raise shared Mutation Meddley healing/bonus-damage output and a branch-appropriate verb-support passive
- ranks 3, 6, and 9 add the identity, specialization, and capstone rule changes

The normal balance target is ranks 1-10. Physical rapid advancement may produce higher effective levels; the same formulas can continue above 10, but post-10 values are secondary balance targets.

Continuous scaling must obey these rules:

- old mechanics mature instead of being replaced by later mechanics
- a rank increase must be visible in the mutation page or in the output of an already-owned verb
- passive scaling supports a branch; it does not become the branch's only reason to exist
- resource capacity should not automatically grow just because level grows unless capacity itself is the intended progression axis
- synergy and triad bonuses are balanced separately and should not multiply the continuous curve into a mandatory combination
- dormant mutations, especially Carapace Evolution without vanilla Carapace, receive no continuous rank bonus
- if rank-10 scaling makes a setup-free branch outperform a setup-heavy sibling, reduce the curve or branch passive rather than removing continuous progression

At the current formulas, maturity increases at 2/5/8 and verb output increases at 4/7/10. These are deliberately coarse enough to be noticeable without adding a new menu choice every level.

## Evolution budget

An evolution choice should usually spend its power budget on one or more of:

- changing targeting or area shape
- changing action economy
- adding a conditional trigger
- adding a drawback in exchange for specialization
- converting one resource/environmental state into another
- creating synergy with a different mutation family
- changing equipment/body-slot incentives
- changing defensive profile
- creating a new tactical loop

Raw percentage increases are acceptable as support, but should rarely be the entire identity of a branch.

## Default milestones

### Rank 3 - identity

The player chooses the primary direction of the mutation. The rank-1/2 baseline and its continuous maturation remain part of the character rather than being discarded.

### Rank 6 - specialization

The player chooses how that identity functions tactically. Rank-2/4/5 progression remains active beneath the specialization.

### Rank 9 - capstone path

The player chooses a strong payoff that reinforces the existing path rather than replacing it. Earlier rank scaling remains active beneath the capstone or unusual adaptation.

### Rank 10 - mastery

Rank 10 is automatic quantitative mastery rather than another mandatory menu choice. The current shared curve adds the third normal verb-output/support increase here.

## Anti-solved-build rules

A branch is suspect if it is better than another branch in nearly every realistic situation.

When comparing sibling branches, ask:

- What build wants branch A but not branch B?
- What enemy/environment favors branch B?
- What stat or equipment tradeoff changes?
- What does the player give up?
- Does one branch improve both offense and defense while another only improves one?
- Is a branch only interesting because its numbers are larger?
- Does continuous level scaling preserve the branch's intended setup requirement, or let raw rank overwhelm it?

If those questions do not produce meaningful distinctions, redesign the branch.

## Synergy policy

Cross-mutation synergies should reward discovery without making a specific pair mandatory.

Good synergy:

- changes behavior when two mechanics naturally interact
- creates an alternate build identity
- remains useful when only one mutation is present

Bad synergy:

- multiplies damage so strongly that both mutations become mandatory
- hides baseline functionality behind possession of another mutation
- creates a single dominant combo that invalidates unrelated builds

## Playtest data to record

For each mutation path, record at minimum:

- mutation cost
- rank tested
- maturity tier and verb-growth tier
- relevant attributes
- equipment assumptions
- average cooldown/use frequency
- damage or mitigation range when relevant
- healing per resource spend when relevant
- action/energy cost
- common failure cases
- strongest known synergy
- strongest known counter
- whether ranks 2/4/5/7/8/10 each feel worth a mutation point
- whether the path feels useful before its capstone
