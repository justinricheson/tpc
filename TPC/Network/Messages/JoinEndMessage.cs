namespace TPC.Network.Messages
{
    public class JoinEndMessage : Message
    {
        public int NodeCount { get; set; }

        public JoinEndMessage()
        {
            Type = MessageType.JoinEnd;
        }
    }
}
