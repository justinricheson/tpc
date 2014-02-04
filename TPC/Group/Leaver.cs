using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using TPC.Network.Messages;
using TPC.Network.Sockets;
using TPC.Utilities;
using Timers = System.Timers;

namespace TPC.Group
{
    public class Leaver : IDisposable
    {
        #region Vars
        private AppConfig _config;
        private ISocket _group;
        private Timers.Timer _t;
        private List<string> _keys;
        private bool _leaveFinished;
        #endregion

        #region Constructor
        public Leaver(AppConfig config, ISocket group, List<string> keys)
        {
            _config = config;

            _keys = keys;

            _group = group;
            _group.OnReceiveMessage += _group_OnReceiveMessage;

            _t = new Timers.Timer(Int32.Parse(_config.LeaveTimeout));
            _t.AutoReset = false;
            _t.Elapsed += _t_Elapsed;
        }
        #endregion

        #region Public
        public void Leave()
        {
            SendLeave();
            _t.Start();

            while (!_leaveFinished)
                Thread.Sleep(100);
        }
        public void Dispose()
        {
            _group.OnReceiveMessage -= _group_OnReceiveMessage;
            _leaveFinished = true;
        }
        #endregion

        #region Private
        private void _t_Elapsed(object sender, Timers.ElapsedEventArgs e)
        {
            LogBroker.Log("Leave timed out");

            _leaveFinished = true;
        }
        private void _group_OnReceiveMessage(object sender, ReceiveMessageEventArgs e)
        {
            if (e.Message.Type != MessageType.KeyInstruct)
                return; // Only care about these messages

            LogBroker.Log(string.Format("RX {0} from {1}",
                e.Message.Type,
                e.Message.SourceID));

            if (e.Message.DestinationID != _config.ID)
                return;

            switch (e.Message.Type)
            {
                case MessageType.KeyInstruct:
                    HandleKeyInstruct(e.Message as KeyInstructMessage);
                    break;
            }
        }
        private void HandleKeyInstruct(KeyInstructMessage msg)
        {
            var srcAddress = IPAddress.Parse(_config.TCPServerAddress);
            var srcPort = Int32.Parse(_config.TCPServerPort);

            var instructionSet = msg.Instructions[_config.ID];

            foreach (var instruction in instructionSet)
            {
                var dstAddress = IPAddress.Parse(instruction.Address);
                var dstPort = Int32.Parse(instruction.Port);

                using (var tcpSocket = new TCPSocket(
                    srcAddress, srcPort,
                    dstAddress, dstPort))
                {
                    var keys = _keys.Take(instruction.KeyCount).ToList();
                    _keys.RemoveRange(0, keys.Count);

                    var keyDistribute = new KeyDistributeMessage()
                    {
                        SourceID = _config.ID,
                        DestinationID = Constants.NULL_DESTINATION,
                        Keys = keys
                    };

                    SendMessage(keyDistribute, tcpSocket);
                }
            }

            _t.Stop();
            _leaveFinished = true;
        }
        private void SendLeave()
        {
            var leaveMsg = new LeaveMessage()
            {
                SourceID = _config.ID,
                DestinationID = Constants.NULL_DESTINATION,
                KeyCount = _keys.Count
            };

            SendMessage(leaveMsg, _group);
        }
        private void SendMessage(Message msg, ISocket socket)
        {
            LogBroker.Log(string.Format("TX {0} to {1}",
                msg.Type,
                msg.DestinationID));
            socket.Send(Message.Encode(msg));
        }
        #endregion
    }
}


// Notes
// Leaver sends leave message with number of keys
// Group elects leader
// Leader sends key request message to group
// Group (except for leaver) sends key response message
// Leader calculates how many keys to send to each node
// Leader sends key instruct to leaver, others set up TCP server to receive
// Leaver sends key distribute to others
// Leaver sends leave ack to leader
// Leader sends leave end to group with updated nodecount