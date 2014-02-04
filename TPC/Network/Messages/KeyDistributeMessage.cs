using System.Collections.Generic;

namespace TPC.Network.Messages
{
    public class KeyDistributeMessage : Message
    {
        public List<string> Keys { get; set; }

        public KeyDistributeMessage()
        {
            Type = MessageType.KeyDistribute;
            Keys = new List<string>();
        }
    }
}
