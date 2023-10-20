using HarmonyLib;
using RedLoader;
using Sons.Cutscenes;
using Sons.Gameplay.GameSetup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnightVAndGliderPickup
{
    [HarmonyPatch]
    internal class Patches
    {
        [HarmonyPatch(typeof(InventoryCutscene), "OnEnable")]
        [HarmonyPostfix]
        public static void OnInvetoryOpen(InventoryCutscene __instance)
        {
            Extras.PostMsg("Opened Inventory");
            if (Glider.gliderItem != null)
            {
                Glider.gliderItem.IsHighlighted = false;
            }
            else
            {
                Extras.PostMsg("Glider Item IS NULL");
            }
        }
    }
}
