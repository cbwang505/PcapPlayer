using System;
using System.Collections.Generic;

using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using ACE.Entity.Enum;
using ACE.Server.Network;
using ACE.PcapReader;

namespace ACE.Server.Command.Handlers
{
    public static class ConsoleCommands
    {
        [CommandHandler("pcap-load", AccessLevel.Player, CommandHandlerFlag.ConsoleInvoke, 0, "Load a PCAP for playback.", "<full-path-to-pcap-file>")]
        public static void HandleLoadPcap(Session session, params string[] parameters)
        {
            // pcap-load "D:\Asheron's Call\Log Files\PCAP Part 1\AC1Live-The-Ripley-Collection-part01\AC1Live-The-Ripley-Collection\aclog pcaps\pkt_2017-1-9_1484023507_log.pcap"
            // pcap-load "D:\ACE\Logs\PCAP Part 1\AC1Live-The-Ripley-Collection-part01\AC1Live-The-Ripley-Collection\aclog pcaps\pkt_2017-1-9_1484023507_log.pcap"

            string pcapFileName;
            if (parameters?.Length != 1)
            {
                Console.WriteLine("pcap-load <full-path-to-pcap-file>");
                //pcapFileName = "D:\\ACE\\player.pcap"; // 
                return;
            }
            else
            {
                pcapFileName = parameters[0];
            }

            // Check if file exists!
            if (!System.IO.File.Exists(pcapFileName))
            {
                Console.WriteLine("Could not find pcap file to load: " + pcapFileName);
                return;
            }
            bool abort = false;

            Console.WriteLine($"Loading pcap...");

            //List<PacketRecord> records = PCapReader.LoadPcap(pcapFileName, true, ref abort);
            PCapReader.LoadPcap(pcapFileName, true, ref abort);

            Console.WriteLine($"Pcap Loaded with " + PCapReader.Records.Count.ToString() + " records.");

            if (PCapReader.LoginInstances > 0)
            {
                Console.WriteLine("\n" + PCapReader.LoginInstances.ToString() + " unique login events detected.");
                if (PCapReader.LoginInstances > 1)
                    Console.WriteLine("Please specify a login to use using the comamnd 'pcap-login <login-#>', where <login-#> is 1 to " + PCapReader.LoginInstances.ToString() + "\n");
                Console.WriteLine("Login set to first instance.");
                Console.WriteLine("Instance has " + PCapReader.TeleportInstances[1] + " teleports. Use @teleport in-game to advance to next, or @telport <index> to select a specific one.");

                Console.WriteLine("StartRecordIndex: " + PCapReader.StartRecordIndex);
                Console.WriteLine("EndRecordIndex: " + PCapReader.EndRecordIndex);
            }
            else
            {
                Console.WriteLine("\nNo login events detected. We will attempt to join this pcap already in progress.\n");
                Console.WriteLine("Instance has " + PCapReader.TeleportInstances[0] + " teleports. Use @teleport in-game to advance to next, or @telport <index> to select a specific one.");
            }

            Console.WriteLine("");
        }

        [CommandHandler("pcap-login", AccessLevel.Player, CommandHandlerFlag.ConsoleInvoke, 0, "Specify a login instance for pcap playback.", "<login-#>")]
        public static void HandlePcapSetLogin(Session session, params string[] parameters)
        {
            if (parameters?.Length != 1)
            {
                if (PCapReader.LoginInstances > 0)
                {
                    Console.WriteLine("\n" + PCapReader.LoginInstances.ToString() + " unique login events detected. Please specify a login to use using the comamnd 'pcap-login <login-#>', where <login-#> is 1 to " + PCapReader.LoginInstances.ToString() + "\n");
                }
                else
                {
                    Console.WriteLine("\nNo login events detected. We cannot play back this pcap at this time, sorry.\n");
                }
                return;
            }

            if (int.TryParse(parameters[0], out int loginID))
            {
                PCapReader.SetLoginInstance(loginID);
                Console.WriteLine("Login instance set. Pcap will play records " + PCapReader.StartRecordIndex.ToString() + " to " + PCapReader.EndRecordIndex.ToString());
                Console.WriteLine("Instance has " + PCapReader.TeleportInstances[loginID] + " teleports. Use @teleport in-game to advance to next, or @telport <index> to select a specific one.");
                PCapReader.GetPcapDuration();
            }
            else
            {
                Console.WriteLine("Unable to set login instance.");
            }

        }

        [CommandHandler("teleport", AccessLevel.Player, CommandHandlerFlag.RequiresWorld, 0, "Sends player to next teleport instance in Pcap", "")]
        public static void HandleTeleport(Session session, params string[] parameters)
        {
            session.PausePcapPlayback();
            int? teleportID = null;
            if (parameters?.Length > 0)
            {
                // If we fail to get a valid int, we will continue with null (which means "next instance");
                if (int.TryParse(parameters[0], out int teleportIndex))
                    teleportID = teleportIndex;
            }

            bool teleportFound = PCapReader.DoTeleport(teleportID);
            if (teleportFound)
                Console.WriteLine("Advancing to next teleport session, entry " + PCapReader.CurrentPcapRecordStart);
            else
                Console.WriteLine("Sorry, there were no additional teleport events in this pcap.");
            session.RestartPcapPlayback();
        }

        [CommandHandler("teleportlist", AccessLevel.Player, CommandHandlerFlag.ConsoleInvoke, 0, "Lists the pcap line numbers of each teleport instance.", "")]
        public static void HandleTeleportList(Session session, params string[] parameters)
        {
            if (parameters?.Length > 0)
            {
                Console.WriteLine("This command doesn't take parameters.");
            }
            if (PCapReader.TeleportIndexes.Count > 0)
            {
                for (int i = 0; i < PCapReader.TeleportIndexes.Count; i++)
                {
                    Console.WriteLine($"{i}: {PCapReader.TeleportIndexes[i]}");
                }
            }
            else
            {
                Console.WriteLine("Sorry, there are not teleport events in this pcap.");
            }
        }
    }
}
