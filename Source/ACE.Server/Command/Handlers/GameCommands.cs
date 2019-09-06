using System;
using System.Collections.Generic;

using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using ACE.Entity.Enum;
using ACE.Server.Network;
using ACE.PcapReader;
using ACE.Server.Network.GameMessages.Messages;

namespace ACE.Server.Command.Handlers
{
    public static class GameCommands
    {
        [CommandHandler("NO_teleport", AccessLevel.Player, CommandHandlerFlag.RequiresWorld, 0,"Sends player to next teleport instance in Pcap", "")]
        public static void NO_HandleTeleport(Session session, params string[] parameters)
        {
            session.PausePcapPlayback();
            bool teleportFound = PCapReader.DoTeleport();
            if (teleportFound)
                Console.WriteLine("Advancing to next teleport session, entry " + PCapReader.CurrentPcapRecordStart);
            else
                Console.WriteLine("Sorry, there were no additional teleport events in this pcap.");
            session.RestartPcapPlayback();
        }
    }
}
