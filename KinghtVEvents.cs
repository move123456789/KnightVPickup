using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mache.Networking;
using System.Runtime.CompilerServices;

namespace KnightVPickup
{
    public class CustomKnightVEvents
    {
        public class NonHostKnightVPickupEvent : SimpleEvent<NonHostKnightVPickupEvent>
        {
            public string Message { get; set; }
            public int MessageCount { get; set; }

            public bool DoDestroy { get; set; }

            public string NetworkID { get; set; }


            public override void OnReceived()
            {
                Plugin.PostLogsToConsole(false, "Running OnRecived, String NetworkID = " + NetworkID);
                Bolt.NetworkId networkId = StringToBoltNetworkId(NetworkID);

                BoltEntity KnightVFromID = BoltNetwork.FindEntity(networkId);
                Plugin.PostLogsToConsole(false, "Network ID From OnReceived = " + networkId);

                if (KnightVFromID != null)
                {
                    Plugin.PostLogsToConsole(false, "gliderFromID != null");
                    if (KnightVFromID.isOwner)
                    {
                        Plugin.PostLogsToConsole(false, "Enity Owner, Deleting World Object");
                        BoltNetwork.Destroy(KnightVFromID);
                    }
                    else
                    {
                        Plugin.PostLogsToConsole(false, "You are Not Enity Owner");
                    }


                }
                else { Plugin.PostLogsToConsole(false, "gliderFromID === NULL"); }

            }
        }

        public static Bolt.NetworkId StringToBoltNetworkId(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Input string cannot be null or empty");
            }

            var match = Regex.Match(input, @"\[(?i:NetworkID) ([0-9A-Fa-f]{1,2}(?:-[0-9A-Fa-f]{1,2}){7})\]", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                throw new ArgumentException($"Input string '{input}' is not in the correct format. Please provide a valid input string.");
            }

            string[] parts = match.Groups[1].Value.Split('-');
            byte[] bytes = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                try
                {
                    bytes[i] = byte.Parse(parts[i], NumberStyles.HexNumber);
                }
                catch (FormatException)
                {
                    throw new ArgumentException($"Part '{parts[i]}' in the input string '{input}' is not in the correct format. Please provide a valid input string.");
                }
            }

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            ulong networkIdValue = BitConverter.ToUInt64(bytes, 0);
            return new Bolt.NetworkId(networkIdValue);
        }
    }
}
