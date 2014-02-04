namespace TPC.Network.Messages
{
    public class KeyRequestMessage : Message
    {
        public KeyRequestMessage()
        {
            Type = MessageType.KeyRequest;
        }
    }
}
