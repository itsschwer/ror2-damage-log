using BepInEx.Configuration;

namespace DamageLog
{
    internal sealed class Config
    {
        private readonly ConfigEntry<float> _entryMaxRetainTime;
        public float EntryMaxRetainTime => _entryMaxRetainTime.Value < 1 ? 1 : _entryMaxRetainTime.Value;

        private readonly ConfigEntry<int> _entryMaxCount;
        public int EntryMaxCount => _entryMaxCount.Value > 0 ? _entryMaxCount.Value : 1;

        private readonly ConfigEntry<bool> _onlyShowWithScoreboard;
        public bool OnlyShowWithScoreboard => _onlyShowWithScoreboard.Value;

        private readonly ConfigEntry<string> _cycleUserKey;
        public string CycleUserKey => _cycleUserKey.Value;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Preference")]
        public Config(ConfigFile config)
        {
            _entryMaxRetainTime = config.Bind<float>("Constraints", "entryMaxRetainTime", 10f,
                "The maximum length of time (seconds) a Damage Log entry will be retained for.\nMinimum is 1.");
            
            _entryMaxCount = config.Bind<int>("Constraints", "entryMaxCount", 16,
                "The (soft) maximum number of Damage Log entries to retain at a time.\nMinimum is 1.");

            _onlyShowWithScoreboard = config.Bind<bool>("Display", "onlyShowWithScoreboard", false,
                "Only show the Damage Log when the scoreboard is open.");

            _cycleUserKey = config.Bind<string>("Controls", "cycleUserKey", "left alt",
                "The key to use to cycle which user's Damage Log should be shown.\nKey names follow the naming conventions outlined at: https://docs.unity3d.com/2019.4/Documentation/Manual/class-InputManager.html#:~:text=Key%20family");
        }
    }
}
