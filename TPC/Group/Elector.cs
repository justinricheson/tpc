using System;
using System.Collections.Generic;
using System.Diagnostics;
using TPC.Network.Messages;
using TPC.Network.Sockets;
using TPC.Utilities;

namespace TPC.Group
{
    public class Elector : IDisposable
    {
        #region Vars
        private AppConfig _config;
        private ISocket _group;
        private Random _r;
        private Dictionary<string, ElectionProposalMessage> _proposals;
        private int _nodeCount;
        private int _acksRx;
        private bool _haveAcked;
        #endregion

        #region Constuctors
        public Elector(AppConfig config, ISocket group)
        {
            _config = config;
            _group = group;
            _group.OnReceiveMessage += _group_OnReceiveMessage;

            _r = new Random();
            _proposals = new Dictionary<string, ElectionProposalMessage>();
        }
        #endregion

        #region Public
        public event EventHandler OnElection;
        public void Elect()
        {
            if (_nodeCount < 2)
            {
                Debug.WriteLine("Easy elect");
                NotifyElection();
                return;
            }

            Debug.WriteLine(string.Format("Normal elect: _haveAcked={0}", _haveAcked));
            if (!_haveAcked)
                SendProposal();
        }
        public void UpdateNodeCount(int nodeCount)
        {
            _nodeCount = nodeCount;
        }
        public void Dispose()
        {
        }
        #endregion

        #region Private
        private void _group_OnReceiveMessage(object sender, ReceiveMessageEventArgs e)
        {
            if (e.Message.Type != MessageType.ElectionProposal
                && e.Message.Type != MessageType.ElectionAck
                && e.Message.Type != MessageType.ElectionEnd)
                return; // Only care about these messages

            LogBroker.Log(string.Format("RX {0} from {1}",
                e.Message.Type,
                e.Message.SourceID));

            if (e.Message.DestinationID != _config.ID && e.Message.DestinationID != Constants.NULL_DESTINATION)
                return;

            Debug.WriteLine(string.Format("RX {0} from {1}", e.Message.Type, e.Message.SourceID));
            switch (e.Message.Type)
            {
                case MessageType.ElectionProposal:
                    if (e.Message.SourceID != _config.ID) // Don't handle loopback proposals
                        HandleElectionProposal(e.Message as ElectionProposalMessage);
                    break;
                case MessageType.ElectionAck:
                    HandleElectionAck(e.Message as ElectionAckMessage);
                    break;
                case MessageType.ElectionEnd:
                    HandleElectionEnd(e.Message as ElectionEndMessage);
                    break;
            }
        }
        private void HandleElectionEnd(ElectionEndMessage msg)
        {
            Debug.WriteLine("HandleElectionEnd");
            Reset();

            if (msg.SourceID == _config.ID)
            {
                NotifyElection();
            }
        }
        private void HandleElectionAck(ElectionAckMessage msg)
        {
            Debug.WriteLine("HandleElectionAck");
            _acksRx++;

            if (_acksRx == _nodeCount - 1)
            {
                Debug.WriteLine(string.Format("Enter if: _acksRx = {0}, _nodeCount = {1}", _acksRx, _nodeCount));
                var myProposal = _proposals[_config.ID];

                bool isLeader = true;
                foreach (var proposal in _proposals.Values)
                {
                    if (proposal.SourceID != _config.ID)
                    {
                        if (proposal.IsGreaterThan(myProposal))
                        {
                            isLeader = false;
                            break;
                        }
                    }
                }

                if (isLeader)
                {
                    var joinEnd = new ElectionEndMessage()
                    {
                        SourceID = _config.ID,
                        DestinationID = Constants.NULL_DESTINATION,
                    };

                    Debug.WriteLine(string.Format("TX {0}", joinEnd.Type));
                    SendMessage(joinEnd, _group);
                }
            }
        }
        private void HandleElectionProposal(ElectionProposalMessage msg)
        {
            Debug.WriteLine("HandleElectionProposal");

            _proposals.Add(msg.SourceID, msg);

            bool ack = false;
            if (_proposals.ContainsKey(_config.ID))
            {
                Debug.WriteLine("Proposals contains my id");
                if (!_proposals[_config.ID].IsGreaterThan(msg))
                {
                    Debug.WriteLine("This id is larger than mine");
                    ack = true;
                }
            }
            else
            {
                Debug.WriteLine("Proposals does not contain my id");
                ack = true;
            }

            if(ack)
            {
                _haveAcked = true;
                var ackMsg = new ElectionAckMessage()
                {
                    SourceID = _config.ID,
                    DestinationID = msg.SourceID
                };

                Debug.WriteLine(string.Format("TX {0}", ackMsg.Type));
                SendMessage(ackMsg, _group);
            }
        }
        private void SendProposal()
        {
            var proposal = new ElectionProposalMessage()
            {
                SourceID = _config.ID,
                DestinationID = Constants.NULL_DESTINATION,
                Proposal = _r.Next(0, 1000000)
            };

            _proposals.Add(proposal.SourceID, proposal);

            Debug.WriteLine(string.Format("TX {0}", proposal.Type));
            SendMessage(proposal, _group);
        }
        private void SendMessage(Message msg, ISocket socket)
        {
            LogBroker.Log(string.Format("TX {0} to {1}",
                msg.Type,
                msg.DestinationID));
            socket.Send(Message.Encode(msg));
        }
        private void NotifyElection()
        {
            LogBroker.Log("I am leader");
            if (OnElection != null)
                OnElection(this, new EventArgs());
        }
        private void Reset()
        {
            _proposals.Clear();
            _acksRx = 0;
            _haveAcked = false;
        }
        #endregion
    }
}



// Notes
// On elect(), if i have acked any proposal, do not send my own proposal, else send proposal
// On rx proposal, add to proposals list, if i have send a proposal, see if mine is larger, if yes, dont ack, else ack, if i have not sent a proposal, ack
// On rx ack, increment acks received, if I have rx nodeCount many acks, check if mine is the largest, if yes, i am leader, else i am not leader
// On rx end, clear proposals and acks received and sendProposal/ackedProposal flags