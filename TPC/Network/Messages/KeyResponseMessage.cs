namespace TPC.Network.Messages
{
    public class KeyResponseMessage : Message
    {
        public string Address { get; set; }
        public string Port { get; set; }
        public int KeyCount { get; set; }

        public KeyResponseMessage()
        {
            Type = MessageType.KeyResponse;
        }
    }
}
