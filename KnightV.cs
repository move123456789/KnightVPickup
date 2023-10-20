using Sons.Inventory;
using Sons.Items.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheForest.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace KnightVAndGliderPickup
{
    public class KnightV
    {
        public static void KnightVToInventory()
        {
            ItemData item = ItemDatabaseManager.ItemById(630); // KnightV
            if (item.Id == 630)
            {
                item._type = (Sons.Items.Core.Types)17;
                item._canBeHotKeyed = true;
                item._alwaysDropOnUnequip = false;
                item._allowPickupWhenInventoryIsFull = true;
                item.UiData._leftClick = (ItemUiData.LeftClickCommands)10;
                item.UiData._rightClick = 0;
                Transform original = LocalPlayer._instance._inventory._inventoryCutscene._inventory.transform.FindChild("LayoutGroups/_Unused_/KnightVLayoutGroup");
                Transform transform = UnityEngine.Object.Instantiate<Transform>(original);
                transform.GetComponent<InventoryLayoutItemGroup>().enabled = true;
                transform.gameObject.SetActive(true);
                transform.parent = LocalPlayer._instance._inventory._inventoryCutscene._inventory.transform.FindChild("LayoutGroups");
                transform.transform.localPosition = new Vector3(-2.6f, 0.15f, 1.6f);
                transform.transform.localRotation = quaternion.Euler(26.6f, 157.4f, 263.3f, math.RotationOrder.ZXY);
            }
        }
    }
}
