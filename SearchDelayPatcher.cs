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
                // Zastosowanie metody GetFieldValue w celu uzyskania wartości pól
                var bool_1 = GetFieldValue<bool>(__instance, "bool_1");
                var bool_0 = GetFieldValue<bool>(__instance, "bool_0");
                var cancellationTokenSource = GetFieldValue<CancellationTokenSource>(__instance, "cancellationTokenSource_0");

                if (bool_1)
                {
                    if (!bool_0)
                    {
                        if (cancellationTokenSource.IsCancellationRequested)
                        {
                            logger.LogInfo("The operation was canceled before the delay started.");
                            return;
                        }

                        // Obliczenie opóźnienia, używając mnożnika
                        var delay = (int)(2000 * SearchReductionPlugin.SearchDelayMultiplier.Value);
                        logger.LogInfo($"Delay set to: {delay} ms (delay multiplier: {SearchReductionPlugin.SearchDelayMultiplier.Value})");
                        await Task.Delay(delay, cancellationTokenSource.Token);
                        logger.LogInfo("Delay finished, continuing search.");
                    }

                    // Kontynuowanie wyszukiwania przedmiotu
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

        // Generic method to get field value dynamically
        private static T GetFieldValue<T>(object instance, string fieldName)
        {
            try
            {
                var fieldInfo = typeof(GClass3231).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    return (T)fieldInfo.GetValue(instance);
                }
                else
                {
                    logger.LogWarning($"Field '{fieldName}' not found.");
                    return default;
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error accessing field '{fieldName}': {ex.GetType()} - {ex.Message}");
                return default;
            }
        }
    }
}
