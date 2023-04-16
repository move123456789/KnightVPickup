using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace KnightVPickup
{
    public class PopUp
    {
        private static async Task DisplayTimer()
        {
            await Task.Delay(2500);
        }
        internal static async void DiplayKnightVPickUp()
        {
            knightVPanel.SetActive(true);
            await Task.Run(DisplayTimer);
            knightVPanel.SetActive(false);
        }
        internal static void CreateUI()
        {
            GameObject KnightVPickupUiObject = GameObject.Find("ToggleableHUD");
            knightVPanel = new GameObject("KnightVPickUpUi");
            knightVPanel.transform.SetParent(KnightVPickupUiObject.transform);
            knightVPanel.transform.localPosition = new Vector3(0f, -460f, 0f);
            GameObject GldierPickupUiTextObject = new("GliderPickUpText");
            KnightVText = GldierPickupUiTextObject.gameObject.AddComponent<TextMeshProUGUI>();
            KnightVText.SetText("Picked Up Knight V");
            KnightVText.fontSize = 42f;
            KnightVText.autoSizeTextContainer = true;
            KnightVText.enableAutoSizing = true;
            GldierPickupUiTextObject.transform.SetParent(knightVPanel.transform);
            KnightVText.rectTransform.localPosition = new Vector3(90f, 0f, 0f);
            knightVPanel.SetActive(false);
        }
        public static GameObject knightVPanel;
        public static TextMeshProUGUI KnightVText;
    }
}
