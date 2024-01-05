using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using Unity.Netcode;

namespace SellTrackerFix
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class SellTrackerFixPlugin : BaseUnityPlugin
    {

        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

        public static SellTrackerFixPlugin Instance;

        internal ManualLogSource logger;


        private void Awake()
        {

            if (Instance == null)
            {
                Instance = this;
            }

            logger = BepInEx.Logging.Logger.CreateLogSource(PluginInfo.PLUGIN_GUID);

            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            harmony.PatchAll();
        }
    }

    public class Patches
    {
        [HarmonyPatch(typeof(DisplayCompanyBuyingRate))]
        public class DisplayCompanyBuyingRatePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Update")]
            public static bool OverwriteText(DisplayCompanyBuyingRate __instance)
            {
                int num = TimeOfDay.Instance.quotaFulfilled + calculatedValue;
                ((TMP_Text)__instance.displayText).fontSize = 28f;
                ((TMP_Text)__instance.displayText).text = $"PROFIT QUOTA: {num}/{TimeOfDay.Instance.profitQuota}";
                return false;
            }
        }

        [HarmonyPatch(typeof(DepositItemsDesk))]
        public class DepositItemsDeskPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("AddObjectToDeskClientRpc")]
            public static void FetchValue(DepositItemsDesk __instance)
            {
                // If not the host
                if (!NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsClient)
                {
                    // Update the itemsOnCounter variable for the client as well
                    __instance.itemsOnCounter.Add(__instance.lastObjectAddedToDesk.GetComponentInChildren<GrabbableObject>());
                }
                
                int num = 0;
                for (int i = 0; i < __instance.itemsOnCounter.Count; i++)
                {
                    if (__instance.itemsOnCounter[i].itemProperties.isScrap)
                    {
                        num += __instance.itemsOnCounter[i].scrapValue;
                    }
                }
                calculatedValue = (int)((float)num * StartOfRound.Instance.companyBuyingRate);
            }

            [HarmonyPostfix]
            [HarmonyPatch("SellItemsClientRpc")]
            public static void ClearValue(DepositItemsDesk __instance)
            {
                // If not the host
                if (!NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsClient)
                {
                    // Clear the itemsOnCounter variable for the client as well
                    __instance.itemsOnCounter.Clear();
                }

                calculatedValue = 0;
            }
        }

        public static int calculatedValue;
    }
}
