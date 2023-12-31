using BepInEx.Configuration;

namespace DamageLog
{
    internal sealed class Config
    {
        // Constraints
        private readonly ConfigEntry<float> _entryMaxRetainTime;
        public float EntryMaxRetainTime => _entryMaxRetainTime.Value < 1 ? 1 : _entryMaxRetainTime.Value;

        private readonly ConfigEntry<int> _entryMaxCount;
        public int EntryMaxCount => _entryMaxCount.Value > 0 ? _entryMaxCount.Value : 1;

        // Display
        private readonly ConfigEntry<bool> _onlyShowWithScoreboard;
        public bool OnlyShowWithScoreboard => _onlyShowWithScoreboard.Value;

        private readonly ConfigEntry<bool> _simpleTextMode;
        public bool SimpleTextMode => _simpleTextMode.Value;

        // Controls
        private readonly ConfigEntry<string> _cycleUserKey;
        public string CycleUserKey => _cycleUserKey.Value;


        public Config(ConfigFile config)
        {
            const string Constraints = "Constraints";
            _entryMaxRetainTime = config.Bind<float>(Constraints, "entryMaxRetainTime", 10f,
                "The maximum length of time (seconds) a Damage Log entry will be retained for.\nMinimum is 1.");
            
            _entryMaxCount = config.Bind<int>(Constraints, "entryMaxCount", 16,
                "The (soft) maximum number of Damage Log entries to retain at a time.\nMinimum is 1.");


            const string Display = "Display";
            _onlyShowWithScoreboard = config.Bind<bool>(Display, "onlyShowWithScoreboard", false,
                "Only show the Damage Log when the scoreboard is open.");

            _simpleTextMode = config.Bind<bool>(Display, "useSimpleTextMode", false,
                "Display Damage Log entries as text instead of portraits with tooltips.");


            const string Controls = "Controls";
            _cycleUserKey = config.Bind<string>(Controls, "cycleUserKey", "left alt",
                "The key to use to cycle which user's Damage Log should be shown.\nKey names follow the naming conventions outlined at: https://docs.unity3d.com/2019.4/Documentation/Manual/class-InputManager.html#:~:text=Key%20family");




            const string Debug = "m_Debug";
            _textSize = config.Bind<float>(Debug, "textModeFontSize", 12);

            _spacing = config.Bind<float>(Debug, "portraitSpacing", 12);
            _portraitSize = config.Bind<float>(Debug, "portraitSize", -1);
            _eliteIconSize = config.Bind<float>(Debug, "eliteIconSize", 32f);
            _portraitTextSize = config.Bind<float>(Debug, "portraitTextSize", 18);
            _damageTextSize = config.Bind<float>(Debug, "damageTextSize", 20);

            _changeStageKey = config.Bind<string>(Debug, "changeStageKey", "right alt");
        }


        private readonly ConfigEntry<float> _textSize;
        public float TextSize => _textSize.Value;


        private readonly ConfigEntry<float> _spacing;
        public float Spacing => _spacing.Value;

        private readonly ConfigEntry<float> _portraitSize;
        public float PortraitSize => _portraitSize.Value;

        private readonly ConfigEntry<float> _eliteIconSize;
        public float EliteIconSize => _eliteIconSize.Value;

        private readonly ConfigEntry<float> _portraitTextSize;
        public float PortraitTextSize => _portraitTextSize.Value;

        private readonly ConfigEntry<float> _damageTextSize;
        public float DamageTextSize => _damageTextSize.Value;


        private readonly ConfigEntry<string> _changeStageKey;
        public string ChangeStageKey => _changeStageKey.Value;
    }
}
