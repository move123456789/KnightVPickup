using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using System.IO;
using TheForest.Utils;
using Sons.Save;
using Sons.Gui;
using Sons.Gameplay.GPS;
using TMPro;
using Bolt;
using System;
using Sons.Gameplay.GameSetup;
using System.Reflection;

namespace KnightVPickup;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    public const string PLUGIN_GUID = "Smokyace.KnightVPickup";
    public const string PLUGIN_NAME = "KnightVPickup";
    public const string PLUGIN_VERSION = "1.0.3";
    private const string author = "SmokyAce";

    public static ConfigFile configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "KnightVPickup.cfg"), true);
    public static ConfigEntry<KeyCode> smokyaceKnightVKey = configFile.Bind("General", "HotKey", KeyCode.Keypad3, "Hotkey for picking up/dropping glider");
    public static ConfigEntry<float> smokyacePickUpGliderTextSize = configFile.Bind("Advanced", "WIP - TextSize", 2f, new ConfigDescription("Change the size of text under the glider", null, "Advanced"));
    public static ConfigEntry<bool> smokyaceDeactivateUI = configFile.Bind("General", "DeactivateUI", false, "If true no Warning UI elements will be added");
    public static ConfigEntry<bool> smokyaceLogToConsole = configFile.Bind("Advanced", "ShowLogs", false, new ConfigDescription("Logs will display in the console", null, "Advanced"));
    public static ConfigEntry<bool> smokyaceDeactivate = configFile.Bind("General", "DisableMod", false, "If true mod will not work");
    public static ConfigEntry<bool> smokyaceLogSphereObjects = configFile.Bind("Advanced", "LogSphereObjects", false, new ConfigDescription("If true colliders in scan sphere will post", null, "Advanced"));


    internal static string dllPath = Assembly.GetExecutingAssembly().Location;
    internal static string directory = Path.GetDirectoryName(dllPath);
    internal static string fileDir = Path.Join(directory, "KnightVData");

    public static ManualLogSource KnightVPickup = new ManualLogSource("KnightVPickup");
    public override void Load()
    {
        BepInEx.Logging.Logger.Sources.Add(KnightVPickup);

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        //For harmony
        Harmony.CreateAndPatchAll(typeof(KnightVPatcher), null);

        //For Mono Behavior
        base.AddComponent<KnightVPickuper>();

        // Data Folder
        if (!Directory.Exists(fileDir))
        {
            Directory.CreateDirectory(fileDir);
        }
    }

    public static void PostLogsToConsole(bool timeDelay, string messange)
    {
        if (!timeDelay && smokyaceLogToConsole.Value)
        {
            KnightVPickup.LogInfo(messange);
        }
        else if (timeDelay && smokyaceLogToConsole.Value)
        {
            if (Time.frameCount % 15 == 0)
            {
                KnightVPickup.LogInfo(messange);
            }
        }
    }

    public static void PostErrorToConsole(string messange)
    {
        KnightVPickup.LogInfo(messange);
    }


    public class KnightVPatcher
    {

        [HarmonyPatch(typeof(GPSTrackerSystem), "OnEnable")]
        [HarmonyPostfix]
        public static void OnEnablePostfix(ref GPSTrackerSystem __instance)
        {
            PostLogsToConsole(false, "IN From GPS OnEnablePostfix From KnightV");
            if (PopUp.knightVPanel == null)
            {
                PopUp.CreateUI();
            }
        }

        [HarmonyPatch(typeof(GameSetupManager), "GetSelectedSaveId")]
        [HarmonyPostfix]
        public static void PostfixGetLoadedSaveID(uint __result)
        {
            PostLogsToConsole(false, "Postfix PostfixGetLoadedSaveID Loaded");
            uint? nullable = __result;
            if (nullable.HasValue)
            {
                PostLogsToConsole(false, "Save Id = " + __result);
                KnightVPickuper.saveId = __result;
            }
            else { PostLogsToConsole(false, "SaveId Posfix __result Does Not Have A Value"); }

        }
    }

    public class JSONData
    {
        public string DoHaveGlider { get; set; }
    }

    public class KnightVPickuper : MonoBehaviour
    {
        public static bool isKnightVPickedUp = false;
        public static int KnightVId = 630;
        public static bool IsBusy = false;
        private bool isQuitEventAdded;
        private bool isInfoLoaded = false;
        internal static uint saveId;

        private void Quitting()
        {
            isQuitEventAdded = false;
            if (saveId != 0)
            {
                var fileName = $"{Plugin.fileDir}/{KnightVPickuper.saveId}.json";

                // Convert the boolean to a string
                string KnightVPickedUpString = isKnightVPickedUp.ToString();

                // Create the JSON string manually
                string jsonString = "{ \"DoHaveKnightV\": \"" + KnightVPickedUpString + "\" }";

                // Save the JSON string to a file
                System.IO.File.WriteAllText(fileName, jsonString);
            }
            isInfoLoaded = false;
        }

        private void Update()
        {
            if (smokyaceDeactivate.Value || !LocalPlayer.IsInWorld || Sons.Gui.PauseMenu.IsActive || TheForest.Utils.LocalPlayer.IsInInventory || LocalPlayer.Inventory.Logs.HasLogs) { return; }
            if (Input.GetKeyDown(smokyaceKnightVKey.Value))
            {
                if (!isKnightVPickedUp)
                {
                    PostLogsToConsole(false, "isKnightVPickedUp From Ground Event");
                    PickUpKnightVFast();
                }
                else
                {
                    LocalPlayer.Inventory.StashHeldItems(true, true);
                    isKnightVPickedUp = false;
                    LocalPlayer.Inventory.AddItem(KnightVId, 1);
                }
            }
            if (!isInfoLoaded)
            {
                if (saveId != 0)
                {
                    isInfoLoaded = true;
                    var filePath = $"{Plugin.fileDir}/{saveId}.json";
                    PostLogsToConsole(false, $"Loading File, does file exisit: {System.IO.File.Exists(filePath)}");
                    if (System.IO.File.Exists(filePath))
                    {
                        // READ THE DATA HERE
                        string jsonString = System.IO.File.ReadAllText(filePath);

                        // Parse the JSON manually
                        string key = "\"DoHaveKnightV\": ";
                        int startIndex = jsonString.IndexOf(key) + key.Length + 1;
                        int endIndex = jsonString.IndexOf("\"", startIndex);
                        string isKnightVPickedUpString = jsonString.Substring(startIndex, endIndex - startIndex);

                        // Convert the string back to a boolean
                        isKnightVPickedUp = bool.Parse(isKnightVPickedUpString);
                        PostLogsToConsole(false, $"File Loaded isKnightVPickedUp STRING = {isKnightVPickedUpString}, isKnightVPickedUp BOOL: {bool.Parse(isKnightVPickedUpString)}");
                    }
                }
            }
            if (!isQuitEventAdded)
            {
                PostLogsToConsole(false, "Adding Quit Event");
                isQuitEventAdded = true;
                PauseMenu.add_OnQuitEvent((Il2CppSystem.Action)Quitting);
            }
        }
        public static void PickUpKnightVFast()
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
                            if (PopUp.knightVPanel != null)
                            {
                                PopUp.DiplayKnightVPickUp();
                            }
                        } else
                        {
                            BoltEntity kinghtVEntity = secondParent.gameObject.GetComponent<BoltEntity>();
                            PostLogsToConsole(false, "Entity IsOwner = " + kinghtVEntity.isOwner);
                            PostLogsToConsole(false, "Entity Has Control = " + kinghtVEntity.hasControl);

                            try
                            {
                                DestroyPickUp destroyPickUp;
                                if (kinghtVEntity.source == null)
                                {
                                    destroyPickUp = DestroyPickUp.Create(GlobalTargets.OnlySelf);
                                    PostLogsToConsole(false, "IN: DestroyPickUp.Create(GlobalTargets.OnlySelf)");
                                }
                                else
                                {
                                    destroyPickUp = DestroyPickUp.Create(kinghtVEntity.source);
                                    PostLogsToConsole(false, "IN: DestroyPickUp.Create(kinghtVEntity.source)");
                                }
                                destroyPickUp.PickUpPlayer = LocalPlayer.Entity;
                                destroyPickUp.PickUpEntity = kinghtVEntity;
                                destroyPickUp.ItemId = 626;
                                destroyPickUp.SibblingId = -1;
                                destroyPickUp.FakeDrop = false;
                                destroyPickUp.Send();
                                IsBusy = false;
                                if (PopUp.knightVPanel != null)
                                {
                                    PopUp.DiplayKnightVPickUp();
                                }
                            }
                            catch (Exception e)
                            {
                                PostErrorToConsole("Something went wrong in PickUpGliderFast(KNIGHTV), Error: " + e);
                            }

                            isKnightVPickedUp = true;
                            IsBusy = false;
                                
                        }
  
                    }

                } 
            }
            IsBusy = false;
        }
    }
}
