# Testing and Release Workflow

## Primary local development loop: Zorin OS / Linux

1. Make changes in the Git repository.
2. Run the repository preflight:

```bash
bash tools/check.sh
```

3. Deploy the runtime files to Qud's offline Mods directory:

```bash
bash tools/deploy.sh
```

4. Start or restart Caves of Qud.
5. Confirm **Mutation Meddley** is enabled in the in-game mod configuration.
6. Test the relevant behavior.
7. Inspect Qud's logs after C# changes:

```bash
bash tools/logs.sh
```

Standard Linux paths:

```text
~/.config/unity3d/Freehold Games/CavesOfQud/Mods/MutationMeddley
~/.config/unity3d/Freehold Games/CavesOfQud/build_log.txt
~/.config/unity3d/Freehold Games/CavesOfQud/Player.log
```

The Bash tools also detect the common Flatpak Steam configuration root under `~/.var/app/com.valvesoftware.Steam/.config`. For an unusual install, set `QUD_CONFIG_DIR` explicitly.

Do not diagnose a Qud API/compiler error by guessing. Copy the exact error and resolve the class, namespace, method, or signature that Qud reports.

## Qud-generated C# project

Qud is the authoritative runtime compiler for this mod. For IDE navigation and local C# diagnostics, generate the project file from the installed game so its assembly references match the installed Qud version.

In Caves of Qud:

1. enable **Enable Mods**
2. enable **Allow scripting mods**
3. restart Qud
4. open **Modding Utilities**
5. choose **Write Mods.csproj file**

Then run:

```bash
bash tools/import-mods-csproj.sh
```

The generated `Mods.csproj` is intentionally ignored by Git. Do not ship generated `bin/` or `obj/` output as Workshop mod content.

## v0.6.3 static-freeze gate

`docs/STATIC_FREEZE_PLAN.md` is the issue-by-issue rationale for this pass. Before treating the current roster as balance-ready, validate every section below.

### 1. Rank-1/rank-2 baseline loops

Use fresh characters or the DEV harness and test every gameplay mutation before any rank-3 branch is selected.

- **Carapace Evolution + vanilla Carapace**: stand still until baseline Brace appears, take damage while wounded, verify one Brace is consumed and one wound closes. Moving should decay baseline Brace. Carapace Evolution without vanilla Carapace must remain dormant.
- **Living Crystal**: stand still or remain in close pressure until baseline Stress appears, take damage while wounded, verify one Stress is consumed and one wound closes. Confirm baseline AV and modest heat/cold resistance appear.
- **Brineborn**: enter qualifying saline terrain, build Reserve, verify baseline heat/cold resistance scales with Reserve, then take damage while wounded and verify one Reserve can close one wound.
- **Ash Metabolism**: build Embers in hot/smoky terrain, verify baseline heat resistance, then take damage while wounded and verify one Ember can cauterize one wound.
- **Walking Colony**: move to build Colony Pressure, verify baseline Toughness and the pressure-threshold DV support, then take damage while wounded and verify one Pressure can close one wound.

For all five, full HP must not consume a baseline resource merely to attempt a heal.

### 2. Evolution graph and controller flow

Verify:

1. the mod appears in the Mod Manager
2. C# compiles without errors
3. `Evolution Seed [DEV]`, `Living Crystal`, `Brineborn`, `Ash Metabolism`, `Walking Colony`, and `Carapace Evolution` appear during mutant character creation
4. each mutation exposes its `Evolve ...` activated ability
5. all five gameplay mutations expose their `Retune ...` activated ability
6. no branch is available below rank 3
7. rank 3 only presents the three identity branches for the mutation being tested
8. choosing one rank-3 branch prevents choosing another tier-1 branch
9. rank 6 only shows specializations belonging to the selected rank-3 branch
10. rank 9 only shows capstones belonging to the selected rank-6 branch
11. the option-list picker works from keyboard and controller-friendly inputs without numeric text entry
12. changing stance with `Retune ...` changes behavior and/or current bonuses; no displayed stance should be a no-op
13. save/reload preserves the selected path and current stance/resource envelope
14. removing the mutation removes its activated abilities
15. an older semicolon-only evolution state still loads and rewrites into the envelope safely on a later state change

### 3. Contact-gate and bonus-damage plumbing

Use `Evolution Seed [DEV]` to toggle `Toggle Mutation Meddley Damage Trace [DEV]` only for controlled checks.

For Hunter Shell / Ramming Gait, Cinder Gut, Saltglass/Scouring contact spends, Resonant/Prismatic/Diamond strike spends, and Surveyor Swarm:

1. prime the branch meter
2. hit an adjacent hostile with an ordinary melee attack
3. verify the meter spends once and the mutation follow-up dispatches once
4. prime the meter again
5. deal damage from non-adjacent range
6. verify the meter is preserved and the mutation follow-up does **not** dispatch
7. confirm trace text reports `contact.gate` rejection for non-contact damage
8. confirm synthetic Mutation Meddley bonus damage cannot recursively re-enter any Mutation Meddley damage handler
9. confirm multiple primed mutations may respond to the original legitimate event, but the synthetic follow-up does not start a nested proc chain
10. confirm trace output distinguishes target resolution, dispatch, event continuation, and observed HP loss

Explicit edge case: stand adjacent to a hostile while also considered engaged, then use a ranged attack at that adjacent target. The current contact gate intentionally uses adjacency + melee engagement because no verified weapon-source API is assumed. Record whether this edge case is classified as contact. If it is, treat that as a concrete follow-up bug rather than silently broadening the gate with an unverified API.

After controlled validation, turn damage tracing back off.

### 4. Brineborn movement and stance truthfulness

Verify:

- movement-sensitive Brineborn bonuses appear immediately after entering a new cell, before EndTurn clears `brine_moved`
- Breakwater Predator and Drift Parliament movement slices are visible during their moved state
- Draw Brine still heals only while wounded and consumes Reserve only when the recovery actually occurs
- Cool Reserve can create Mend while healthy by banking Reserve while stationary or in wet/saline/hot conditions
- stored Mend later responds to pressure through Wellspring's normal pressure handler
- Cool Sump + Cool Reserve can recover Reserve from hostile wet/saline/hot pressure
- Shell Up, Knife Rind, Dry Tide, and Surge Tide each create their intended Bastion/Wake payoff instead of only draining Reserve
- reserve capacity remains 6 plus supported Amphibious / lit Photosynthetic Skin bonuses without collapsing to the base cap

### 5. Walking Colony stance and solo-branch truthfulness

Verify:

- rank-3 Marrow Hive + Knit Flesh turns stationary Colony Pressure into healing/Stitch only when wounded
- rank-3 Marrow Hive + Bank Scars can turn stationary Pressure into Stitch before Scar Feeders is selected
- after Scar Feeders is selected, its native stronger Bank Scars loop applies without obvious double conversion
- Surveyor Range Ahead and Harry Line build Scout differently and Scout is consumed only on qualifying melee contact
- Graft Parliament can build Parliament without support mutations: stationary Delegate Load banks load, while moving Override Frame can trade Pressure for load
- adding a compatible BODY_PART_INTERACTION or STRUCTURAL mutation improves the corresponding Graft loop through the normal synergy code rather than disabling it
- Burrowed Nursery adds a distinct rooted Burrowing Claws Stitch-regeneration effect under pressure
- Pack Pursuit refunds Scout on moving melee contact
- Distributed Verdict retaliates only on ordinary incoming damage, not generic environmental damage
- Colony Interface and Choir of Tendons regenerate Parliament under their intended supporting-anatomy conditions

### 6. Carapace Evolution actionability

`Carapace Evolution` remains a companion mutation and must be tested with and without vanilla Carapace.

Verify:

- without vanilla Carapace, no baseline loop, branch passive, tag, discovery, stance reaction, or capstone reaction leaks through
- re-adding vanilla Carapace restores the saved path/stance/state
- Fortress builds and spends Brace; Anchor Down and Spiteful Wall produce different behavior
- Spiteful Wall retaliates against a valid source and does not recursively proc MM damage
- Hunter Shell builds Impact from movement/contact; Skirmish and Ramming spend it differently
- Ramming Gait damages prey on qualifying melee contact but preserves Impact on non-contact damage
- Adaptive Carapace builds heat/mire/rime from its environment and spends them on pressure/contact
- Thermal Baffles reinforce stance-matched environmental attunement
- Mire Sheath gains Mire from wet/saline pressure and can spend Mire for a close-contact membrane follow-up
- Porcupine Redoubt retaliates while rooted with Quills
- Skitter Bulwark refunds Impact on moving contact with Multiple Legs
- Hookstorm Frame adds its Quill-backed contact follow-up
- Estuary Husk reinforces Mire under wet/saline pressure
- Storm Carapace responds to environmental pressure by reinforcing its weakest attunement

### 7. Living Crystal actionability

Verify each identity independently:

- Diamond Lattice builds Stress from pressure/stillness and spends it through pressure/contact
- Faceted Bulwark and Dense Core alter the Stress loop rather than only the sheet
- Prismatic Matrix builds Dawn in light and Dusk in dimness; the matching pool spends on pressure/contact
- Dawn Glare and Dusk Glare remain meaningfully different after rank 6
- Resonant Crystal turns movement Cadence into Release; Pulse Step and Humming Guard spend Release differently
- every Living Crystal rank-9 choice alters generation, spend, refund, or direct event outcome
- Cadence is only surfaced for Resonant Crystal

### 8. Ash Metabolism actionability

Verify:

- Furnace Skin creates Kiln through Bank/Flare behavior and Kiln answers pressure
- Cinder Gut creates Rush and Rush only cashes out on qualifying contact
- Smoke Organ creates Haze and Haze changes pressure/route behavior
- Glasshouse Carapace restores Kiln under hot pressure
- Ember Halo restores Kiln under lit pressure
- Wakefeast restores Rush after moving melee contact
- Overdraft Heart restores Rush after hot melee contact
- Crematory Mirage restores Haze under smoky pressure
- Blackdraft Engine restores Haze after moving smoky melee contact
- Cinder Jet adds one direct draft follow-up and restores Haze during moving smoky melee contact

### 9. Environment predicate matrix

With `Evolution Seed [DEV]` present, each adaptive mutation appends live diagnostic booleans for `lit`, `wet`, `saline`, `hot`, and `smoky` to its mechanics text.

Record values on at least:

- ordinary dry floor
- ordinary lit floor
- ordinary dark floor
- open fresh water/liquid
- actual brine/salt liquid or terrain
- actual fire
- lava or magma if safely testable
- smoke
- steam
- another gas cloud
- ash/cinder-themed terrain that should or should not count as hot/smoky

Do not redesign the sensor from assumptions. Any mismatch should be reported with the exact real cell/liquid state that produced it.

### 10. Rank-9 capstone verb audit

For every normal and unusual rank-9 option, perform this test:

> Ignore its AV/DV/Quickness/resistance lines. Does selecting it still alter a meter, generation/spend/refund rule, event reaction, or direct combat consequence?

The answer must be yes for every shipped rank-9 choice before calling the content model static-complete.

### 11. Synergy and hidden-adaptation test

Supported vanilla synergy pack for `0.6.3`:

- `Carapace`
- `Regeneration`
- `Multiple Legs`
- `Quills`
- `Electrical Generation`
- `Light Manipulation`
- `Flaming Ray`
- `Freezing Ray`
- `Photosynthetic Skin`
- `Phasing`
- `Amphibious`
- `Heightened Hearing`
- `Burrowing Claws`

Verify:

1. each supported vanilla mutation activates at least one visible synergy with one of the flagship mutations
2. dormant `Carapace Evolution` never contributes semantic tags or pair/triad eligibility
3. branch-specific synergies change by branch or stance rather than only repeating the same numeric bonus
4. all eighteen named triads appear only for their intended branch combinations
5. all twenty hidden adaptations require their discovery conditions before they appear at rank 9
6. hidden discovery stops once that rank-9 tier is already spent
7. save/reload preserves hidden unlock history while recomputing active tags/synergies from the live build

### 12. Save compatibility and stress test

Before public release:

1. keep a v0.6.2 save with selected branches, stances, resources, and at least one hidden unlock
2. update to v0.6.3
3. load the old save and verify the same selections/resources
4. exercise a new baseline/capstone reaction
5. save again and reload
6. remove/re-add vanilla Carapace and verify dormant state
7. move or auto-explore at least 200 local tiles on a build carrying several owned mutations
8. watch for visible stutter, state corruption, repeated messages, or Player.log spam

No public serialized field layout changed in v0.6.3. If a future change requires one, add an explicit migration plan first.

## Steam Workshop upload on Linux

Use Qud's built-in uploader only after the deployed local copy works.

1. Deploy with `bash tools/deploy.sh`.
2. Start Qud.
3. Open **Modding Utilities** from the main-menu overlay.
4. Open the **Steam Workshop Uploader**.
5. Select Mutation Meddley.
6. Use **Create Workshop Id for Mod...** the first time.
7. Fill in title, description, tags, visibility, and preview image as desired.
8. Use **Upload Content...**.
9. Subscribe to the Workshop item and test the Workshop-installed copy separately.

Qud creates `workshop.json` for the Workshop item. It is intentionally ignored by this repository and the deployment script preserves the deployed copy.

When testing a subscribed Workshop build, avoid simultaneously loading a conflicting offline development copy of the same mod.

## Release checklist

Before a public release:

- `bash tools/check.sh` passes
- clean C# compile in the current local Qud installation
- no unexpected `Player.log` errors
- rank-1/rank-2 baseline matrix passes
- contact-gate melee/ranged matrix passes
- every stance has a reachable mechanical difference
- every rank-6 specialization modifies its branch loop
- every rank-9 choice passes the non-stat verb audit
- environment predicate matrix has been recorded
- new-game test passes
- v0.6.2 existing-save test passes
- mutation gain/removal and Carapace dormancy tests pass
- controller option lists remain usable without numeric text entry
- 200+ tile multi-mutation stress test shows no unacceptable stutter/log spam
- Workshop-installed copy tested separately on Linux before publishing broadly
- manifest version and README status match the tested commit
