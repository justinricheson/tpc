namespace TPC.Network.Messages
{
    public class JoinAckMessage : Message
    {
        public int ExpectedResponses { get; set; }
        
        public JoinAckMessage()
        {
            Type = MessageType.JoinAck;
        }
    }
}
