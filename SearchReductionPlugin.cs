using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using JetBrains.Annotations;

namespace SearchReductionPlugin
{
    [BepInPlugin("com.yourname.searchreductionplugin", "SearchReduction", "1.0.0")]
    public class SearchReductionPlugin : BaseUnityPlugin
    {
        private static readonly ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("SearchReduction:Plugin");
        internal static ConfigEntry<float> SearchDelayMultiplier { get; set; }
        internal static ConfigEntry<float> SearchTimeMultiplier { get; set; }

        [UsedImplicitly]
        private void Start()
        {
            SearchDelayMultiplier = Config.Bind("Settings", "Search Delay Multiplier", 0.5f,
                new ConfigDescription(
                    "Delay after container opening",
                    new AcceptableValueRange<float>(0.0f, 1.0f),
                    new ConfigurationManagerAttributes
                    {
                        ShowRangeAsPercent = false
                    }
                )
            );

            SearchTimeMultiplier = Config.Bind("Settings", "Search Time Multiplier", 0.5f,
                new ConfigDescription(
                    "Item search time",
                    new AcceptableValueRange<float>(0.0f, 1.0f),
                    new ConfigurationManagerAttributes
                    {
                        ShowRangeAsPercent = false
                    }
                )
            );

            logger.LogInfo($"Search Delay set to: {SearchDelayMultiplier.Value}");
            logger.LogInfo($"Search Time set to: {SearchTimeMultiplier.Value}");
            
                new SearchDelayPatcher().Enable();
                new SearchTimePatcher().Enable();
            }
        }
    }
