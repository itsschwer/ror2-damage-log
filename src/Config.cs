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
                Plugin.Logger.LogWarning($"{nameof(Config)}> {section}.{key} | '{entry.Value}' is not a valid input key string, using '{defaultInputKey}' instead.");
                entry.Value = defaultInputKey;
            }

            return entry;
        }
    }

    internal sealed class Config
    {
        private readonly ConfigFile file;
        internal void Reload() { Plugin.Logger.LogDebug($"Reloading {file.ConfigFilePath.Substring(file.ConfigFilePath.LastIndexOf(System.IO.Path.DirectorySeparatorChar)+1)}"); file.Reload(); }


        // Bosses
        private readonly ConfigEntry<bool> trackBosses;
        public bool TrackBosses => trackBosses.Value;

        // Constraints
        private readonly ConfigEntry<float> entryMaxRetainTime;
        public float EntryMaxRetainTime => entryMaxRetainTime.Value < 1 ? 1 : entryMaxRetainTime.Value;

        // Display
        private readonly ConfigEntry<bool> onlyShowWithScoreboard;
        public bool OnlyShowWithScoreboard => onlyShowWithScoreboard.Value;

        private readonly ConfigEntry<bool> useSimpleTextMode;
        public bool SimpleTextMode => useSimpleTextMode.Value;

        private readonly ConfigEntry<bool> hideDamageTimer;
        public bool HideDamageTimer => hideDamageTimer.Value;

        // Display: Portraits Mode
        private readonly ConfigEntry<int> maximumPortraitCount;
        public int MaximumPortraitCount => maximumPortraitCount.Value > 0 ? maximumPortraitCount.Value : 1;

        private readonly ConfigEntry<bool> showDamageIdentifier;
        public bool ShowIdentifier => showDamageIdentifier.Value;

        // Display: Simple Text Mode
        private readonly ConfigEntry<bool> compactLines;
        public bool CompactLines => compactLines.Value;

        private readonly ConfigEntry<bool> showRawDamageInsteadOfPercentage;
        public bool ShowRawDamageInsteadOfPercentage => showRawDamageInsteadOfPercentage.Value;

        // Controls
        private readonly ConfigEntry<string> cycleUserKey;
        public string CycleUserKey => cycleUserKey.Value;

        private readonly ConfigEntry<string> cycleBossKey;
        public string CycleBossKey => cycleBossKey.Value;


        internal Config(ConfigFile config)
        {
            file = config;

            trackBosses = config.Bind<bool>("Bosses", nameof(trackBosses), false,
                "Generate Damage Logs for bosses. Use " + nameof(cycleBossKey) + " to display these in the UI.");


            const string Constraints = "Constraints";
            entryMaxRetainTime = config.Bind<float>(Constraints, nameof(entryMaxRetainTime), 10f,
                "The maximum length of time (seconds) a Damage Log entry will be retained for.\nMinimum is 1.");


            const string Display = "Display";
            onlyShowWithScoreboard = config.Bind<bool>(Display, nameof(onlyShowWithScoreboard), false,
                "Only show the Damage Log when the scoreboard is open.");

            useSimpleTextMode = config.Bind<bool>(Display, nameof(useSimpleTextMode), false,
                "Display Damage Log entries as text instead of portraits with tooltips.");

            hideDamageTimer = config.Bind<bool>(Display, nameof(hideDamageTimer), false,
                "Hide the timer portion of each Damage Log entry showing how long it has been since the damage was inflicted.");


            const string PortraitsMode = "Display: Portraits Mode";
            maximumPortraitCount = config.Bind<int>(PortraitsMode, nameof(maximumPortraitCount), 12,
                "The maximum number of Damage Log entry portraits to show at a time.\nMinimum is 1.");

            showDamageIdentifier = config.Bind<bool>(PortraitsMode, nameof(showDamageIdentifier), false,
                "Show the damage identifier in the portrait tooltip. Can show additional information about damage attributed to The Planet.");


            const string SimpleTextMode = "Display: Simple Text Mode";
            compactLines = config.Bind<bool>(SimpleTextMode, nameof(compactLines), false,
                "Remove empty lines used as separators between Damage Log entries.");

            showRawDamageInsteadOfPercentage = config.Bind<bool>(SimpleTextMode, nameof(showRawDamageInsteadOfPercentage), false,
                "Show the raw damage value instead of the percentage of full combined health.");


            const string Controls = "Controls"; // Could use BepInEx.Configuration.KeyboardShortcut,
                                                // but that prevents ability to press while moving and
                                                // requires separate binds to allow for holding shift to cycle in reverse.
                                                // KeyCode appears buggy, with Risk of Options showing the wrong option for some reason?
                                                //     (e.g. RoO "Backquote" becomes "Minus" in game and in text)
                                                //     (e.g. text and game "Backquote" becomes "RightArrow" in RoO)
                                                // Changing Unity package reference didn't seem to help.
            cycleUserKey = config.BindInputKey(Controls, nameof(cycleUserKey), "left alt",
                "The key to use to cycle which user's Damage Log should be shown.");

            cycleBossKey = config.BindInputKey(Controls, nameof(cycleBossKey), "`",
                "The key to use to cycle which boss's Damage Log should be shown.");



            const string Debug = "m_Debug";
            canvasOffsetRight= config.Bind<float>(Debug, nameof(canvasOffsetRight), 8);
            canvasOffsetTop = config.Bind<float>(Debug, nameof(canvasOffsetTop), 12);
            canvasWidth = config.Bind<float>(Debug, nameof(canvasWidth), 88);
            textSize = config.Bind<float>(Debug, nameof(textSize), 14);
            portraitSpacing = config.Bind<float>(Debug, nameof(portraitSpacing), 8);
            portraitSize = config.Bind<float>(Debug, nameof(portraitSize), 78);
            eliteIconSize = config.Bind<float>(Debug, nameof(eliteIconSize), 24);
            portraitTextSize = config.Bind<float>(Debug, nameof(portraitTextSize), 18);
            damageTextSize = config.Bind<float>(Debug, nameof(damageTextSize), 20);
        }


        private readonly ConfigEntry<float> canvasOffsetRight;
        private readonly ConfigEntry<float> canvasOffsetTop;
        public UnityEngine.Vector2 CanvasOffsetTopRight => new UnityEngine.Vector2(canvasOffsetRight.Value, canvasOffsetTop.Value);

        private readonly ConfigEntry<float> canvasWidth;
        public float CanvasWidth => canvasWidth.Value;

        private readonly ConfigEntry<float> textSize;
        public float TextSize => textSize.Value;

        private readonly ConfigEntry<float> portraitSpacing;
        public float PortraitSpacing => portraitSpacing.Value;

        private readonly ConfigEntry<float> portraitSize;
        public float PortraitSize => portraitSize.Value;

        private readonly ConfigEntry<float> eliteIconSize;
        public float EliteIconSize => eliteIconSize.Value;

        private readonly ConfigEntry<float> portraitTextSize;
        public float PortraitTextSize => portraitTextSize.Value;

        private readonly ConfigEntry<float> damageTextSize;
        public float DamageTextSize => damageTextSize.Value;
    }
}
