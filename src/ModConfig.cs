namespace TractorSmash
{
    /// <summary>Settings in config.json (created automatically on first run).</summary>
    public class ModConfig
    {
        /// <summary>Master switch.</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Only smash while riding the Tractor Mod tractor. Set false to smash on foot/any mount too.</summary>
        public bool OnlyWhileRidingTractor { get; set; } = true;

        /// <summary>How many tiles around you are affected (3 = a 7x7 area).</summary>
        public int Radius { get; set; } = 3;

        /// <summary>Game ticks between smashes (60 ticks = 1 second). Lower = faster but louder.</summary>
        public int TicksBetweenRuns { get; set; } = 8;

        /// <summary>Stones, ore nodes, boulders, meteorites.</summary>
        public bool BreakRocks { get; set; } = true;

        /// <summary>Twigs and weeds.</summary>
        public bool ClearTwigsAndWeeds { get; set; } = true;

        /// <summary>Wild trees, including saplings.</summary>
        public bool CutTrees { get; set; } = true;

        /// <summary>Large stumps and logs.</summary>
        public bool CutStumpsAndLogs { get; set; } = true;

        /// <summary>Bushes (never tea bushes).</summary>
        public bool CutBushes { get; set; } = true;

        /// <summary>Fruit trees. OFF by default so you don't wipe out an orchard.</summary>
        public bool CutFruitTrees { get; set; } = false;

        /// <summary>Trees with a tapper on them. OFF by default.</summary>
        public bool CutTappedTrees { get; set; } = false;

        /// <summary>Remove grass. OFF by default (grass is where hay comes from).</summary>
        public bool ClearGrass { get; set; } = false;

        /// <summary>Automatically water every crop in range, no watering can needed.</summary>
        public bool WaterCrops { get; set; } = true;
    }
}
