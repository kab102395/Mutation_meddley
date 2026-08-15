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

## Framework and content test

Verify:

1. the mod appears in the Mod Manager
2. C# compiles without errors
3. `Evolution Seed [DEV]`, `Living Crystal`, `Brineborn`, and `Carapace Evolution` appear during mutant character creation
4. each mutation exposes its `Evolve ...` activated ability
5. `Living Crystal`, `Brineborn`, and `Carapace Evolution` also expose their `Retune ...` activated abilities
6. no branch is available below rank 3
7. rank 3 only presents the three identity branches for the mutation being tested
8. choosing one rank-3 branch prevents choosing another tier-1 branch
9. rank 6 only shows specializations belonging to the selected rank-3 branch
10. rank 9 only shows capstones belonging to the selected rank-6 branch
11. the option-list picker works from keyboard and controller-friendly inputs without numeric text entry
12. changing stance with `Retune ...` visibly changes the mutation's stat profile and level text
13. save/reload preserves the selected path and current stance
14. removing the mutation removes its activated abilities

## Carapace adapter test

`Carapace Evolution` is a companion mutation, not a direct replacement for vanilla `Carapace`.

Verify:

1. taking both `Carapace` and `Carapace Evolution` produces the intended shell-focused build
2. `Carapace Evolution` alone still behaves as a coherent standalone mutation
3. changing `Carapace Evolution` stances updates the shell-focused stat tradeoff
4. vanilla `Carapace` still provides its ordinary shell behavior alongside the companion evolution layer
5. save/reload preserves the companion path and stance while vanilla `Carapace` remains present

## Save compatibility checks

Before changing persistent framework fields:

1. keep a save made with the prior released version
2. update the mod
3. load the old save
4. verify mutation state and activated abilities
5. save again
6. reload the new save

Do not release serialized field-layout changes without a migration plan.

## Steam Workshop upload on Linux

Use Qud's built-in uploader after the deployed local copy works.

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
- new-game test
- existing-save test where applicable
- mutation gain/removal test
- each evolution path selected at least once
- Workshop-installed copy tested on Linux
- manifest version updated
- README status updated
- balance notes updated for gameplay mutations
