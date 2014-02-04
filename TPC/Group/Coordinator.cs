using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Timers;
using TPC.Network.Messages;
using TPC.Network.Sockets;
using TPC.Processor;
using TPC.Utilities;

namespace TPC.Group
{
    public class Coordinator : IDisposable
    {
        #region Vars
        private AppConfig _config;
        private MD5HashTableProcessor _processor;
        private ISocket _group;

        private List<KeyResponseMessage> _responses;
        private Timer _responseTimeout;

        private Joiner _joiner;
        private JoinMessage _joinMsg;
        private bool _isJoined;
        
        private Elector _elector;

        private Leaver _leaver;
        private LeaveMessage _leaveMsg;
        private TCPServer _tcpServer; // To listen for key distributions when nodes leave
        private int _leaveAcksExpected;
        private int _leaveAcksReceived;

        private int _nodeCount;
        #endregion

        #region Constructor
        public Coordinator()
        {
            _config = AppConfig.GetConfig();
            LogBroker.Log(string.Format("My ID: {0}", _config.ID));

            _responses = new List<KeyResponseMessage>();
            _responseTimeout = new Timer(Int32.Parse(_config.ResponseTimeout));
            _responseTimeout.AutoReset = false;
            _responseTimeout.Elapsed += _responseTimeout_Elapsed;
        }
        #endregion

        #region Public
        public void Join()
        {
            if (_isJoined)
                return;

            _group = new MulticastSocket(
                IPAddress.Parse(_config.GroupMcg),
                Int32.Parse(_config.GroupPort));

            List<string> keys;
            using (_joiner = new Joiner(_config, _group))
            {
                bool joined = _joiner.Join();
                if (joined)
                {
                    _nodeCount = _joiner.GetNodeCount();
                    keys = _joiner.GetKeys();
                }
                else
                {
                    _nodeCount = 1; // Won't rx joinend so need to update here
                    keys = MD5HashTableProcessor.GenerateKeys();
                }
            }

            _elector = new Elector(_config, _group);
            _elector.OnElection += _elector_OnElection;
            UpdateNodeCount(_nodeCount);

            // Do this after joiner is disposed since it uses the same socket
            _group.OnReceiveMessage += _group_OnReceiveMessage;

            _processor = new MD5HashTableProcessor();
            _processor.Add(keys);
            _processor.OnProcessComplete += _processor_OnProcessComplete;
            _processor.Start();

            _isJoined = true;

            LogBroker.Log("Join Complete");
        }
        public void Leave()
        {
            if (!_isJoined)
                return;

            _processor.Stop();
            var keys = _processor.Get(_processor.KeyCount);

            // Do this before instantiating leaver since it uses the same socket
            _group.OnReceiveMessage -= _group_OnReceiveMessage;

            // If we're the only node, there is no need to redistribute keys
            if (_nodeCount > 1)
            {
                using (var _leaver = new Leaver(_config, _group, keys))
                    _leaver.Leave();
            }

            // Kill socket so we don't respond to group messages
            _group.Dispose();

            _isJoined = false;

            LogBroker.Log(string.Format("{0} keys processed", _processor.ProcessedKeyCount));
            LogBroker.Log("Leave Complete");
        }
        public void Dispose()
        {
            if (_joiner != null)
                _joiner.Dispose();

            if (_leaver != null)
                _leaver.Dispose();

            if (_elector != null)
                _elector.Dispose();

            if (_group != null)
                _group.Dispose();
        }
        #endregion

        #region Private
        private void _processor_OnProcessComplete(object sender, ProcessCompleteEventArgs<string, string> e)
        {
            LogBroker.Log(string.Format("Processing complete: {0} keys processed", e.Results.Count));
        }
        private void _group_OnReceiveMessage(object sender, ReceiveMessageEventArgs e)
        {
            if (e.Message.Type != MessageType.Join
                && e.Message.Type != MessageType.KeyRequest
                && e.Message.Type != MessageType.KeyResponse
                && e.Message.Type != MessageType.KeyInstruct
                && e.Message.Type != MessageType.JoinAckAck
                && e.Message.Type != MessageType.JoinEnd
                && e.Message.Type != MessageType.Leave
                && e.Message.Type != MessageType.LeaveAck
                && e.Message.Type != MessageType.LeaveEnd)
                return; // Only care about these messages

            if (e.Message.DestinationID != _config.ID && e.Message.DestinationID != Constants.NULL_DESTINATION)
                return;

            LogBroker.Log(string.Format("RX {0} from {1}",
                e.Message.Type,
                e.Message.SourceID));

            switch (e.Message.Type)
            {
                case MessageType.Join:
                    HandleJoin(e.Message as JoinMessage);
                    break;
                case MessageType.KeyRequest:
                    HandleKeyRequest(e.Message as KeyRequestMessage);
                    break;
                case MessageType.KeyResponse:
                    HandleKeyResponse(e.Message as KeyResponseMessage);
                    break;
                case MessageType.KeyInstruct:
                    HandleKeyInstruct(e.Message as KeyInstructMessage);
                    break;
                case MessageType.JoinAckAck:
                    HandleJoinAckAck(e.Message as JoinAckAckMessage);
                    break;
                case MessageType.JoinEnd:
                    HandleJoinEnd(e.Message as JoinEndMessage);
                    break;
                case MessageType.Leave:
                    HandleLeave(e.Message as LeaveMessage);
                    break;
                case MessageType.LeaveAck:
                    HandleLeaveAck(e.Message as LeaveAckMessage);
                    break;
                case MessageType.LeaveEnd:
                    HandleLeaveEnd(e.Message as LeaveEndMessage);
                    break;
            }
        }
        private void _responseTimeout_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_joinMsg != null)
                HandleJoinKeyRequestComplete();
            else
                HandleLeaveKeyRequestComplete();
        }
        private void _tcpServer_OnReceiveMessage(object sender, ReceiveMessageEventArgs e)
        {
            LogBroker.Log(string.Format("RX {0} from {1}",
                e.Message.Type,
                e.Message.SourceID));

            switch (e.Message.Type)
            {
                case MessageType.KeyDistribute:
                    var msg = e.Message as KeyDistributeMessage;
                    _processor.Add(msg.Keys);

                    var leaveAck = new LeaveAckMessage()
                    {
                        SourceID = _config.ID,
                        DestinationID = Constants.NULL_DESTINATION
                    };

                    SendMessage(leaveAck, _group);
                    break;
            }
        }
        private void _elector_OnElection(object sender, EventArgs e)
        {
            _responses.Clear();
            if (!_responseTimeout.Enabled)
                _responseTimeout.Start();

            var dataReqMsg = new KeyRequestMessage()
            {
                SourceID = _config.ID,
                DestinationID = Constants.NULL_DESTINATION
            };

            SendMessage(dataReqMsg, _group);
        }
        private void HandleLeaveKeyRequestComplete()
        {
            var sum = (from msg in _responses select msg.KeyCount).Sum() + _leaveMsg.KeyCount;
            var avg = Math.Floor(sum / (double)(_responses.Count));

            var instructions = new Dictionary<string, List<Instruction>>();
            if (_leaveMsg.KeyCount > 0)
            {
                foreach (var msg in _responses)
                {
                    int diff = (int)avg - msg.KeyCount;
                    if (diff > 0)
                    {
                        if(!instructions.ContainsKey(_leaveMsg.SourceID))
                            instructions.Add(_leaveMsg.SourceID, new List<Instruction>());

                        var instruction = new Instruction()
                        {
                            Address = msg.Address,
                            Port = msg.Port,
                            KeyCount = diff
                        };

                        instructions[_leaveMsg.SourceID].Add(instruction);
                    }
                }

                // Because we're flooring the average, there may be unassigned keys
                // So just send whatever is left to the first node in the list
                var instructionSum = (from instructionSet in instructions.Values
                                      from instruction in instructionSet
                                      select instruction.KeyCount).Sum();
                if (instructionSum < _leaveMsg.KeyCount)
                {
                    var diff = _leaveMsg.KeyCount - instructionSum;
                    instructions.Values.First().First().KeyCount += diff;
                }
            }

            if (instructions.Keys.Count > 0)
            {
                _leaveAcksReceived = 0;
                _leaveAcksExpected = 0;
                foreach (var set in instructions.Values)
                    _leaveAcksExpected += set.Count;

                var instruct = new KeyInstructMessage()
                {
                    SourceID = _config.ID,
                    DestinationID = _leaveMsg.SourceID,
                    Instructions = instructions
                };

                SendMessage(instruct, _group);
            }
        }
        private void HandleJoinKeyRequestComplete()
        {
            var sum = (from msg in _responses select msg.KeyCount).Sum();
            var avg = Math.Floor(sum / (_responses.Count + 1d)); // + 1 is for the joiner

            var instructions = new Dictionary<string, List<Instruction>>();
            if (avg > _responses.Count + 1) // + 1 is for the joiner
            {
                foreach (var msg in _responses)
                {
                    int diff = msg.KeyCount - (int)avg;
                    if (diff > 0)
                    {
                        if(!instructions.ContainsKey(msg.SourceID))
                            instructions.Add(msg.SourceID, new List<Instruction>());

                        var instruction = new Instruction()
                        {
                            Address = _joinMsg.Address,
                            Port = _joinMsg.Port,
                            KeyCount = diff
                        };

                        instructions[msg.SourceID].Add(instruction);
                    }
                }
            }

            if (instructions.Keys.Count > 0)
            {
                var distMsg = new KeyInstructMessage()
                {
                    SourceID = _config.ID,
                    DestinationID = Constants.NULL_DESTINATION,
                    Instructions = instructions
                };

                SendMessage(distMsg, _group);
            }

            var joinAck = new JoinAckMessage()
            {
                SourceID = _config.ID,
                DestinationID = _joinMsg.SourceID,
                ExpectedResponses = instructions.Keys.Count
            };
            SendMessage(joinAck, _group);
        }
        private void HandleLeaveEnd(LeaveEndMessage msg)
        {
            _tcpServer.Dispose();

            UpdateNodeCount(msg.NodeCount);
            _processor.Start();
        }
        private void HandleLeaveAck(LeaveAckMessage msg)
        {
            _leaveAcksReceived++;

            if (_leaveAcksReceived == _leaveAcksExpected)
            {
                var leaveEnd = new LeaveEndMessage()
                {
                    SourceID = _config.ID,
                    DestinationID = Constants.NULL_DESTINATION,
                    NodeCount = _nodeCount - 1
                };

                SendMessage(leaveEnd, _group);
            }
        }
        private void HandleLeave(LeaveMessage msg)
        {
            _tcpServer = new TCPServer(
                IPAddress.Parse(_config.TCPServerAddress),
                Int32.Parse(_config.TCPServerPort));
            _tcpServer.OnReceiveMessage += _tcpServer_OnReceiveMessage;

            _joinMsg = null;
            _leaveMsg = msg;
            _elector.Elect();
        }
        private void HandleJoinEnd(JoinEndMessage msg)
        {
            UpdateNodeCount(msg.NodeCount);
            _processor.Start();
        }
        private void HandleJoinAckAck(JoinAckAckMessage msg)
        {
            var joinEndMsg = new JoinEndMessage()
            {
                SourceID = _config.ID,
                DestinationID = Constants.NULL_DESTINATION,
                NodeCount = _nodeCount + 1
            };

            SendMessage(joinEndMsg, _group);
        }
        private void HandleKeyInstruct(KeyInstructMessage msg)
        {
            if (!msg.Instructions.ContainsKey(_config.ID))
                return;

            var instructionSet = msg.Instructions[_config.ID];
            foreach (var instruction in instructionSet)
            {
                int count = instruction.KeyCount;
                var keys = _processor.Get(count).ToList();

                var srcAddress = IPAddress.Parse(_config.TCPServerAddress);
                var srcPort = Int32.Parse(_config.TCPServerPort);
                var dstAddress = IPAddress.Parse(instruction.Address);
                var dstPort = Int32.Parse(instruction.Port);
                using (var tcpSocket = new TCPSocket(
                    srcAddress, srcPort,
                    dstAddress, dstPort))
                {
                    var distMsg = new KeyDistributeMessage()
                    {
                        SourceID = _config.ID,
                        DestinationID = Constants.NULL_DESTINATION,
                        Keys = keys
                    };

                    SendMessage(distMsg, tcpSocket);
                }
            }
        }
        private void HandleKeyResponse(KeyResponseMessage msg)
        {
            _responses.Add(msg);
        }
        private void HandleKeyRequest(KeyRequestMessage msg)
        {
            _processor.Stop();
            int c = _processor.KeyCount;

            var dataRspMsg = new KeyResponseMessage()
            {
                SourceID = _config.ID,
                DestinationID = msg.SourceID,
                Address = _config.TCPServerAddress,
                Port = _config.TCPServerPort,
                KeyCount = c
            };

            SendMessage(dataRspMsg, _group);
        }
        private void HandleJoin(JoinMessage msg)
        {
            _leaveMsg = null;
            _joinMsg = msg;
            _elector.Elect();
        }
        private void SendMessage(Message msg, ISocket socket)
        {
            LogBroker.Log(string.Format("TX {0} to {1}",
                msg.Type,
                msg.DestinationID));
            socket.Send(Message.Encode(msg));
        }
        private void UpdateNodeCount(int nodeCount)
        {
            LogBroker.Log(string.Format("NodeCount = {0}", nodeCount));
            _nodeCount = nodeCount;
            _elector.UpdateNodeCount(nodeCount);
        }
        #endregion
    }
}
