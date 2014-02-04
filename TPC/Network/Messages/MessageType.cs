namespace TPC.Network.Messages
{
    public enum MessageType
    {
        Join, JoinAck, JoinAckAck, JoinEnd,
        KeyRequest, KeyResponse, KeyInstruct, KeyDistribute,
        ElectionProposal, ElectionAck, ElectionEnd,
        Leave, LeaveAck, LeaveEnd
    }
}
