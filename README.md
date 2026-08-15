# Mutation Meddley

Mutation Meddley is a Caves of Qud mutation-evolution framework and content mod.

The long-term goal is to make mutations branch into mechanically distinct builds instead of following a single linear scaling path. The framework is designed around milestone evolutions, prerequisites, mutually exclusive branches, actionable stances, stored-state verbs, and cross-mutation synergies.

## Target development platform

The primary development and test platform for this repository is **Zorin OS / Linux with Steam**.

The tooling assumes a normal Linux shell and supports both the standard Steam layout and the common Flatpak Steam layout for Qud's user configuration directory. If Qud is installed in a nonstandard location, paths can be overridden with environment variables instead of editing scripts.

## Current status

Version `0.6.3` is the static-freeze candidate for the first full verb/passive playtest. It contains:

- a reusable evolution framework with a single extensible serialized state envelope
- controller-friendly evolution and stance pickers built on `Popup.ShowOptionList`
- rank 3, 6, and 9 branching with prerequisites, tier locking, and unusual hidden adaptations
- a modest rank-1/rank-2 baseline passive/reaction loop for every shipped gameplay mutation
- event-driven branch verbs that build and spend shell, crystal, brine, ash, and colony state
- a conservative shared close-contact gate so non-adjacent damage cannot cash out melee/contact meters
- a shared explicit `Damage`/`TakeDamage` dispatch path with cross-mutation recursion protection and DEV tracing
- live mechanics and current-passive readouts in mutation level text
- a shared runtime semantic-tag and synergy-query layer
- a developer regression mutation, `Evolution Seed [DEV]`, including live environment-predicate diagnostics
- four Mutation Meddley-owned flagship mutations: `Living Crystal`, `Brineborn`, `Ash Metabolism`, and `Walking Colony`
- a narrow companion adapter for vanilla `Carapace`: `Carapace Evolution`
- curated synergy support for `Carapace`, `Regeneration`, `Multiple Legs`, `Quills`, `Electrical Generation`, `Light Manipulation`, `Flaming Ray`, `Freezing Ray`, `Photosynthetic Skin`, `Phasing`, `Amphibious`, `Heightened Hearing`, and `Burrowing Claws`
- twenty curated hidden adaptations across the owned mutations and `Carapace Evolution`
- eighteen named triad adaptations
- Linux/Zorin deployment, validation, log, and `Mods.csproj` helper scripts

The `0.6.3` pass specifically closes the known static actionability gaps: pre-rank-3 dead zones, Brineborn movement refresh, Cool Reserve, pre-Scar-Feeders Bank Scars, solo Graft Parliament fallback behavior, passive-heavy Carapace/Brine/Ash/Colony specializations and capstones, and non-contact meter consumption. See `docs/STATIC_FREEZE_PLAN.md` for the issue-by-issue plan and `docs/TESTING.md` for the release gate.

`Carapace Evolution` is intentionally a companion mutation rather than a full replacement of Qud's built-in `Carapace` class. If vanilla `Carapace` is lost, the companion mutation becomes dormant but keeps its chosen path and stance for later reactivation.

## Zorin OS quick start

Keep the Git repository in a normal development directory, for example `~/Development/Mutation_meddley`, and deploy a clean runtime copy into Qud's offline Mods directory.

From the repository root:

```bash
bash tools/check.sh
bash tools/deploy.sh
```

For a standard native Steam installation, the default runtime target resolves to:

```text
~/.config/unity3d/Freehold Games/CavesOfQud/Mods/MutationMeddley
```

For Steam installed as a Flatpak, the scripts also look under:

```text
~/.var/app/com.valvesoftware.Steam/.config/unity3d/Freehold Games/CavesOfQud
```

You can always override Qud's configuration directory explicitly:

```bash
QUD_CONFIG_DIR="/custom/path/to/CavesOfQud" bash tools/deploy.sh
```

Or deploy to an exact mod directory:

```bash
MUTATION_MEDDLEY_MOD_DIR="/custom/Mods/MutationMeddley" bash tools/deploy.sh
```

The deployment script preserves an existing `workshop.json` in the deployed copy.

## C# development project on Linux

Caves of Qud itself is the authoritative compiler for scripting mods. For IDE navigation and compile-time assistance, Qud can generate its current `Mods.csproj` against the assemblies in your installed game.

In Qud:

1. Enable **Enable Mods**.
2. Enable **Allow scripting mods**.
3. Restart Qud.
4. Open **Modding Utilities**.
5. Select **Write Mods.csproj file**.

Then, from this repository:

```bash
bash tools/import-mods-csproj.sh
```

That copies the Qud-generated project file into the repository root for use with VS Code, Rider, or another Linux-capable C# editor. `Mods.csproj`, `bin/`, and `obj/` are ignored by Git because they are local/generated development artifacts rather than Workshop content.

## Testing in Caves of Qud

The full v0.6.3 matrix is in `docs/TESTING.md`. The minimum local gate is:

1. Run `bash tools/check.sh`.
2. Run `bash tools/deploy.sh`.
3. Start or restart Caves of Qud.
4. Confirm a fresh successful scripting-mod compile.
5. Start a new mutant and verify each paid mutation does something visible before rank 3.
6. For each mutation, test all rank-3 identities, both stances, both rank-6 specializations, and rank-9 capstones.
7. Verify a primed contact meter spends on adjacent melee contact but survives a non-adjacent ranged hit.
8. With `Evolution Seed [DEV]`, inspect `lit/wet/saline/hot/smoky` diagnostics on real representative cells.
9. Save/reload, then verify Carapace dormancy by removing and regaining vanilla `Carapace`.
10. Run a multi-mutation movement stress test before drawing balance conclusions.

All five gameplay mutations expose `Retune ...` after a path exists. Normal gameplay interaction uses option lists rather than typed numbers.

Existing pre-envelope saves that only store semicolon-separated evolution IDs should still load cleanly; v0.6.3 retains the same persistent envelope contract as v0.6.2 and introduces no new public serialized field.

For controlled proc validation, `Evolution Seed [DEV]` exposes `Toggle Mutation Meddley Damage Trace [DEV]`. Leave it off for normal play. Turn it on only when verifying target/source resolution, close-contact classification, one-proc-per-spend behavior, event-continuation semantics, observed HP loss, or bonus-damage failure paths.

To inspect the current Linux logs from the terminal:

```bash
bash tools/logs.sh
```

The standard Linux log locations are:

```text
~/.config/unity3d/Freehold Games/CavesOfQud/build_log.txt
~/.config/unity3d/Freehold Games/CavesOfQud/Player.log
```

If Qud reports a C# build error, use the exact error from these logs rather than guessing at API signatures.

## Steam Workshop

Keep this static-freeze candidate off Workshop until the local compile and behavioral matrix pass.

Once the deployed mod works locally, use Qud's built-in **Modding Utilities > Steam Workshop Uploader**. Qud will create `workshop.json` inside the deployed mod directory; the repository ignores that file and the deployment script preserves it.

When testing a subscribed Workshop build, avoid loading the separate offline development copy at the same time.

## Design documents

- `docs/STATIC_FREEZE_PLAN.md` - issue-by-issue v0.6.3 correction plan and acceptance contract
- `docs/ARCHITECTURE.md` - framework boundaries and evolution model
- `docs/BALANCE.md` - mutation/evolution balance rules
- `docs/SYNERGY_MATRIX.md` - synergy ownership, discovery keys, and QA matrix
- `docs/TESTING.md` - full Linux behavior/release matrix
- `AGENTS.md` - constraints for Codex/AI changes

## Namespace and identifier policy

All unique C# classes, commands, and other internal identifiers created by this project should use the `MutationMeddley_` prefix unless a Qud API specifically requires another form. This reduces collisions with the base game and other mods.
