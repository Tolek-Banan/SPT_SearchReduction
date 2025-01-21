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
                var profileFieldInfo = typeof(GClass3231).GetField("profile_0", BindingFlags.NonPublic | BindingFlags.Instance);
                var profile_0 = profileFieldInfo?.GetValue(__instance) as Profile;

                if (profile_0 == null)
                {
                    logger.LogError("Failed to access 'profile_0' field.");
                    return;
                }

                var searchControllerFieldInfo = typeof(GClass3231).GetField("iplayerSearchController_0", BindingFlags.NonPublic | BindingFlags.Instance);
                var iplayerSearchController_0 = searchControllerFieldInfo?.GetValue(__instance) as IPlayerSearchController;

                if (iplayerSearchController_0 == null)
                {
                    logger.LogError("Failed to access 'iplayerSearchController_0' field.");
                    return;
                }

                var itemFieldInfo = typeof(GClass3231).GetField("Item", BindingFlags.NonPublic | BindingFlags.Instance);
                var item = itemFieldInfo?.GetValue(__instance) as SearchableItemItemClass;

                if (item == null)
                {
                    logger.LogError("Failed to access 'Item' field.");
                    return;
                }

                var cancellationTokenSourceFieldInfo = typeof(GClass3231).GetField("cancellationTokenSource_0", BindingFlags.NonPublic | BindingFlags.Instance);
                var cancellationTokenSource_0 = cancellationTokenSourceFieldInfo?.GetValue(__instance) as CancellationTokenSource;

                if (cancellationTokenSource_0 == null)
                {
                    logger.LogError("Failed to access 'cancellationTokenSource_0' field.");
                    return;
                }

                bool bool_0 = __instance.Boolean_0;

                if (iplayerSearchController_0.ContainsUnknownItems(item))
                {
                    logger.LogInfo("Found unknown items.");

                    bool flag = item.Parent.GetOwner().RootItem is InventoryEquipment;
                    IInventoryProfileSkillInfo skillsInfo = profile_0.SkillsInfo;

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
    }
}
