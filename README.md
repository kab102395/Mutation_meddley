# Mutation Meddley

Mutation Meddley is a Caves of Qud mutation-evolution framework and content mod.

The long-term goal is to make mutations branch into mechanically distinct builds instead of following a single linear scaling path. The framework is designed around milestone evolutions, prerequisites, mutually exclusive branches, and cross-mutation synergies.

## Target development platform

The primary development and test platform for this repository is **Zorin OS / Linux with Steam**.

The tooling assumes a normal Linux shell and supports both the standard Steam layout and the common Flatpak Steam layout for Qud's user configuration directory. If Qud is installed in a nonstandard location, paths can be overridden with environment variables instead of editing scripts.

## Current status

Version `0.1.0` is a framework proof of concept. It contains:

- a Qud `manifest.json`
- a reusable `MutationMeddley_EvolvingMutationBase`
- tiered evolution choices with prerequisites
- persistent selected-evolution state
- an activated `Evolve ...` command
- a developer-only proof-of-concept mutation, `Evolution Seed [DEV]`
- rank 3, 6, and 9 branch milestones
- Linux/Zorin deployment, validation, log, and `Mods.csproj` helper scripts

The developer mutation intentionally does not provide final gameplay effects yet. Its purpose is to validate loading, C# compilation, mutation leveling, branch selection, save/load persistence, and the framework API before vanilla mutations are modified.

## Zorin OS quick start

Keep the Git repository in a normal development directory, for example `~/Development/Mutation_meddley`, and deploy a clean runtime copy into Qud's offline Mods directory.

From the repository root:

```bash
git checkout feat/basic-framework
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

1. Run `bash tools/check.sh`.
2. Run `bash tools/deploy.sh`.
3. Start or restart Caves of Qud.
4. Make sure mods and scripting mods are enabled.
5. Enable **Mutation Meddley** in the in-game mod configuration if necessary.
6. Start a new mutant character and take `Evolution Seed [DEV]`.
7. Increase the mutation rank normally.
8. At ranks 3, 6, and 9, use `Evolve Evolution Seed [DEV]` and choose a branch.
9. Save and reload after making choices to verify persistence.

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

Do not create or commit `workshop.json` manually during normal development. Once the deployed mod works locally, use Qud's built-in **Modding Utilities > Steam Workshop Uploader**.

Create the Workshop ID from the deployed Linux copy, fill in the item metadata, and use **Upload Content...**. Qud will create `workshop.json` inside the deployed mod directory. The repository ignores that file so the local Workshop association is not accidentally committed.

When testing a subscribed Workshop build, avoid loading the separate offline development copy at the same time.

## Design documents

- `docs/ARCHITECTURE.md` - framework boundaries and evolution model
- `docs/BALANCE.md` - mutation/evolution balance rules
- `docs/TESTING.md` - Zorin/Linux test and Workshop workflow
- `AGENTS.md` - constraints for Codex/AI changes

## Namespace and identifier policy

All unique C# classes, commands, and other internal identifiers created by this project should use the `MutationMeddley_` prefix unless a Qud API specifically requires another form. This reduces collisions with the base game and other mods.
