using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace TractorSmash
{
    /// <summary>
    /// Companion for Pathoschild's Tractor Mod. While you ride the tractor, everything in range
    /// (rocks, trees, stumps, twigs, weeds, bushes...) is smashed and crops are watered, without
    /// holding any tool. Tools in your hands still work normally (hoe, seeds, etc. via Tractor Mod).
    ///
    /// It never touches tilled soil or crops (other than watering), placed objects, or chests.
    /// </summary>
    public class ModEntry : Mod
    {
        private ModConfig Config = new ModConfig();

        // Hidden iridium-level tools. They are never in your inventory.
        private Axe FakeAxe;
        private Pickaxe FakePickaxe;

        private string LastMountDescription;

        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.ReturnedToTitle += (s, e) => { this.FakeAxe = null; this.FakePickaxe = null; };
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!this.Config.Enabled || !Context.IsWorldReady)
                return;
            if (!e.IsMultipleOf((uint)Math.Max(1, this.Config.TicksBetweenRuns)))
                return;

            Farmer who = Game1.player;
            if (who?.currentLocation == null || Game1.activeClickableMenu != null || Game1.eventUp)
                return;

            this.LogMountChange(who);

            if (this.Config.OnlyWhileRidingTractor && !this.IsRidingTractor(who))
                return;

            this.FakeAxe ??= new Axe { UpgradeLevel = 4 };
            this.FakePickaxe ??= new Pickaxe { UpgradeLevel = 4 };

            GameLocation location = who.currentLocation;
            Vector2 center = who.Tile;
            int r = Math.Max(0, this.Config.Radius);

            // The game's tool code drains stamina; remember it and put it back so smashing is free.
            float stamina = who.Stamina;
            try
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = -r; dy <= r; dy++)
                        this.ProcessTile(location, who, new Vector2(center.X + dx, center.Y + dy));
                }
            }
            catch (Exception ex)
            {
                this.Monitor.LogOnce($"Smashing failed (this message shows once): {ex}", LogLevel.Error);
            }
            finally
            {
                who.Stamina = stamina;
            }
        }

        private void ProcessTile(GameLocation location, Farmer who, Vector2 tile)
        {
            int tx = (int)tile.X;
            int ty = (int)tile.Y;
            int px = tx * 64 + 32;
            int py = ty * 64 + 32;

            location.terrainFeatures.TryGetValue(tile, out TerrainFeature feature);

            // Tilled soil: only water it. Never use a pickaxe/axe on these tiles (it would undo the tilling).
            if (feature is HoeDirt dirt)
            {
                if (this.Config.WaterCrops && dirt.crop != null && dirt.state.Value != HoeDirt.watered)
                    dirt.state.Value = HoeDirt.watered;
                return;
            }

            bool useAxe = false;
            bool usePickaxe = false;

            // objects: stones, twigs, weeds only
            if (location.Objects.TryGetValue(tile, out SObject obj))
            {
                if (this.Config.BreakRocks && obj.IsBreakableStone())
                    usePickaxe = true;
                else if (this.Config.ClearTwigsAndWeeds && (obj.IsTwig() || obj.IsWeeds()))
                    useAxe = true;
            }

            // terrain features
            if (feature is Tree tree)
            {
                if (this.Config.CutTrees && (!tree.tapped.Value || this.Config.CutTappedTrees))
                    useAxe = true;
            }
            else if (feature is FruitTree)
            {
                if (this.Config.CutFruitTrees)
                    useAxe = true;
            }
            else if (feature is Grass)
            {
                if (this.Config.ClearGrass)
                    location.terrainFeatures.Remove(tile);
            }

            // resource clumps: stumps, logs, boulders, meteorites
            foreach (ResourceClump clump in location.resourceClumps)
            {
                if (!clump.occupiesTile(tx, ty))
                    continue;

                int id = clump.parentSheetIndex.Value;
                bool isWood = id == ResourceClump.stumpIndex || id == ResourceClump.hollowLogIndex;
                if (isWood && this.Config.CutStumpsAndLogs)
                    useAxe = true;
                else if (!isWood && this.Config.BreakRocks)
                    usePickaxe = true;
                break;
            }

            // bushes (large terrain features); skip planted tea bushes
            if (this.Config.CutBushes)
            {
                foreach (LargeTerrainFeature large in location.largeTerrainFeatures)
                {
                    if (large is Bush bush && bush.size.Value != Bush.greenTeaBush && bush.getBoundingBox().Contains(px, py))
                    {
                        useAxe = true;
                        break;
                    }
                }
            }

            if (useAxe)
                this.FakeAxe.DoFunction(location, px, py, 0, who);
            if (usePickaxe)
                this.FakePickaxe.DoFunction(location, px, py, 0, who);
        }

        /// <summary>Whether the player is riding the Tractor Mod tractor (it's a horse with a tractor-ish name/data).</summary>
        private bool IsRidingTractor(Farmer who)
        {
            Horse horse = who.mount;
            if (horse == null)
                return false;

            if ((horse.Name ?? "").IndexOf("tractor", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return horse.modData.Keys.Any(key => key.IndexOf("tractor", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>Log what you're riding whenever it changes, so detection problems are easy to diagnose from the SMAPI log.</summary>
        private void LogMountChange(Farmer who)
        {
            Horse horse = who.mount;
            string description = horse == null
                ? null
                : $"name '{horse.Name}', modData keys [{string.Join(", ", horse.modData.Keys)}], tractor detected: {this.IsRidingTractor(who)}";

            if (description != this.LastMountDescription)
            {
                this.LastMountDescription = description;
                if (description != null)
                    this.Monitor.Log($"Mounted: {description}", LogLevel.Info);
            }
        }
    }
}
