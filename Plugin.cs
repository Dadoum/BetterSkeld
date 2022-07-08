using System;
using System.Collections.Generic;
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
                
                // Récup les ressources (avec une méthode moche mais rapide).
                GameObject admin = null;
                GameObject animation = null;
                Vent adminVent = null;
                Vent cafeteriaVent = null;
                Vent navNordVent = null;
                Vent navSudVent = null;
                Vent weaponsVent = null;
                Vent shieldVent = null;
                Vent couloirVent = null;
                Vent elecVent = null;
                Vent reactorNordVent = null;
                Vent reactorSudVent = null;
                Vent engineNordVent = null;
                Vent engineSudVent = null;
                Vent securityVent = null;
                Vent medVent = null;
                List<GameObject> impostorDetectors = new List<GameObject>();
                var gameObjects = GameObject.FindObjectsOfType<GameObject>();
                foreach (var gameObject in gameObjects)
                {
                    var name = gameObject.name;
                    if (name == "MapRoomConsole")
                    {
                        admin = gameObject;
                        continue;
                    }
                    else if (name == "MapAnimation")
                    {
                        animation = gameObject;
                        continue;
                    }

                    var vent = gameObject.GetComponent<Vent>();
                    if (vent != null)
                    {
                        if (name == "AdminVent")
                        {
                            adminVent = vent;
                        }
                        else if (name == "CafeVent")
                        {
                            cafeteriaVent = vent;
                        }
                        else if (name == "NavVentNorth")
                        {
                            navNordVent = vent;
                        }
                        else if (name == "NavVentSouth")
                        {
                            navSudVent = vent;
                        }
                        else if (name == "WeaponsVent")
                        {
                            weaponsVent = vent;
                        }
                        else if (name == "ShieldsVent")
                        {
                            shieldVent = vent;
                        }
                        else if (name == "BigYVent")
                        {
                            couloirVent = vent;
                        }
                        else if (name == "ElecVent")
                        {
                            elecVent = vent;
                        }
                        else if (name == "UpperReactorVent")
                        {
                            reactorNordVent = vent;
                        }
                        else if (name == "ReactorVent")
                        {
                            reactorSudVent = vent;
                        }
                        else if (name == "LEngineVent")
                        {
                            engineNordVent = vent;
                        }
                        else if (name == "REngineVent")
                        {
                            engineSudVent = vent;
                        }
                        else if (name == "SecurityVent")
                        {
                            securityVent = vent;
                        }
                        else if (name == "MedVent")
                        {
                            medVent = vent;
                        }
                    }
                    else if (gameObject.GetComponent<ImpostorDetector>() != null)
                    {
                        impostorDetectors.Add(gameObject);
                    }
                }

                if (admin == null ||
                    animation == null ||
                    adminVent == null ||
                    cafeteriaVent == null ||
                    navNordVent == null ||
                    navSudVent == null ||
                    weaponsVent == null ||
                    shieldVent == null ||
                    couloirVent == null ||
                    elecVent == null ||
                    reactorNordVent == null ||
                    reactorSudVent == null ||
                    engineNordVent == null ||
                    engineSudVent == null ||
                    securityVent == null ||
                    medVent == null)
                    return;
                
                var vitalsObj = res.transform.Find("Office/panel_vitals").gameObject;
                
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
                
                // Relier toutes les vents de gauche entre elles, et toutes celles de droite entre elles
                Instance.Log.LogDebug("Rerouting vents...");
                
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
