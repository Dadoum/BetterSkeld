using System;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BetterSkeld
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class BetterSkeldPlugin : BasePlugin
    {
        public static BetterSkeldPlugin Instance { get; private set; }
        private static bool patched;


        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
        public static class ShipStatusAwakePatch
        {
            public static void Prefix(ShipStatus __instance)
            {
                patched = __instance.Type != ShipStatus.MapType.Ship;
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
        public static class ShipStatusUpdatePatch
        {
            public static void Postfix()
            {
                if (!patched)
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
                    
                    // Ajouter les signaux vitaux
                    Instance.Log.LogDebug("Placing vitals...");
                    var vitals = GameObject.Instantiate(vitalsObj);
                    vitals.transform.position = new Vector3(-17.2812f, 0.1971f, 0f);
                    
                    // Désactiver l'admin
                    Instance.Log.LogDebug("Disabling admin...");
                    admin.GetComponent<CircleCollider2D>().enabled = false;
                    
                    // Désactiver l'animation car il n'y a plus de carte, donc pour le RP faut plus qu'on la voie
                    animation.active = false;
                    
                    Instance.Log.LogDebug("Done !");
                    patched = true;
                }
            }
        }
        
        public override void Load()
        {
            Instance = this;
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
        }
    }
}
