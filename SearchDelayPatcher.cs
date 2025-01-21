using HarmonyLib;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Logging;

namespace SearchReductionPlugin
{
    public class SearchDelayPatcher
    {
        private static readonly ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("SearchReduction:Delay");

        private static readonly FieldInfo bool1Field = typeof(GClass3231).GetField("bool_1", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo bool0Field = typeof(GClass3231).GetField("bool_0", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo cancellationTokenSourceField = typeof(GClass3231).GetField("cancellationTokenSource_0", BindingFlags.NonPublic | BindingFlags.Instance);

        public void Enable()
        {
            var harmony = new Harmony("com.yourname.searchreductionplugin");

            var methods = new (string methodName, string prefixMethodName)[]
            {
                ("method_5", nameof(Method5Prefix))
            };

            foreach (var method in methods)
            {
                var methodToPatch = AccessTools.Method(typeof(GClass3231), method.methodName);
                var prefixMethod = typeof(SearchDelayPatcher).GetMethod(method.prefixMethodName);

                if (methodToPatch != null && prefixMethod != null)
                {
                    harmony.Patch(methodToPatch, new HarmonyMethod(prefixMethod));
                    logger.LogInfo($"Patched {method.methodName}.");
                }
                else
                {
                    logger.LogWarning($"Method not found: {method.methodName} or its prefix.");
                }
            }
        }

        [HarmonyPrefix]
        public static async void Method5Prefix(GClass3231 __instance)
        {
            try
            {
                var bool_1 = (bool)bool1Field.GetValue(__instance);
                var bool_0 = (bool)bool0Field.GetValue(__instance);
                var cancellationTokenSource = (CancellationTokenSource)cancellationTokenSourceField.GetValue(__instance);

                if (bool_1)
                {
                    if (!bool_0)
                    {
                        if (cancellationTokenSource.IsCancellationRequested)
                        {
                            logger.LogInfo("The operation was canceled before the delay started.");
                            return;
                        }

                        var delay = (int)(2000 * SearchReductionPlugin.SearchDelayMultiplier.Value);
                        logger.LogInfo($"Delay set to: {delay} ms (delay multiplier: {SearchReductionPlugin.SearchDelayMultiplier.Value})");
                        await Task.Delay(delay, cancellationTokenSource.Token);
                        logger.LogInfo("Delay finished, continuing search.");
                    }

                    __instance.SearchItem();
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInfo("The task was canceled.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in Method5Prefix: {ex.GetType()} - {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                logger.LogInfo("End of Method5Prefix.");
            }
        }
    }
}
