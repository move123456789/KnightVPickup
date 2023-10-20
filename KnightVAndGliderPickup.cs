using RedLoader;
using Sons.Gameplay.GameSetup;
using Sons.Gui;
using SonsSdk;
using SUI;
using TheForest.Utils;
using UnityEngine;

namespace KnightVAndGliderPickup;

public class KnightVAndGliderPickup : SonsMod
{
    public KnightVAndGliderPickup()
    {
        // Don't register any update callbacks here. Manually register them instead.
        // Removing this will call OnUpdate, OnFixedUpdate etc. even if you don't use them.
        HarmonyPatchAll = true;
    }

    protected override void OnInitializeMod()
    {
        // Do your early mod initialization which doesn't involve game or sdk references here
        Config.Init();
    }

    protected override void OnSdkInitialized()
    {
        // Do your mod initialization which involves game or sdk references here
        // This is for stuff like UI creation, event registration etc.
        
        KnightVAndGliderPickupUi.Create();
        // Adding Ingame CFG
        SettingsRegistry.CreateSettings(this, null, typeof(Config));

        SdkEvents.BeforeLoadSave.Subscribe(() =>
        {
            Extras._saveId = GameSetupManager.GetSelectedSaveId();
        });

        Extras.dataPath = DataPath;

        Extras.EnsureFolderExists();

    }

    protected override void OnGameStart()
    {
        // This is called once the player spawns in the world and gains control.
        if (!Extras.isQuitEventAdded)
        {
            Extras.PostMsg("Adding Quit Event");
            Extras.isQuitEventAdded = true;
            PauseMenu.add_OnQuitEvent((Il2CppSystem.Action)Quitting);
            LocalPlayer.Inventory._inventoryCutscene.OnCutsceneEnded.AddListener((UnityEngine.Events.UnityAction)Glider.OnOpenInventory);
        }


        Extras.PostMsg("Running KnightVToInventory and GliderToInventory");
        KnightV.KnightVToInventory();
        Glider.GliderToInventory();
    }


    private void Quitting()
    {
        Extras.isQuitEventAdded = false;
        if (Extras._saveId != 0)
        {
            if (LocalPlayer.Inventory._itemInstanceManager.HaveAny(626) && LocalPlayer.Inventory._itemInstanceManager.HaveAny(630))
            {
                Data.SaveData(Extras._saveId, "1", "1");
            } 
            else if (LocalPlayer.Inventory._itemInstanceManager.HaveAny(626) && !LocalPlayer.Inventory._itemInstanceManager.HaveAny(630))
            {
                Data.SaveData(Extras._saveId, "1", "0");
            } 
            else if (!LocalPlayer.Inventory._itemInstanceManager.HaveAny(626) && LocalPlayer.Inventory._itemInstanceManager.HaveAny(630))
            {
                Data.SaveData(Extras._saveId, "0", "1");
            }
            else if (!LocalPlayer.Inventory._itemInstanceManager.HaveAny(626) && !LocalPlayer.Inventory._itemInstanceManager.HaveAny(630))
            {
                Data.SaveData(Extras._saveId, "0", "0");
            }

        } else { RLog.Error("SaveID == 0 In Qutting"); }
        
    }
}