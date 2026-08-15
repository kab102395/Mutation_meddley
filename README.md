# Mutation Meddley

Mutation Meddley is a Caves of Qud mutation-evolution framework and content mod.

The long-term goal is to make mutations branch into mechanically distinct builds instead of following a single linear scaling path. The framework is designed around milestone evolutions, prerequisites, mutually exclusive branches, and cross-mutation synergies.

## Current status

Version `0.1.0` is a framework proof of concept. It contains:

- a Qud `manifest.json`
- a reusable `MutationMeddley_EvolvingMutationBase`
- tiered evolution choices with prerequisites
- persistent selected-evolution state
- an activated `Evolve ...` command
- a developer-only proof-of-concept mutation, `Evolution Seed [DEV]`
- rank 3, 6, and 9 branch milestones
- a Windows deployment script for the Qud offline Mods directory

The developer mutation intentionally does not provide final gameplay effects yet. Its purpose is to validate loading, C# compilation, mutation leveling, branch selection, save/load persistence, and the framework API before vanilla mutations are modified.

## Recommended development layout

Keep the Git repository in a normal development directory and deploy a clean runtime copy into Qud's offline Mods directory.

On Windows, from the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\deploy.ps1
```

The default target is:

```text
%USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\Mods\MutationMeddley
```

You can override the target:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\deploy.ps1 -Target "D:\Some\Other\Mods\MutationMeddley"
```

## Testing in Caves of Qud

1. Run `tools/deploy.ps1`.
2. Start Caves of Qud.
3. Make sure `Modding > Enable Mods` is enabled.
4. Open the in-game Mod Manager and enable **Mutation Meddley** if necessary.
5. Start a new mutant character and take `Evolution Seed [DEV]`.
6. Increase the mutation rank normally.
7. At ranks 3, 6, and 9, use the activated ability named `Evolve Evolution Seed [DEV]` and choose a branch.
8. Save and reload after making choices to verify persistence.

If Qud reports a compilation/load error, check:

```text
%USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\build_log.txt
%USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\Player.log
```

See `docs/TESTING.md` for the development loop.

## Steam Workshop

Do not create or commit `workshop.json` manually during normal development. Once the deployed mod works locally, use Qud's built-in **Modding Utilities > Steam Workshop Uploader**. Qud will create `workshop.json` in the deployed mod directory when you create the Workshop item.

The repository ignores `workshop.json` so a local Workshop ID is not accidentally committed.

## Design documents

- `docs/ARCHITECTURE.md` - framework boundaries and evolution model
- `docs/BALANCE.md` - mutation/evolution balance rules
- `docs/TESTING.md` - local test and Workshop workflow
- `AGENTS.md` - constraints for Codex/AI changes

## Namespace and identifier policy

All unique C# classes, commands, and other internal identifiers created by this project should use the `MutationMeddley_` prefix unless a Qud API specifically requires another form. This reduces collisions with the base game and other mods.
