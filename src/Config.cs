using BepInEx.Configuration;

namespace DamageLog
{
    internal class Config
    {
        private readonly ConfigEntry<float> _entryMaxRetainTime;
        public float EntryMaxRetainTime => _entryMaxRetainTime.Value < 1 ? 1 : _entryMaxRetainTime.Value;

        private readonly ConfigEntry<int> _entryMaxCount;
        public int EntryMaxCount => _entryMaxCount.Value > 0 ? _entryMaxCount.Value : 1;

        private readonly ConfigEntry<bool> _onlyShowWithScoreboard;
        public bool OnlyShowWithScoreboard => _onlyShowWithScoreboard.Value;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("", "IDE0290")] // Use primary constructor
        public Config(ConfigFile config)
        {
            _entryMaxRetainTime = config.Bind<float>("Constraints", "entryMaxRetainTime", 10f,
                "The maximum length of time (seconds) a Damage Log entry will be retained for.");
            
            _entryMaxCount = config.Bind<int>("Constraints", "entryMaxCount", 16,
                "The (soft) maximum number of Damage Log entries to retain at a time.");

            _onlyShowWithScoreboard = config.Bind<bool>("Display", "onlyShowWithScoreboard", false,
                "Only show the Damage Log when the scoreboard is open.");
        }
    }
}
