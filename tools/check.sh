#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

command -v python3 >/dev/null 2>&1 || {
  echo "python3 is required for the repository preflight checks." >&2
  exit 1
}

python3 - "$REPO_ROOT" <<'PY'
import json
import pathlib
import re
import sys
import xml.etree.ElementTree as ET

root = pathlib.Path(sys.argv[1])
manifest = root / "manifest.json"
mutations = root / "Mutations.xml"
code_dir = root / "Code"
installer = code_dir / "MutationMeddley_BiologySupport.Install.cs"
biology_core = code_dir / "MutationMeddley_BiologySupport.CoreA.cs"
biology_actions = code_dir / "MutationMeddley_BiologySupport.ActionsA.cs"
action_service = code_dir / "MutationMeddley_PrimaryActionService.cs"
adaptive = code_dir / "MutationMeddley_AdaptiveMutationBase.cs"

errors = []
warnings = []


def fail(message):
    errors.append(message)


def warn(message):
    warnings.append(message)


with manifest.open("r", encoding="utf-8") as handle:
    data = json.load(handle)

required_manifest_keys = ("ID", "Title", "Version")
missing = [key for key in required_manifest_keys if not data.get(key)]
if missing:
    fail(f"manifest.json is missing required values: {', '.join(missing)}")

if data.get("ID") != "MutationMeddley":
    fail(f"manifest ID must remain MutationMeddley, got {data.get('ID')!r}")

try:
    mutation_tree = ET.parse(mutations)
except ET.ParseError as exc:
    fail(f"Mutations.xml is not well-formed XML: {exc}")
    mutation_tree = None

cs_files = sorted(code_dir.rglob("*.cs")) if code_dir.exists() else []
if not cs_files:
    fail("No C# files were found under Code/.")

all_code = "\n".join(path.read_text(encoding="utf-8") for path in cs_files)

# Caves of Qud compiles scripting mods from source. A checked-in DLL is both stale
# and a packaging hazard for this repository's supported deployment model.
dll_files = sorted(root.rglob("*.dll"))
if dll_files:
    fail("Do not commit scripting-mod DLLs: " + ", ".join(str(p.relative_to(root)) for p in dll_files))

# Mutation XML is part of the public registration contract. Keep identifiers unique
# and ensure every registered class actually exists in the source set.
if mutation_tree is not None:
    mutation_nodes = mutation_tree.findall(".//mutation")
    seen_names = set()
    seen_classes = set()
    for node in mutation_nodes:
        class_name = (node.get("Class") or "").strip()
        mutation_name = (node.get("Name") or "<unnamed>").strip()
        if mutation_name in seen_names:
            fail(f"Mutations.xml contains duplicate mutation name {mutation_name!r}")
        seen_names.add(mutation_name)
        if class_name in seen_classes:
            fail(f"Mutations.xml contains duplicate mutation class {class_name!r}")
        seen_classes.add(class_name)
        if not class_name:
            fail(f"Mutations.xml mutation {mutation_name!r} has no Class")
            continue
        if not class_name.startswith("MutationMeddley_"):
            fail(f"Mutations.xml class {class_name!r} must use the MutationMeddley_ prefix")
        if re.search(rf"\bclass\s+{re.escape(class_name)}\b", all_code) is None:
            fail(f"Mutations.xml registers {class_name!r}, but no matching C# class was found")

# The first 0.7.1 build failed Qud compilation because [Serializable] was repeated on
# multiple partial declarations. Catch that exact class of regression before Qud.
biology_serializable = re.findall(
    r"\[Serializable\]\s*public\s+partial\s+class\s+MutationMeddley_BiologySupport\b",
    all_code,
    flags=re.MULTILINE,
)
if len(biology_serializable) != 1:
    fail(
        "MutationMeddley_BiologySupport must have exactly one [Serializable] partial "
        f"declaration; found {len(biology_serializable)}"
    )

# Common typo class of failure: this misspelling is close enough to compile review to
# be easy to overlook but guarantees a missing-type compiler error.
if "MutationMledley_" in all_code:
    fail("Found misspelled identifier prefix MutationMledley_; use MutationMeddley_")

if not installer.exists():
    fail("Missing MutationMeddley_BiologySupport.Install.cs")
else:
    installer_text = installer.read_text(encoding="utf-8")
    if installer_text.count("[PlayerMutator]") != 1:
        fail("Biology bootstrap must contain exactly one [PlayerMutator]")
    if "IPlayerMutator" not in installer_text:
        fail("Biology [PlayerMutator] must implement IPlayerMutator")
    if installer_text.count("[CallAfterGameLoadedAttribute]") != 1:
        fail("Biology bootstrap must contain exactly one [CallAfterGameLoadedAttribute]")
    if installer_text.count("trustedPlayerObject: true") < 2:
        fail("Both new-game and loaded-save Biology hooks must use the trusted player-object path")

if not biology_core.exists():
    fail("Missing MutationMeddley_BiologySupport.CoreA.cs")
else:
    biology_text = biology_core.read_text(encoding="utf-8")
    # Mutation-specific command events must be received by the owning mutation, not
    # by the player-global inspector. This is the Qud-native ownership boundary.
    for command_name in (
        "CarapaceCommand",
        "CrystalCommand",
        "BrineCommand",
        "AshCommand",
        "ColonyCommand",
    ):
        if f"RegisterPartEvent(this, {command_name}" in biology_text:
            fail(f"Biology must not register mutation action command {command_name}")

if not action_service.exists():
    fail("Missing MutationMeddley_PrimaryActionService.cs")
else:
    action_service_text = action_service.read_text(encoding="utf-8")
    for marker in (
        "class MutationMeddley_PrimaryActionService",
        "MutationMeddley_TryUse(",
        'owner.UseEnergy(1000, "Physical Mutation")',
        "MutationMeddley_TryBiologyHeal",
    ):
        if marker not in action_service_text:
            fail(f"Primary action service is missing required transaction marker: {marker}")

if not biology_actions.exists():
    fail("Missing MutationMeddley_BiologySupport.ActionsA.cs")
else:
    biology_actions_text = biology_actions.read_text(encoding="utf-8")
    if "MutationMeddley_PrimaryActionService.MutationMeddley_TryUse" not in biology_actions_text:
        fail("Biology action bridge must delegate to MutationMeddley_PrimaryActionService")
    # The player-global inspector may route an action, but it must not own resource
    # transaction details or the turn cost itself.
    if "UseEnergy(" in biology_actions_text:
        fail("Biology ActionsA must not spend energy directly")
    for resource_key in (
        "carapace_brace",
        "lc_stress",
        "brine_reserve",
        "ash_embers",
        "colony_charge",
    ):
        if resource_key in biology_actions_text:
            fail(f"Biology ActionsA still owns mutation resource transaction {resource_key!r}")

if not adaptive.exists():
    fail("Missing MutationMeddley_AdaptiveMutationBase.cs")
else:
    adaptive_text = adaptive.read_text(encoding="utf-8")
    required_adaptive_markers = (
        "Object.RegisterPartEvent(this, primaryActionCommand)",
        "MutationMeddley_SyncPrimaryActionAbility",
        '"primary_action_guid"',
        '"primary_action_signature"',
        "MutationMeddley_SyncModeAbility",
    )
    for marker in required_adaptive_markers:
        if marker not in adaptive_text:
            fail(f"Adaptive mutation lifecycle is missing required marker: {marker}")

# Scope guards. This stabilization release intentionally uses Qud-native parts/events
# only and does not introduce unseeded System.Random behavior.
if re.search(r"\bnew\s+(?:System\.)?Random\s*\(", all_code):
    fail("Unseeded Random construction found in Code/; use a verified Qud-compatible RNG path")
if "HarmonyLib" in all_code or re.search(r"\[Harmony(?:Patch|Prefix|Postfix|Transpiler)", all_code):
    fail("Harmony usage found in Code/; v0.7.1 stabilization is intentionally Qud-native")

# Known technical debt stays visible in every preflight instead of disappearing from
# the engineering checklist. These direct heals predate shared verb scaling.
direct_heals = []
for path in cs_files:
    for line_no, line in enumerate(path.read_text(encoding="utf-8").splitlines(), start=1):
        if "ParentObject.Heal(" in line:
            direct_heals.append((path.relative_to(root), line_no, line.strip()))
if direct_heals:
    warn(
        "Direct ParentObject.Heal calls still bypass shared continuous verb scaling:\n"
        + "\n".join(f"    {path}:{line_no}: {line}" for path, line_no, line in direct_heals)
    )

# Keep deprecated surfaces visible without blocking the supported target build; Qud's
# installed compiler remains the authority for whether/when these must migrate.
legacy_register_count = all_code.count("override void Register(GameObject Object)")
option_list_count = all_code.count("Popup.ShowOptionList(")
if legacy_register_count:
    warn(f"Legacy Register(GameObject) overrides present: {legacy_register_count}")
if option_list_count:
    warn(f"Popup.ShowOptionList calls present: {option_list_count}")

if errors:
    print("Repository preflight FAILED:", file=sys.stderr)
    for error in errors:
        print(f"  ERROR: {error}", file=sys.stderr)
    if warnings:
        print("\nWarnings:", file=sys.stderr)
        for warning in warnings:
            for line in warning.splitlines():
                print(f"  WARN: {line}", file=sys.stderr)
    raise SystemExit(1)

print("Repository preflight passed:")
print(f"  manifest: {data.get('Title')} {data.get('Version')} ({data.get('ID')})")
print("  Mutations.xml: well-formed XML")
print(f"  C# sources: {len(cs_files)} file(s)")
print("  registered mutation classes: present, unique, and prefixed")
print("  Biology serialization: exactly one [Serializable] declaration")
print("  Biology lifecycle: new-game + load hooks present")
print("  mutation action ownership: mutation-side registration verified")
print("  primary action transactions: isolated from Biology UI")
print("  Harmony/unseeded RNG guards: clear")

if warnings:
    print("\nKnown warnings/debt:")
    for warning in warnings:
        for line in warning.splitlines():
            print(f"  WARN: {line}")
PY

cat <<'EOF'

Static repository checks passed.
Caves of Qud remains the authoritative compiler and runtime validator for this scripting mod.
After deployment, fully restart Qud and inspect build_log.txt and Player.log.
EOF
