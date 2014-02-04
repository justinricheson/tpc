using Newtonsoft.Json;
using System;
using System.Linq;
using TPC.Utilities;

namespace TPC.Network.Messages
{
    public abstract class Message
    {
        public MessageType Type { get; protected set; }
        public string SourceID { get; set; }
        public string DestinationID { get; set; }

        public static Message Decode(byte[] bytes)
        {
            var jsonArray = bytes.SubArray(5, bytes.Count() - 5);

            switch (bytes[0])
            {
                case Constants.JOIN_MSG_ID:
                    return JsonConvert.DeserializeObject<JoinMessage>(jsonArray.ToASCIIString());
                case Constants.JOIN_ACK_MSG_ID:
                    return JsonConvert.DeserializeObject<JoinAckMessage>(jsonArray.ToASCIIString());
                case Constants.JOIN_ACK_ACK_MSG_ID:
                    return JsonConvert.DeserializeObject<JoinAckAckMessage>(jsonArray.ToASCIIString());
                case Constants.JOIN_END_MSG_ID:
                    return JsonConvert.DeserializeObject<JoinEndMessage>(jsonArray.ToASCIIString());
                case Constants.KEY_REQUEST_MSG_ID:
                    return JsonConvert.DeserializeObject<KeyRequestMessage>(jsonArray.ToASCIIString());
                case Constants.KEY_RESPONSE_MSG_ID:
                    return JsonConvert.DeserializeObject<KeyResponseMessage>(jsonArray.ToASCIIString());
                case Constants.KEY_INSTRUCT_MSG_ID:
                    return JsonConvert.DeserializeObject<KeyInstructMessage>(jsonArray.ToASCIIString());
                case Constants.KEY_DISTRIBUTE_MSG_ID:
                    return JsonConvert.DeserializeObject<KeyDistributeMessage>(jsonArray.ToASCIIString());
                case Constants.ELECTION_PROPOSAL_MSG_ID:
                    return JsonConvert.DeserializeObject<ElectionProposalMessage>(jsonArray.ToASCIIString());
                case Constants.ELECTION_ACK_MSG_ID:
                    return JsonConvert.DeserializeObject<ElectionAckMessage>(jsonArray.ToASCIIString());
                case Constants.ELECTION_END_MSG_ID:
                    return JsonConvert.DeserializeObject<ElectionEndMessage>(jsonArray.ToASCIIString());
                case Constants.LEAVE_MSG_ID:
                    return JsonConvert.DeserializeObject<LeaveMessage>(jsonArray.ToASCIIString());
                case Constants.LEAVE_ACK_MSG_ID:
                    return JsonConvert.DeserializeObject<LeaveAckMessage>(jsonArray.ToASCIIString());
                case Constants.LEAVE_END_MSG_ID:
                    return JsonConvert.DeserializeObject<LeaveEndMessage>(jsonArray.ToASCIIString());
                default:
                    throw new ArgumentException("Unidentified message type");
            }
        }

        public static byte[] Encode(Message message)
        {
            var msgid = new byte[] { MessageTypeToByteMap.Map(message.Type) };
            var bytes = ToJson(message).ToByteArray();
            var length = BitConverter.GetBytes(bytes.Length);

            var start = 0;
            byte[] encMsg = new byte[1 + length.Length + bytes.Length];
            Buffer.BlockCopy(msgid, 0, encMsg, start, msgid.Length);
            Buffer.BlockCopy(length, 0, encMsg, start += msgid.Length, length.Length);
            Buffer.BlockCopy(bytes, 0, encMsg, start += length.Length, bytes.Length);

            return encMsg;
        }

        public static string ToJson(Message message)
        {
            return JsonConvert.SerializeObject(message);
        }
    }
}
