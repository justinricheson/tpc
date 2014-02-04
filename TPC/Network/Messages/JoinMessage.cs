using System.Net;

namespace TPC.Network.Messages
{
    public class JoinMessage : Message
    {
        public string Address { get; set; }
        public string Port { get; set; }

        public JoinMessage()
        {
            Type = MessageType.Join;
        }
    }
}
