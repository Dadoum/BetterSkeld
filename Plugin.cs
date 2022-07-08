using System;
using System.Linq;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using TMPro;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace BetterSkeld
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class BetterSkeldPlugin : BasePlugin
    {
        public static BetterSkeldPlugin Instance { get; private set; }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
        public static class ShipStatusAwakePatch
        {
            public static void Prefix(ShipStatus __instance)
            {
                var patcher = new GameObject("BetterSkeld Patcher");
                switch (__instance.Type)
                {
                    case ShipStatus.MapType.Ship:
                        patcher.AddComponent<SkeldPatcher>();
                        break;
                    case ShipStatus.MapType.Hq:
                        patcher.AddComponent<MiraHQPatcher>();
                        break;
                }
            }
        }

        public class SkeldPatcher : MonoBehaviour
        {
            private void FixedUpdate()
            {
                var client = AmongUsClient.Instance;
                
                // On charge Polus 
                var res = Addressables.LoadAssetAsync<GameObject>(client.ShipPrefabs[(Index) (int) ShipStatus.MapType.Pb]).Result;
                if (!res)
                    return; // On réessaiera de charger à la prochaine update
                
                Instance.Log.LogDebug("Patching TheSkeld...");
                
                // Récup les ressources
                var vitalsObj = res.transform.Find("Office/panel_vitals").gameObject;
                var admin = GameObject.Find("MapRoomConsole");
                var animation = GameObject.Find("MapAnimation");
                var impostorDetectors = GameObject.FindObjectsOfType<ImpostorDetector>();
                
                // Ajouter les signaux vitaux
                Instance.Log.LogDebug("Placing vitals...");
                var vitals = GameObject.Instantiate(vitalsObj);
                vitals.transform.position = new Vector3(1.9162f, -16.1985f, -2.4142f);
                vitals.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                vitals.transform.localScale = new Vector3(0.6636f, 0.7418f, 1f);
                
                // Désactiver l'admin
                Instance.Log.LogDebug("Disabling admin...");
                admin.GetComponent<CircleCollider2D>().enabled = false;
                
                // Désactiver l'animation car il n'y a plus de carte, donc pour le RP faut plus qu'on la voie
                animation.active = false;
                
                // HACK: je sais pas trop à quoi il sert en temps normal, mais ce composant cause le bug de glissement
                // à la tâche des feuilles !
                Instance.Log.LogDebug("Fixing \"Clean O₂ Filter\" task...");
                foreach (var impostorDetector in impostorDetectors)
                {
                    impostorDetector.GetComponent<CircleCollider2D>().enabled = false;
                }
                
                // Relier toutes les vents à gauche, et toutes celles à droite
                Instance.Log.LogDebug("Rerouting vents...");
                // TODO: faire ça après
                
                Instance.Log.LogInfo("Successfully patched TheSkeld !");
                Destroy(this.gameObject);
            }
        }

        public class MiraHQPatcher : MonoBehaviour
        {
            private void FixedUpdate()
            {
                var client = AmongUsClient.Instance;
                
                // On charge Polus 
                var res = Addressables.LoadAssetAsync<GameObject>(client.ShipPrefabs[(Index) (int) ShipStatus.MapType.Pb]).Result;
                if (!res)
                    return; // On réessaiera de charger à la prochaine update
                
                Instance.Log.LogDebug("Patching MiraHQ...");
                
                // Récup les ressources
                var impostorDetectors = GameObject.FindObjectsOfType<ImpostorDetector>();

                // HACK: je sais pas trop à quoi il sert en temps normal, mais ce composant cause le bug de glissement
                // à la tâche des feuilles !
                Instance.Log.LogDebug("Fixing \"Clean O₂ Filter\" task...");
                foreach (var impostorDetector in impostorDetectors)
                {
                    impostorDetector.GetComponent<CircleCollider2D>().enabled = false;
                }
                
                Instance.Log.LogInfo("Successfully patched MiraHQ !");
                Destroy(this.gameObject);
            }
        }

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        public static class VersionShowerPatch
        {
            public static void Postfix(VersionShower __instance)
            {
                __instance.text.text += $"<size=40%> + {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} (Dadoum)</size>";
            }
        }
        
        public override void Load()
        {
            Instance = this;
            
            ClassInjector.RegisterTypeInIl2Cpp<MiraHQPatcher>();
            ClassInjector.RegisterTypeInIl2Cpp<SkeldPatcher>();
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
            
            SceneManager.sceneLoaded += (Action<Scene, LoadSceneMode>) ((scene, loadSceneMode) =>
            {
                ModManager.Instance.ShowModStamp();
            });
        }
    }
}
