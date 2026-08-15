#!/usr/bin/env bash
set -euo pipefail

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
BUILD_LOG="$QUD_CONFIG/build_log.txt"
PLAYER_LOG="$QUD_CONFIG/Player.log"

printf 'Qud configuration directory: %s\n\n' "$QUD_CONFIG"

for log in "$BUILD_LOG" "$PLAYER_LOG"; do
  if [[ -f "$log" ]]; then
    printf '===== %s =====\n' "$log"
    tail -n 120 "$log"
    printf '\n'
  else
    printf 'Not found yet: %s\n\n' "$log"
  fi
done
