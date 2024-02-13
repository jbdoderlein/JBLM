using BepInEx;
using System.IO;
using System.Reflection;
using Unity.Audio;
using UnityEngine;

namespace LethalCompanyTemplate
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(LethalLib.Plugin.ModGUID)]
    public class Plugin : BaseUnityPlugin
    {
        public static AssetBundle crolard_bundle;
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} start loading!");
            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            crolard_bundle = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "crolard"));
            if (crolard_bundle == null)
            {
                Logger.LogError("Failed to load custom assets.");
                return;
            }

            int iRarity = 10;

            Logger.LogInfo("Loading Crolard Item");
            Item crolard = crolard_bundle.LoadAsset<Item>("Assets/CrolardMod/Items/Scraps/crolard/CrolardItem.asset");
            
            Logger.LogInfo("Registering Crolard as scrap");
            LethalLib.Modules.Utilities.FixMixerGroups(crolard.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(crolard.spawnPrefab);
            LethalLib.Modules.Items.RegisterScrap(crolard, iRarity, LethalLib.Modules.Levels.LevelTypes.All);

            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "This is crolard\n\n";
            LethalLib.Modules.Items.RegisterShopItem(crolard, null, null, node, 20);
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}