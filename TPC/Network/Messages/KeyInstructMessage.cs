using System.Collections.Generic;

namespace TPC.Network.Messages
{
    public class KeyInstructMessage : Message
    {
        public Dictionary<string, List<Instruction>> Instructions { get; set; }

        public KeyInstructMessage()
        {
            Type = MessageType.KeyInstruct;
            Instructions = new Dictionary<string, List<Instruction>>();
        }
    }

    public class Instruction
    {
        public string Address { get; set; }
        public string Port { get; set; }
        public int KeyCount { get; set; }
    }
}
