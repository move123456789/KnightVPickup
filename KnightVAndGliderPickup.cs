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

        LoadSavedDataIntoGame();
    }


    private void Quitting()
    {
        Extras.isQuitEventAdded = false;
        LocalPlayer.Inventory._inventoryCutscene.OnCutsceneEnded.RemoveListener((UnityEngine.Events.UnityAction)Glider.OnOpenInventory);
        PauseMenu.remove_OnQuitEvent((Il2CppSystem.Action)Quitting);
        if (Extras._saveId != 0)
        {
            Extras.PostMsg("Saving Data");
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

    private void LoadSavedDataIntoGame()
    {
        if (LocalPlayer.Inventory._itemInstanceManager.HaveAny(626) && LocalPlayer.Inventory._itemInstanceManager.HaveAny(630)) { Extras.PostMsg("Glider and KnightV Alread In Inventory"); return; }
        if (Extras._saveId == 0) { Extras.PostMsg("SaveId is 0 cant load data ino world"); return; }

        var data = Data.GetData(Extras._saveId);
        if (data != null)
        {
            Extras.PostMsg($"Glider: {data.glider}, Knightv: {data.knightv}");
            if (data.glider == "1")
            {
                LocalPlayer.Inventory.AddItem(626, 1, false, false);
            }
            if (data.knightv == "1")
            {
                LocalPlayer.Inventory.AddItem(630, 1, false, false);
            }

        } else { Extras.PostMsg("No Data Found For World"); }
    }
}