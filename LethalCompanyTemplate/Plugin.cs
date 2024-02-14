using BepInEx;
using System.IO;
using System.Reflection;
using LethalCompanyTemplate.Behaviours;
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
            
            // Crolard
            int crolardRarity = 10;
            Logger.LogInfo("Loading Crolard Item");
            Item crolard = crolard_bundle.LoadAsset<Item>("Assets/CrolardMod/Items/Scraps/crolard/CrolardItem.asset");
            CrolardBehaviour crolardBehaviour = crolard.spawnPrefab.AddComponent<CrolardBehaviour>();
            crolardBehaviour.grabbable = true;
            crolardBehaviour.grabbableToEnemies = true;
            crolardBehaviour.itemProperties = crolard;
            Logger.LogInfo("Registering Crolard as scrap");
            LethalLib.Modules.Utilities.FixMixerGroups(crolard.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(crolard.spawnPrefab);
            LethalLib.Modules.Items.RegisterScrap(crolard, crolardRarity, LethalLib.Modules.Levels.LevelTypes.All);
            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "This is crolard\n\n";
            LethalLib.Modules.Items.RegisterShopItem(crolard, null, null, node, 20);
            
            // Crolard
            int saucisseRarity = 30;
            Logger.LogInfo("Loading Saucisse Item");
            Item saucisse = crolard_bundle.LoadAsset<Item>("Assets/CrolardMod/Items/Scraps/saucisse/SaucisseItem.asset");
            SaucisseBehaviour saucisseBehaviour = saucisse.spawnPrefab.AddComponent<SaucisseBehaviour>();
            saucisseBehaviour.grabbable = true;
            saucisseBehaviour.grabbableToEnemies = true;
            saucisseBehaviour.itemProperties = saucisse;
            Logger.LogInfo("Registering Saucisse as scrap");
            LethalLib.Modules.Utilities.FixMixerGroups(saucisse.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(saucisse.spawnPrefab);
            LethalLib.Modules.Items.RegisterScrap(saucisse, saucisseRarity, LethalLib.Modules.Levels.LevelTypes.All);
            TerminalNode saucisseNode = ScriptableObject.CreateInstance<TerminalNode>();
            saucisseNode.clearPreviousText = true;
            saucisseNode.displayText = "A friend of crolard\n\n";
            LethalLib.Modules.Items.RegisterShopItem(saucisse, null, null, saucisseNode, 5);
            
            
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}