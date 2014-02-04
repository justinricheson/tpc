namespace TPC.Network.Messages
{
    public static class Constants
    {
        public const byte JOIN_MSG_ID = 0x10;
        public const byte JOIN_ACK_MSG_ID = 0x11;
        public const byte JOIN_ACK_ACK_MSG_ID = 0x12;
        public const byte JOIN_END_MSG_ID = 0x13;
        public const byte KEY_REQUEST_MSG_ID = 0x14;
        public const byte KEY_RESPONSE_MSG_ID = 0x15;
        public const byte KEY_INSTRUCT_MSG_ID = 0x16;
        public const byte KEY_DISTRIBUTE_MSG_ID = 0x17;
        public const byte ELECTION_PROPOSAL_MSG_ID = 0x18;
        public const byte ELECTION_ACK_MSG_ID = 0x19;
        public const byte ELECTION_END_MSG_ID = 0x1A;
        public const byte LEAVE_MSG_ID = 0x1B;
        public const byte LEAVE_ACK_MSG_ID = 0x1C;
        public const byte LEAVE_END_MSG_ID = 0x1D;
    }
}
