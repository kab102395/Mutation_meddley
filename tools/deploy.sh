#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
MOD_NAME="MutationMeddley"

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

  # Qud creates this tree after it has been launched at least once.
  printf '%s\n' "$xdg_base/unity3d/Freehold Games/CavesOfQud"
}

QUD_CONFIG="$(find_qud_config_dir)"
TARGET="${1:-${MUTATION_MEDDLEY_MOD_DIR:-$QUD_CONFIG/Mods/$MOD_NAME}}"

printf 'Deploying Mutation Meddley\n'
printf 'Source: %s\n' "$REPO_ROOT"
printf 'Target: %s\n' "$TARGET"

mkdir -p "$TARGET"

for file in manifest.json Mutations.xml; do
  if [[ ! -f "$REPO_ROOT/$file" ]]; then
    printf 'Required runtime file not found: %s\n' "$REPO_ROOT/$file" >&2
    exit 1
  fi
  cp -f "$REPO_ROOT/$file" "$TARGET/$file"
done

for directory in Code Textures; do
  rm -rf "$TARGET/$directory"
  if [[ -d "$REPO_ROOT/$directory" ]]; then
    cp -a "$REPO_ROOT/$directory" "$TARGET/$directory"
  fi
done

for file in Preview.png preview.png; do
  if [[ -f "$REPO_ROOT/$file" ]]; then
    cp -f "$REPO_ROOT/$file" "$TARGET/$file"
  fi
done

printf '\nDeployment complete.\n'
printf 'Existing workshop.json was preserved if present.\n'
printf 'Restart Caves of Qud before testing C# changes.\n'
printf 'Build log: %s/build_log.txt\n' "$QUD_CONFIG"
printf 'Player log: %s/Player.log\n' "$QUD_CONFIG"
