using BepInEx;
using System.IO;
using System.Reflection;
using LethalCompanyTemplate.Behaviours;
using LethalLib.Modules;
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
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
            
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} loaded netcode patcher !");
            
            
            
            
            string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            crolard_bundle = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "crolard"));
            if (crolard_bundle == null)
            {
                Logger.LogError("Failed to load custom assets.");
                return;
            }
            
            // Crolard
            int crolardRarity = 3;
            Logger.LogInfo("Loading Crolard Item");
            Item crolard = crolard_bundle.LoadAsset<Item>("Assets/CrolardMod/Items/Scraps/crolard/CrolardItem.asset");
            CrolardBehaviour crolardBehaviour = crolard.spawnPrefab.AddComponent<CrolardBehaviour>();
            crolardBehaviour.grabbable = true;
            crolardBehaviour.grabbableToEnemies = true;
            crolardBehaviour.itemProperties = crolard;
            crolardBehaviour.ganjaCrolard = crolard_bundle.LoadAsset<AudioClip>("Assets/CrolardMod/Items/Scraps/crolard/MoaningFar.mp3");
            crolardBehaviour.ganjaNoiseLow = crolard_bundle.LoadAsset<AudioClip>("Assets/CrolardMod/Items/Scraps/crolard/crilow.mp3");
            crolardBehaviour.ganjaNoiseMedium = crolard_bundle.LoadAsset<AudioClip>("Assets/CrolardMod/Items/Scraps/crolard/crimid.mp3");
            crolardBehaviour.ganjaNoiseHigh= crolard_bundle.LoadAsset<AudioClip>("Assets/CrolardMod/Items/Scraps/crolard/crihigh.mp3");
            crolardBehaviour.ganjaExplosion = crolard_bundle.LoadAsset<AudioClip>("Assets/CrolardMod/Items/Scraps/crolard/explosion.mp3");
            
            Logger.LogInfo("Registering Crolard as scrap");
            LethalLib.Modules.Utilities.FixMixerGroups(crolard.spawnPrefab);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(crolard.spawnPrefab);
            LethalLib.Modules.Items.RegisterScrap(crolard, crolardRarity, LethalLib.Modules.Levels.LevelTypes.All);
            
            // Saucisse
            int saucisseRarity = 20;
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
            
            /*
            // TODO : remove before prod
            TerminalNode cNode = ScriptableObject.CreateInstance<TerminalNode>();
            cNode.displayText = "RTandom info";
            cNode.clearPreviousText = true;
            Items.RegisterShopItem(crolard, null, null, cNode, 1);
            
            TerminalNode sNode = ScriptableObject.CreateInstance<TerminalNode>();
            sNode.displayText = "RTandom info";
            sNode.clearPreviousText = true;
            Items.RegisterShopItem(saucisse, null, null, sNode, 1);
            */
            
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}