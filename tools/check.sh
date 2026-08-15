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
import sys
import xml.etree.ElementTree as ET

root = pathlib.Path(sys.argv[1])

manifest = root / "manifest.json"
mutations = root / "Mutations.xml"
code_dir = root / "Code"

with manifest.open("r", encoding="utf-8") as handle:
    data = json.load(handle)

required_manifest_keys = ("ID", "Title", "Version")
missing = [key for key in required_manifest_keys if not data.get(key)]
if missing:
    raise SystemExit(f"manifest.json is missing required values: {', '.join(missing)}")

ET.parse(mutations)

cs_files = sorted(code_dir.rglob("*.cs")) if code_dir.exists() else []
if not cs_files:
    raise SystemExit("No C# files were found under Code/.")

print("Repository preflight passed:")
print(f"  manifest: {data['Title']} {data['Version']} ({data['ID']})")
print("  Mutations.xml: well-formed XML")
print(f"  C# sources: {len(cs_files)} file(s)")
PY

cat <<'EOF'

Static repository checks passed.
Caves of Qud remains the authoritative compiler for the scripting mod.
After deployment, launch/restart Qud and inspect build_log.txt and Player.log.
EOF
