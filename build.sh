#!/usr/bin/env bash
# Local build: compiles the mod, then copies the installable ZIP into ./dist and checks its layout.
#
# Usage:
#   ./build.sh [GAME_PATH]
#
#   GAME_PATH  Folder holding the Stardew Valley / SMAPI / MonoGame DLLs to compile against.
#              Optional if ModBuildConfig can auto-detect your game install.
#              (Also read from the GAME_PATH environment variable.)
#
# Requires: the .NET SDK (net6.0 targeting pack is restored from NuGet) and network access for NuGet.
# This script only writes to src/bin, src/obj and dist/. It never deletes source files or your game/mod data.

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MOD_NAME="TractorSmash"
PROJECT="$ROOT/src/$MOD_NAME.csproj"
DIST="$ROOT/dist"
GAME_PATH="${1:-${GAME_PATH:-}}"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

die() { echo "ERROR: $*" >&2; exit 1; }

command -v dotnet >/dev/null 2>&1 || die "dotnet SDK not found."
[ -f "$PROJECT" ] || die "Project not found: $PROJECT"

ARGS=(--configuration Release -p:EnableModDeploy=false)   # don't copy into a Mods folder; we only want the ZIP
if [ -n "$GAME_PATH" ]; then
  [ -d "$GAME_PATH" ] || die "GAME_PATH does not exist: $GAME_PATH"
  ARGS+=("-p:GamePath=$GAME_PATH")
fi

echo "==> Building $MOD_NAME (Release)"
dotnet build "$PROJECT" "${ARGS[@]}"

# Newest ZIP produced under src/bin
ZIP="$(find "$ROOT/src/bin" -name '*.zip' -type f -print0 2>/dev/null | xargs -0 -r ls -t | head -n 1 || true)"
[ -n "$ZIP" ] || die "Build finished but no ZIP was found under src/bin."

list_zip() {
  if command -v unzip >/dev/null 2>&1; then
    unzip -Z1 "$1"
  else
    python3 -I -c "import sys,zipfile;print('\n'.join(zipfile.ZipFile(sys.argv[1]).namelist()))" "$1"
  fi
}

ENTRIES="$(list_zip "$ZIP")"
for required in "$MOD_NAME/$MOD_NAME.dll" "$MOD_NAME/manifest.json"; do
  echo "$ENTRIES" | grep -qx "$required" || die "ZIP is missing '$required'. Contents:
$ENTRIES"
done

mkdir -p "$DIST"
cp -f "$ZIP" "$DIST/"
OUT="$DIST/$(basename "$ZIP")"

echo "==> ZIP contents:"
echo "$ENTRIES" | sed 's/^/    /'
echo "==> Installable ZIP: $OUT"
echo "    Extract it into your game's Mods folder so you get Mods/$MOD_NAME/$MOD_NAME.dll"
