using System;
using System.Collections.Generic;

using ACE.Common;
using ACE.Database.Models.Shard;
using ACE.Server.Managers;
using ACE.PcapReader;

namespace ACE.Server.Network.GameMessages.Messages
{
    public class GameMessageCharacterList : GameMessage
    {
        public GameMessageCharacterList(List<Character> characters, Session session) : base(GameMessageOpcode.CharacterList, GameMessageGroup.UIQueue)
        {
            Writer.Write(0u);
            Writer.Write(1u);

            if(PCapReader.CharacterGUID == 0)
            {
                Writer.Write(0x50000001);
                Writer.WriteString16L("NO PCAP LOADED");
            }
            else
            {
                Writer.Write(PCapReader.CharacterGUID);
                Writer.WriteString16L("PCap Playback");
            }
            Writer.Write(0u);

            Writer.Write(0u);
            var slotCount = (uint)PropertyManager.GetLong("max_chars_per_account").Item;
            Writer.Write(slotCount);
            Writer.WriteString16L(session.Account);
            Writer.Write(1u /*useTurbineChat*/);
            Writer.Write(1u /*hasThroneOfDestiny*/);
        }
    }
}
