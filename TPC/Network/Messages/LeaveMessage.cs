using System.Net;

namespace TPC.Network.Messages
{
    public class LeaveMessage : Message
    {
        public int KeyCount { get; set; }

        public LeaveMessage()
        {
            Type = MessageType.Leave;
        }
    }
}
