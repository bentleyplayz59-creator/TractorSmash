# Tractor Smash

A companion for Pathoschild's [Tractor Mod](https://www.nexusmods.com/stardewvalley/mods/1401). While you ride the tractor, everything in range is smashed without holding a tool: rocks, boulders, meteorites, trees, stumps, logs, twigs, weeds and bushes. Crops in range are watered automatically. Hold a hoe or seeds and Tractor Mod still tills and plants as usual.

It never touches tilled soil (other than watering), placed objects, or chests. Fruit trees, tapped trees and grass are **off** by default.

- Target: Stardew Valley 1.6.x (developed against 1.6.15), SMAPI 4.x, `net6.0`
- Optional dependency: `Pathoschild.TractorMod`

## Install

1. Install SMAPI and Tractor Mod.
2. Extract the ZIP so you end up with `Mods/TractorSmash/TractorSmash.dll` and `manifest.json`.
3. Start the game. `config.json` is created on first run.

## Config (`config.json`)

| Option | Default | |
|---|---|---|
| `Enabled` | `true` | Master switch |
| `OnlyWhileRidingTractor` | `true` | `false` = smash on foot / any mount |
| `Radius` | `3` | Tiles around you (3 = 7×7) |
| `TicksBetweenRuns` | `8` | 60 ticks = 1 second |
| `BreakRocks`, `ClearTwigsAndWeeds`, `CutTrees`, `CutStumpsAndLogs`, `CutBushes` | `true` | Categories |
| `CutFruitTrees`, `CutTappedTrees`, `ClearGrass` | `false` | Opt-in categories |
| `WaterCrops` | `true` | Water crops in range |

When you mount, the SMAPI log prints what you're riding and whether it was detected as the tractor. If smashing doesn't start, check that line.

## Build

### On GitHub (automatic)

Every push, pull request, or manual run (Actions → *Build mod* → *Run workflow*) runs `.github/workflows/build.yml`:
checkout → SMAPI build environment → `dotnet build --configuration Release` → verify the ZIP contains `TractorSmash/TractorSmash.dll` and `manifest.json` → upload.

Download the ZIP from the run page: **Actions → the run → Artifacts** (named `TractorSmash-mod`).

### Locally

```bash
./build.sh [GAME_PATH]
```

Requires the .NET SDK. `GAME_PATH` is a folder with the Stardew Valley / SMAPI / MonoGame DLLs (optional if auto-detected). The ZIP is copied to `dist/`.

## Layout

```
.github/workflows/build.yml   CI
src/TractorSmash.csproj       project (ModBuildConfig 4.3.2)
src/manifest.json             mod ID + version (keep in sync with <Version> in the .csproj)
src/*.cs                      source
build.sh                      local build
```

`manifest.json` lives next to the project file because Pathoschild.Stardew.ModBuildConfig copies it from there into the build output and ZIP. The compiled DLL is produced by the build and is not committed.

## Before publishing

Check `UniqueID` in `src/manifest.json` (currently the placeholder `BentleyPlayz.TractorSmash`; `Author` is `BentleyPlayz`). If you change the mod ID or folder name, update the two ZIP paths checked in `build.yml` and `build.sh`.
