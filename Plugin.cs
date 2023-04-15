using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using System.IO;
using TheForest;
using TheForest.Utils;
using Sons.Gameplay;
using Sons.Save;
using Sons.Gui;
using Sons.Gameplay.GPS;
using TMPro;
using Bolt;
using BoltInternal;
using static KnightVPickup.CustomKnightVEvents;
using System;
using Sons.Gameplay.GameSetup;

namespace KnightVPickup;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    public const string PLUGIN_GUID = "Smokyace.KnightVPickup";
    public const string PLUGIN_NAME = "KnightVPickup";
    public const string PLUGIN_VERSION = "1.0.0";
    private const string author = "SmokyAce";

    public static ConfigFile configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "KnightVPickup.cfg"), true);
    public static ConfigEntry<KeyCode> smokyaceKnightVKey = configFile.Bind("General", "HotKey", KeyCode.Keypad3, "Hotkey for picking up/dropping glider");
    public static ConfigEntry<float> smokyacePickUpGliderTextSize = configFile.Bind("Advanced", "WIP - TextSize", 2f, new ConfigDescription("Change the size of text under the glider", null, "Advanced"));
    public static ConfigEntry<bool> smokyaceDeactivateUI = configFile.Bind("General", "DeactivateUI", false, "If true no Warning UI elements will be added");
    public static ConfigEntry<bool> smokyaceLogToConsole = configFile.Bind("Advanced", "ShowLogs", false, new ConfigDescription("Logs will display in the console", null, "Advanced"));
    public static ConfigEntry<bool> smokyaceDeactivate = configFile.Bind("General", "DisableMod", false, "If true mod will not work");
    public static ConfigEntry<bool> smokyaceLogSphereObjects = configFile.Bind("Advanced", "LogSphereObjects", false, new ConfigDescription("If true colliders in scan sphere will post", null, "Advanced"));


    public static ManualLogSource DLog = new ManualLogSource("DLog");
    public override void Load()
    {
        BepInEx.Logging.Logger.Sources.Add(DLog);

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        //For harmony
        Harmony.CreateAndPatchAll(typeof(KnightVPatcher), null);

        //For Mono Behavior
        base.AddComponent<KnightVPickuper>();

    }

    public static void PostLogsToConsole(bool timeDelay, string messange)
    {
        if (!timeDelay && smokyaceLogToConsole.Value)
        {
            DLog.LogInfo(messange);
        }
        else if (timeDelay && smokyaceLogToConsole.Value)
        {
            if (Time.frameCount % 15 == 0)
            {
                DLog.LogInfo(messange);
            }
        }
    }
    

    public class KnightVPatcher
    {
        private static void DisplayWarning()
        {
            if (smokyaceDeactivateUI.Value)
            {
                return;
            }
            if (KnightVPickuper.isKnightVPickedUp)
            {
                PostLogsToConsole(false, "Display Warning On");
                KnightVPatcher.DropKnightVWarningTextPriv.SetActive(true);
            }
            else
            {
                PostLogsToConsole(false, "Display Warning OFF");
                KnightVPatcher.DropKnightVWarningTextPriv.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(PauseMenu), "TriggerQuitToTitle")]
        [HarmonyPostfix]
        public static void PostfixGetQuit(PauseMenu __instance)
        {
            PostLogsToConsole(false, "In Get Quit PostFix");
            DisplayWarning();
        }

        [HarmonyPatch(typeof(GPSTrackerSystem), "OnEnable")]
        [HarmonyPostfix]
        public static void OnEnablePostfix(ref GPSTrackerSystem __instance)
        {
            PostLogsToConsole(false, "IN From GPS OnEnablePostfix From KnightV");

            bool flag = KnightVPatcher.DropKnightVWarningTextPriv == null;
            if (flag)
            {
                KnightVPatcher.loadUI();
            }
        }
        public static void loadUI()
        {
            PostLogsToConsole(false, "In LoadUI From KnightVPickup");
            Mache.Networking.EventDispatcher.RegisterEvent<NonHostKnightVPickupEvent>();
            if (smokyaceDeactivateUI.Value)
            {
                return;
            }
            GameObject gameObjectEscapePriv = GameObject.Find("ModalDialogManager");
            GameObject DynamicModalDialogGuiPriv = gameObjectEscapePriv.transform.GetChild(0).gameObject;
            GameObject PanelPriv = DynamicModalDialogGuiPriv.transform.GetChild(1).gameObject;
            GameObject ContentPriv = PanelPriv.transform.GetChild(2).gameObject;
            KnightVPatcher.DropKnightVWarningTextPriv = new GameObject("DropKnightVWarningText");
            KnightVPatcher.KnightVWarningText = DropKnightVWarningTextPriv.gameObject.AddComponent<TextMeshProUGUI>();
            KnightVPatcher.KnightVWarningText.SetText("DROP Knight V BEFORE QUITTING");
            KnightVPatcher.KnightVWarningText.fontSize = 50f;
            KnightVPatcher.KnightVWarningText.autoSizeTextContainer = true;
            KnightVPatcher.KnightVWarningText.enableAutoSizing = true;
            DropKnightVWarningTextPriv.transform.SetParent(ContentPriv.transform);
            KnightVPatcher.KnightVWarningText.rectTransform.localPosition = new Vector3(0f, 0f, 0f);
            KnightVPatcher.KnightVWarningText.rectTransform.offsetMax = new Vector2(500, 25);
            KnightVPatcher.DropKnightVWarningTextPriv.transform.localScale = new Vector3(1, 1, 1);
            KnightVPatcher.DropKnightVWarningTextPriv.transform.localPosition = new Vector3(0f, 180f, 0f);
            KnightVPatcher.DropKnightVWarningTextPriv.SetActive(false);
        }
        public static GameObject DropKnightVWarningTextPriv;
        public static TextMeshProUGUI KnightVWarningText;

    }

    
    public class KnightVPickuper : MonoBehaviour
    {
        public static bool isKnightVPickedUp = false;
        public static int KnightVId = 630;
        public static bool IsBusy = false;

        private void Update()
        {
            if (smokyaceDeactivate.Value || !LocalPlayer.IsInWorld || Sons.Gui.PauseMenu.IsActive || TheForest.Utils.LocalPlayer.IsInInventory || LocalPlayer.Inventory.Logs.HasLogs) { return; }
            if (Input.GetKeyDown(smokyaceKnightVKey.Value))
            {
                if (!isKnightVPickedUp)
                {
                    PostLogsToConsole(false, "KnightVPickup From Ground Event");
                    PickUpGliderFast();
                }
                else
                {
                    LocalPlayer.Inventory.StashHeldItems(true, true);
                    isKnightVPickedUp = false;
                    LocalPlayer.Inventory.AddItem(KnightVId, 1);
                }
            }
        }
        public static void PickUpGliderFast()
        {
            IsBusy = true;
            Collider[] hitColliders = Physics.OverlapSphere(LocalPlayer.Transform.position, 2, ~(1 << 26), QueryTriggerInteraction.Ignore);
            foreach (var hitCollider in hitColliders)
            {
                if (smokyaceLogSphereObjects.Value && smokyaceLogToConsole.Value)
                {
                    PostLogsToConsole(false, "HitCollider GameObject Name = " + hitCollider.gameObject.name);
                }
                if (hitCollider.gameObject.name == "WheelCollisionA")
                {
                    PostLogsToConsole(false, "HitCollider GameObject Name = " + hitCollider.gameObject.name);
                    Transform parent = hitCollider.gameObject.transform.GetParent();
                    PostLogsToConsole(false, "Parent GameObject Name = " + parent.gameObject.name);
                    Transform secondParent = parent.gameObject.transform.GetParent();
                    PostLogsToConsole(false, "secondParent GameObject Name = " + secondParent.gameObject.name);
                    if (secondParent.gameObject.name == "KnightVPickup(Clone)")
                    {
                        if (GameSetupManager._instance._saveGameType == SaveGameType.SinglePlayer || GameSetupManager._instance._saveGameType == SaveGameType.Multiplayer)
                        {
                            isKnightVPickedUp = true;
                            Destroy(secondParent.gameObject);
                            IsBusy = false;
                        } else
                        {
                            try
                            {
                                BoltEntity entity = secondParent.gameObject.GetComponent<BoltEntity>();
                                PostLogsToConsole(false, "Entity IsOwner = " + entity.isOwner);
                                PostLogsToConsole(false, "Entity Has Control = " + entity.hasControl);
                                if (entity.isOwner)
                                {
                                    isKnightVPickedUp = true;
                                    Destroy(secondParent.gameObject);
                                    IsBusy = false;
                                }
                                else
                                {
                                    NetworkId netID = entity.networkId;
                                    PostLogsToConsole(false, "Network id = " + netID);

                                    Mache.Networking.EventDispatcher.RaiseEvent(new NonHostKnightVPickupEvent
                                    {
                                        Message = "Destroy KnightV World Object!",
                                        MessageCount = 1,
                                        DoDestroy = true,
                                        NetworkID = netID.ToString(),
                                    });

                                    isKnightVPickedUp = true;
                                    IsBusy = false;
                                }
                            }

                            catch (Exception e)
                            {
                                Plugin.PostLogsToConsole(false, "Catched: " + e);
                            }
                        }
                            
                    }

                } 
            }
            IsBusy = false;
        }
    }
}
