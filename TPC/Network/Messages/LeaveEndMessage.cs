namespace TPC.Network.Messages
{
    public class LeaveEndMessage : Message
    {
        public int NodeCount { get; set; }

        public LeaveEndMessage()
        {
            Type = MessageType.LeaveEnd;
        }
    }
}
