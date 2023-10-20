using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using UnityEngine;

namespace KnightVAndGliderPickup
{
    internal class Data
    {
        public class SavedInfo
        {
            public string glider { get; set; }
            public string knightv { get; set; }
        }

        public static void SaveData(uint saveId, string gliderValue, string knightValue)
        {
            Extras.PostMsg("Trying To Save");

            var fileName = $"{saveId}.json";
            var jsonObj = new JsonObject
            {
                // ... [Your existing key-value pairs]

                ["glider"] = gliderValue,
                ["knightv"] = knightValue
            };

            WriteDynamicJsonObject(jsonObj, fileName);
        }



        internal static SavedInfo GetData(uint saveId)
        {
            string folderPath = Path.Combine(Extras.dataPath, Extras.saveFolder);
            if (File.Exists($"{folderPath}/{saveId}.json"))
            {
                Extras.PostMsg("Save File For World Found");

                string text = File.ReadAllText($"{folderPath}/{saveId}.json");

                var saveInfo = System.Text.Json.JsonSerializer.Deserialize<SavedInfo>(text);

                return saveInfo; // This will return the loaded data including glider and knightv values.
            }
            else
            {
                Extras.PostMsg("File Not Found");
                return null;
            }
        }

        internal static void WriteDynamicJsonObject(JsonObject jsonObj, string fileName)
        {
            string folderPath = Path.Combine(Extras.dataPath, Extras.saveFolder);
            string fullPath = Path.Combine(folderPath, fileName);
            using var fileStream = File.Create(fullPath);
            using var utf8JsonWriter = new Utf8JsonWriter(fileStream);
            jsonObj.WriteTo(utf8JsonWriter);
            Extras.PostMsg("Data Saved");
        }
    }
}
