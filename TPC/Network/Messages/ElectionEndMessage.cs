namespace TPC.Network.Messages
{
    public class ElectionEndMessage : Message
    {
        public ElectionEndMessage()
        {
            Type = MessageType.ElectionEnd;
        }
    }
}
