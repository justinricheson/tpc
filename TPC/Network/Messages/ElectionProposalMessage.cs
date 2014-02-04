using System;

namespace TPC.Network.Messages
{
    public class ElectionProposalMessage : Message
    {
        public int Proposal { get; set; }

        public ElectionProposalMessage()
        {
            Type = MessageType.ElectionProposal;
        }

        public bool IsGreaterThan(ElectionProposalMessage msg)
        {
            if (Proposal > msg.Proposal)
                return true;
            else if (Proposal == msg.Proposal)
            {
                var id1 = new Guid(SourceID);
                var id2 = new Guid(msg.SourceID);

                return id1.CompareTo(id2) > 0;
            }
            else return false;
        }
    }
}
