using RedLoader;
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
    public class Glider
    {
        public static InventoryLayoutItem gliderItem;
        public static void GliderToInventory()
        {
            ItemData item = ItemDatabaseManager.ItemById(626); // Glider
            if (item.Id == 626)
            {
                item._type = (Sons.Items.Core.Types)17;
                item._canBeHotKeyed = true;
                item._alwaysDropOnUnequip = false;
                item._allowPickupWhenInventoryIsFull = true;
                item.UiData._leftClick = (ItemUiData.LeftClickCommands)10;
                item.UiData._rightClick = 0;
                Transform original = LocalPlayer._instance._inventory._inventoryCutscene._inventory.transform.FindChild("LayoutGroups/_Unused_/HangGliderLayoutGroup");
                Transform transform = UnityEngine.Object.Instantiate<Transform>(original);
                transform.GetComponent<InventoryLayoutItemGroup>().enabled = true;
                transform.gameObject.SetActive(true);
                transform.parent = LocalPlayer._instance._inventory._inventoryCutscene._inventory.transform.FindChild("LayoutGroups");
                transform.transform.localPosition = new Vector3(-1.4f, 0.45f, 2.5f);
                //transform.transform.localRotation = quaternion.Euler(80.5f, 143f, 331.3f, math.RotationOrder.ZXY);
                transform.localRotation = Quaternion.Euler(80.5f, 143f, 331.3f);
                transform.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                InventoryLayoutItemGroup gliderItemGroup = transform.gameObject.GetComponent<InventoryLayoutItemGroup>();
                Il2CppSystem.Collections.Generic.List<InventoryLayoutItem> gliderLayoutItem = gliderItemGroup.LayoutItems;
                gliderItem = gliderLayoutItem[0];
                gliderItem.OnMouseExitEvent.AddListener((UnityEngine.Events.UnityAction<LayoutItem>)HandleMouseExit);
                
            }
        }
        public static void HandleMouseExit(LayoutItem item)
        {
            Extras.PostMsg("Mouse Exit Glider");
            item.IsHighlighted = false;
            
        }
        internal static void OnOpenInventory()
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
