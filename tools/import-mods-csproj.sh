#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

find_qud_config_dir() {
  if [[ -n "${QUD_CONFIG_DIR:-}" ]]; then
    printf '%s\n' "$QUD_CONFIG_DIR"
    return
  fi

  local xdg_base="${XDG_CONFIG_HOME:-$HOME/.config}"
  local candidates=(
    "$xdg_base/unity3d/Freehold Games/CavesOfQud"
    "$HOME/.var/app/com.valvesoftware.Steam/.config/unity3d/Freehold Games/CavesOfQud"
  )

  local candidate
  for candidate in "${candidates[@]}"; do
    if [[ -d "$candidate" ]]; then
      printf '%s\n' "$candidate"
      return
    fi
  done

  printf '%s\n' "$xdg_base/unity3d/Freehold Games/CavesOfQud"
}

QUD_CONFIG="$(find_qud_config_dir)"
SOURCE="$QUD_CONFIG/Mods.csproj"
DESTINATION="$REPO_ROOT/Mods.csproj"

if [[ ! -f "$SOURCE" ]]; then
  cat >&2 <<EOF
Qud-generated Mods.csproj was not found at:
  $SOURCE

In Caves of Qud:
  1. Enable Mods and Allow scripting mods.
  2. Restart the game.
  3. Open Modding Utilities.
  4. Choose "Write Mods.csproj file".
Then run this script again.
EOF
  exit 1
fi

cp -f "$SOURCE" "$DESTINATION"
printf 'Copied Qud-generated project file to:\n  %s\n' "$DESTINATION"
printf 'Do not treat this generated file as the shipped mod; Qud compiles the runtime C# from the deployed mod directory.\n'
