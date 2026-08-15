# AGENTS.md

These rules apply to Codex and other automated contributors working on Mutation Meddley.

## Project goal

Mutation Meddley is a Caves of Qud mutation framework and mutation pack focused on branching evolution, build diversity, meaningful tradeoffs, and cross-mutation synergies.

The framework should make it possible to add many mutations without duplicating branch-selection and persistence logic in every mutation class.

## Development platform

The owner's primary development and test platform is **Zorin OS / Linux**.

- Write repository tooling for Bash/Linux first.
- Use Linux paths in primary documentation and examples.
- Do not introduce Windows-only build/deploy requirements unless explicitly requested.
- Support standard Steam and Flatpak Steam layouts where practical.
- Keep path overrides available for nonstandard Steam libraries or Qud configuration locations.
- Treat filesystem path casing as significant.

## Hard constraints

1. Do not invent Caves of Qud APIs. Verify API names/signatures against current game assemblies, current official modding documentation, or known-good current examples before using them.
2. Prefer normal Qud parts, mutations, events, XML merging, and activated abilities over Harmony patches.
3. Do not add a Harmony patch unless the requested feature cannot reasonably be implemented through Qud's supported systems and the reason is documented.
4. Prefix project-owned unique identifiers with `MutationMeddley_` unless a Qud API specifically requires a different format.
5. Do not copy whole vanilla XML blocks when a merge or small definition is sufficient.
6. Avoid modifying vanilla mutations directly until the framework proof of concept has been tested in-game.
7. Keep framework code independent of specific mutation names. Mutation-specific behavior belongs in mutation classes, not in the framework base class.
8. Treat save compatibility as a design requirement. Do not casually add/remove/change serialized public fields on existing mutation classes. Add an explicit migration plan when serialized shape must change.
9. Use named arguments for Qud APIs with optional parameters when practical.
10. Do not use unseeded randomness for gameplay mechanics when Qud provides a seeded alternative.
11. Qud's installed/current assemblies and its runtime scripting compiler are authoritative. A local editor build does not prove the mod loads in-game.
12. Keep Qud-generated `Mods.csproj`, `bin/`, `obj/`, logs, and `workshop.json` out of source control.

## Evolution model

Default milestone model:

- rank 3: primary specialization
- rank 6: secondary specialization
- rank 9: advanced specialization/capstone path
- rank 10: optional automatic capstone improvement, not necessarily another choice

A mutation may deviate from this model when the design justifies it.

Evolution choices should primarily change behavior rather than only increase numbers.

Avoid trees where one branch is a strict mathematical upgrade over the others. Each branch should push the player toward a different tactical pattern, equipment preference, stat priority, risk profile, or mutation synergy.

## Mutation implementation checklist

For each new gameplay mutation:

- add or update its `Mutations.xml` entry
- use a project-prefixed C# class
- define base behavior and rank scaling
- define evolution choices and prerequisites
- document the intended build identities
- document meaningful weaknesses/tradeoffs
- note any cross-mutation synergies
- test mutation gain/removal
- test rank changes
- test branch selection
- test save/reload after branch selection
- inspect `build_log.txt` and `Player.log` after C# changes

## Testing boundary

Do not claim that C# changes compile in Caves of Qud unless they have actually been loaded by the owner's current Linux Qud installation or validated against the exact current game assemblies.

On this repository, the normal local loop is:

```bash
bash tools/check.sh
bash tools/deploy.sh
# restart/test Caves of Qud
bash tools/logs.sh
```

When a build/load error occurs, use the exact compiler/runtime message from Qud's logs before editing APIs by guesswork.
