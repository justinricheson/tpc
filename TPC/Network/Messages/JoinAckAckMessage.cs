namespace TPC.Network.Messages
{
    public class JoinAckAckMessage : Message
    {
        public JoinAckAckMessage()
        {
            Type = MessageType.JoinAckAck;
        }
    }
}
