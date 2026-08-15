# Testing and Release Workflow

## Local development loop (Windows)

1. Make changes in the Git repository.
2. From the repository root run:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\deploy.ps1
```

3. Start or restart Caves of Qud.
4. Confirm **Mutation Meddley** is enabled in the Mod Manager.
5. Test the relevant behavior.
6. Inspect Qud's logs after C# changes.

Default Windows logs:

```text
%USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\build_log.txt
%USERPROFILE%\AppData\LocalLow\Freehold Games\CavesOfQud\Player.log
```

Do not diagnose a Qud API/compiler error by guessing. Copy the exact error and resolve the class, namespace, method, or signature that Qud reports.

## Framework proof-of-concept test

Use `Evolution Seed [DEV]`.

Verify:

1. the mod appears in the Mod Manager
2. C# compiles without errors
3. the mutation appears during mutant character creation
4. the `Evolve Evolution Seed [DEV]` activated ability exists
5. no branch is available below rank 3
6. rank 3 presents exactly Bulwark, Predator, and Symbiote
7. choosing one rank-3 branch prevents choosing another tier-1 branch
8. rank 6 only shows specializations belonging to the selected rank-3 branch
9. rank 9 only shows capstones belonging to the selected rank-6 branch
10. save/reload preserves the selected path
11. removing the mutation removes its activated ability

## Save compatibility checks

Before changing persistent framework fields:

1. keep a save made with the prior released version
2. update the mod
3. load the old save
4. verify mutation state and activated abilities
5. save again
6. reload the new save

Do not release serialized field-layout changes without a migration plan.

## Steam Workshop upload

Use Qud's built-in uploader after the deployed local copy works.

1. Deploy the mod to Qud's offline `Mods\MutationMeddley` folder.
2. Start Qud.
3. Open **Modding Utilities** from the main-menu overlay.
4. Open the **Steam Workshop Uploader**.
5. Select Mutation Meddley.
6. Use **Create Workshop Id for Mod...** the first time.
7. Fill in title, description, tags, visibility, and preview image as desired.
8. Use **Upload Content...**.
9. Subscribe to the Workshop item and test the Workshop-installed copy separately.

Qud creates `workshop.json` for the Workshop item. It is intentionally ignored by this repository.

When testing a subscribed Workshop build, avoid simultaneously loading a conflicting offline development copy of the same mod.

## Release checklist

Before a public release:

- clean C# compile in current Qud
- no unexpected `Player.log` errors
- new-game test
- existing-save test where applicable
- mutation gain/removal test
- each evolution path selected at least once
- Workshop-installed copy tested
- manifest version updated
- README status updated
- balance notes updated for gameplay mutations
