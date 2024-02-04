using BepInEx.Configuration;

namespace DamageLog
{
    internal static class ConfigUtility {
        public static ConfigEntry<string> BindInputKey(this ConfigFile config, string section, string key, string defaultInputKey, string description) {
            const string docs = "\nKey names follow the naming conventions outlined at: https://docs.unity3d.com/2019.4/Documentation/Manual/class-InputManager.html#:~:text=Key%20family";

            ConfigEntry<string> entry = config.Bind<string>(section, key, defaultInputKey, description + docs);

            try {
                UnityEngine.Input.GetKeyDown(entry.Value);
            }
            catch (System.ArgumentException) {
                Log.Warning($"Config> {section}.{key} | '{entry.Value}' is not a valid input key string, using '{defaultInputKey}' instead.");
                entry.Value = defaultInputKey;
            }

            return entry;
        }
    }

    internal sealed class Config
    {
        // Constraints
        private readonly ConfigEntry<float> entryMaxRetainTime;
        public float EntryMaxRetainTime => entryMaxRetainTime.Value < 1 ? 1 : entryMaxRetainTime.Value;

        private readonly ConfigEntry<int> entryMaxCount;
        public int EntryMaxCount => entryMaxCount.Value > 0 ? entryMaxCount.Value : 1;

        // Display
        private readonly ConfigEntry<bool> onlyShowWithScoreboard;
        public bool OnlyShowWithScoreboard => onlyShowWithScoreboard.Value;

        private readonly ConfigEntry<bool> showDamageIdentifier;
        public bool ShowIdentifier => showDamageIdentifier.Value;

        private readonly ConfigEntry<bool> useSimpleTextMode;
        public bool SimpleTextMode => useSimpleTextMode.Value;

        // Controls
        private readonly ConfigEntry<string> cycleUserKey;
        public string CycleUserKey => cycleUserKey.Value;


        public Config(ConfigFile config)
        {
            const string Constraints = "Constraints";
            entryMaxRetainTime = config.Bind<float>(Constraints, nameof(entryMaxRetainTime), 10f,
                "The maximum length of time (seconds) a Damage Log entry will be retained for.\nMinimum is 1.");
            
            entryMaxCount = config.Bind<int>(Constraints, nameof(entryMaxCount), 16,
                "The (soft) maximum number of Damage Log entries to retain at a time.\nMinimum is 1.");


            const string Display = "Display";
            onlyShowWithScoreboard = config.Bind<bool>(Display, nameof(onlyShowWithScoreboard), false,
                "Only show the Damage Log when the scoreboard is open.");

            showDamageIdentifier = config.Bind<bool>(Display, nameof(showDamageIdentifier), false,
                "Show damage identifier in tooltip.");

            useSimpleTextMode = config.Bind<bool>(Display, nameof(useSimpleTextMode), false,
                "Display Damage Log entries as text instead of portraits with tooltips.");


            const string Controls = "Controls";
            cycleUserKey = config.BindInputKey(Controls, nameof(cycleUserKey), "left alt",
                "The key to use to cycle which user's Damage Log should be shown.");




            const string Debug = "m_Debug";
            textSize = config.Bind<float>(Debug, nameof(textSize), 12);
            portraitSpacing = config.Bind<float>(Debug, nameof(portraitSpacing), 6);
            portraitSize = config.Bind<float>(Debug, nameof(portraitSize), 78);
            eliteIconSize = config.Bind<float>(Debug, nameof(eliteIconSize), 24);
            portraitTextSize = config.Bind<float>(Debug, nameof(portraitTextSize), 18);
            damageTextSize = config.Bind<float>(Debug, nameof(damageTextSize), 20);
#if DEBUG
            changeStageKey = config.Bind<string>(Debug, nameof(changeStageKey), "right alt");
#endif
        }


        private readonly ConfigEntry<float> textSize;
        public float TextSize => textSize.Value;

        private readonly ConfigEntry<float> portraitSpacing;
        public float Spacing => portraitSpacing.Value;

        private readonly ConfigEntry<float> portraitSize;
        public float PortraitSize => portraitSize.Value;

        private readonly ConfigEntry<float> eliteIconSize;
        public float EliteIconSize => eliteIconSize.Value;

        private readonly ConfigEntry<float> portraitTextSize;
        public float PortraitTextSize => portraitTextSize.Value;

        private readonly ConfigEntry<float> damageTextSize;
        public float DamageTextSize => damageTextSize.Value;

#if DEBUG
        private readonly ConfigEntry<string> changeStageKey;
        public string ChangeStageKey => changeStageKey.Value;
#endif
    }
}
