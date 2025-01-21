using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Logging;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;

namespace SearchReductionPlugin
{
    public class SearchTimePatcher
    {
        private static readonly ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("SearchReduction:Time");

        public void Enable()
        {
            var harmony = new Harmony("com.yourname.searchreductionplugin");

            var methods = new (string methodName, string prefixMethodName)[]
            {
                ("method_6", nameof(Method6Prefix))
            };

            foreach (var method in methods)
            {
                var methodToPatch = AccessTools.Method(typeof(GClass3231), method.methodName);
                var prefixMethod = typeof(SearchTimePatcher).GetMethod(method.prefixMethodName);

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
        public static async void Method6Prefix(GClass3231 __instance)
        {
            try
            {
                // Dynamic access to profile_0, iplayerSearchController_0, and other fields
                var profile_0 = GetFieldValue<Profile>(__instance, "profile_0");
                if (profile_0 == null)
                {
                    logger.LogError("Failed to access 'profile_0' field.");
                    return;
                }

                var iplayerSearchController_0 = GetFieldValue<IPlayerSearchController>(__instance, "iplayerSearchController_0");
                if (iplayerSearchController_0 == null)
                {
                    logger.LogError("Failed to access 'iplayerSearchController_0' field.");
                    return;
                }

                var item = GetFieldValue<SearchableItemItemClass>(__instance, "Item");
                if (item == null)
                {
                    logger.LogError("Failed to access 'Item' field.");
                    return;
                }

                var cancellationTokenSource_0 = GetFieldValue<CancellationTokenSource>(__instance, "cancellationTokenSource_0");
                if (cancellationTokenSource_0 == null)
                {
                    logger.LogError("Failed to access 'cancellationTokenSource_0' field.");
                    return;
                }

                bool bool_0 = __instance.Boolean_0;

                // Check for unknown items
                if (iplayerSearchController_0.ContainsUnknownItems(item))
                {
                    logger.LogInfo("Found unknown items.");

                    bool flag = item.Parent.GetOwner().RootItem is InventoryEquipment;
                    IInventoryProfileSkillInfo skillsInfo = profile_0.SkillsInfo;

                    // Calculating delay factor based on the profile and item class
                    float num = (flag ? (1f + skillsInfo.AttentionLootSpeedValue + skillsInfo.SearchBuffSpeedValue) : (1f + skillsInfo.AttentionLootSpeedValue));
                    logger.LogInfo($"Calculated delay factor: {num}");

                    Item foundItem;
                    while (__instance.method_7(out foundItem))
                    {
                        if (foundItem == null)
                        {
                            logger.LogError("Failed to find an item in 'method_7'.");
                            break;
                        }

                        float delayMultiplier = SearchReductionPlugin.SearchTimeMultiplier.Value;
                        float randomizedDelay = UnityEngine.Random.Range(1, 3) / num;
                        float delayInMilliseconds = (bool_0 ? 0f : randomizedDelay) * 1000f * delayMultiplier;

                        logger.LogInfo($"Item delay set to: {delayInMilliseconds} ms (time multiplier: {delayMultiplier})");

                        try
                        {
                            await Task.Delay((int)delayInMilliseconds, cancellationTokenSource_0.Token);
                        }
                        catch (TaskCanceledException cancelEx)
                        {
                            logger.LogWarning($"Task was canceled. {cancelEx.Message}");
                            break;
                        }
                        catch (Exception ex)
                        {
                            logger.LogError($"Error during delay: {ex.GetType()} - {ex.Message}\n{ex.StackTrace}");
                            break;
                        }

                        if (__instance.Boolean_0)
                        {
                            logger.LogInfo("Task was canceled.");
                            break;
                        }

                        string itemId = "some_id";
                        ItemTemplate itemTemplate = new ItemTemplate();
                        Item newItem = new Item(itemId, itemTemplate);

                        logger.LogInfo($"Processing new item.");

                        Item nextItem;
                        if (!__instance.method_7(out nextItem))
                        {
                            logger.LogError("Failed to retrieve the next item.");
                            break;
                        }

                        __instance.DiscoverItem(nextItem);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in Method6Prefix: {ex.GetType()} - {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                logger.LogInfo("End of Method6Prefix.");
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
