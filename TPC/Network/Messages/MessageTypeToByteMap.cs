using System.Collections.Generic;

namespace TPC.Network.Messages
{
    public static class MessageTypeToByteMap
    {
        public static Dictionary<MessageType, byte> _messageTypeToByteMap;

        static MessageTypeToByteMap()
        {
            _messageTypeToByteMap = new Dictionary<MessageType, byte>();
            Init();
        }

        public static byte Map(MessageType type)
        {
            return _messageTypeToByteMap[type];
        }

        private static void Init()
        {
            _messageTypeToByteMap.Add(MessageType.Join, Constants.JOIN_MSG_ID);
            _messageTypeToByteMap.Add(MessageType.JoinAck, Constants.JOIN_ACK_MSG_ID);
            _messageTypeToByteMap.Add(MessageType.JoinAckAck, Constants.JOIN_ACK_ACK_MSG_ID);
            _messageTypeToByteMap.Add(MessageType.JoinEnd, Constants.JOIN_END_MSG_ID);
            _messageTypeToByteMap.Add(MessageType.KeyRequest, Constants.KEY_REQUEST_MSG_ID);
            _messageTypeToByteMap.Add(MessageType.KeyResponse, Constants.KEY_RESPONSE_MSG_ID);
            _messageTypeToByteMap.Add(MessageType.KeyInstruct, Constants.KEY_INSTRUCT_MSG_ID);
            _messageTypeToByteMap.Add(MessageType.KeyDistribute, Constants.KEY_DISTRIBUTE_MSG_ID);
            _messageTypeToByteMap.Add(MessageType.ElectionProposal, Constants.ELECTION_PROPOSAL_MSG_ID);
            _messageTypeToByteMap.Add(MessageType.ElectionAck, Constants.ELECTION_ACK_MSG_ID);
            _messageTypeToByteMap.Add(MessageType.ElectionEnd, Constants.ELECTION_END_MSG_ID);
            _messageTypeToByteMap.Add(MessageType.Leave, Constants.LEAVE_MSG_ID);
            _messageTypeToByteMap.Add(MessageType.LeaveAck, Constants.LEAVE_ACK_MSG_ID);
            _messageTypeToByteMap.Add(MessageType.LeaveEnd, Constants.LEAVE_END_MSG_ID);
        }
    }
}
