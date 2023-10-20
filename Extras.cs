using RedLoader;
using SonsSdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KnightVAndGliderPickup
{
    internal class Extras
    {
        internal static bool isQuitEventAdded;
        internal static uint _saveId;

        internal static string dataPath;
        internal const string saveFolder = "InventoryData";

        internal static void EnsureFolderExists(string folderName = saveFolder)
        {
            string folderPath = Path.Combine(dataPath, folderName);

            if (!Directory.Exists(folderPath))
            {
                PostMsg("Creating InventoryData Folder");
                Directory.CreateDirectory(folderPath);
            }
        }

        internal static bool DoesJsonFileExists(uint saveId)
        {
            string fileLocation = Path.Combine(dataPath, saveFolder);

            if (File.Exists($"{fileLocation}/{saveId}.json"))
            {
                PostMsg($"{fileLocation}/{saveId} does exist.");
                return true;
            } else { PostMsg($"{fileLocation}/{saveId} does NOT exist.");  return false; }
        }

        internal static void PostMsg(string msg)
        {
            if (Config.LogsToConsole.Value)
            {
                RLog.Msg(msg);
            }
        }
    }
}
