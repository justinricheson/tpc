namespace TPC.Network.Messages
{
    public class LeaveAckMessage : Message
    {
        public LeaveAckMessage()
        {
            Type = MessageType.LeaveAck;
        }
    }
}
