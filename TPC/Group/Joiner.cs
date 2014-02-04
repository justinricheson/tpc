using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using TPC.Network.Messages;
using TPC.Network.Sockets;
using TPC.Utilities;
using Timers = System.Timers;

namespace TPC.Group
{
    public class Joiner : IDisposable
    {
        #region Vars
        private AppConfig _config;
        private ISocket _group;
        private TCPServer _listener;
        private bool _joinSucceeded;
        private bool _joinFinished;
        private Timers.Timer _t;
        private List<string> _keys;
        private JoinAckMessage _joinAck;
        private int _actualResponses;
        private int _nodeCount;
        #endregion

        #region Constructor
        public Joiner(AppConfig config, ISocket group)
        {
            _config = config;

            _group = group;
            _group.OnReceiveMessage += _group_OnReceiveMessage;

            _listener = new TCPServer(
                IPAddress.Parse(_config.TCPServerAddress),
                Int32.Parse(_config.TCPServerPort));
            _listener.OnReceiveMessage += _listener_OnReceiveMessage;

            _t = new Timers.Timer(Int32.Parse(config.JoinTimeout));
            _t.AutoReset = false;
            _t.Elapsed += _t_Elapsed;

            _keys = new List<string>();
        }
        #endregion

        #region Public
        public bool Join()
        {
            SendJoin();
            _t.Start();

            while (!_joinFinished)
                Thread.Sleep(100);

            return _joinSucceeded;
        }
        public List<string> GetKeys()
        {
            return _keys;
        }
        public int GetNodeCount()
        {
            return _nodeCount;
        }
        public void Dispose()
        {
            _group.OnReceiveMessage -= _group_OnReceiveMessage;
            _listener.Dispose();
            _joinFinished = true;
        }
        #endregion

        #region Private
        private void SendJoin()
        {
            var joinMsg = new JoinMessage()
            {
                SourceID = _config.ID,
                DestinationID = Constants.NULL_DESTINATION,
                Address = _config.TCPServerAddress,
                Port = _config.TCPServerPort
            };

            SendMessage(joinMsg, _group);
        }
        private void _t_Elapsed(object sender, Timers.ElapsedEventArgs e)
        {
            LogBroker.Log("Join timed out");

            _joinSucceeded = false;
            _joinFinished = true;
        }
        private void _group_OnReceiveMessage(object sender, ReceiveMessageEventArgs e)
        {
            if (e.Message.Type != MessageType.JoinAck
                && e.Message.Type != MessageType.JoinEnd)
                return; // Only care about these messages

            LogBroker.Log(string.Format("RX {0} from {1}",
                e.Message.Type,
                e.Message.SourceID));

            if (e.Message.DestinationID != _config.ID && e.Message.DestinationID != Constants.NULL_DESTINATION)
                return;

            switch (e.Message.Type)
            {
                case MessageType.JoinAck:
                    _joinAck = e.Message as JoinAckMessage;
                    break;
                case MessageType.JoinEnd:
                     _t.Stop();
                     _nodeCount = (e.Message as JoinEndMessage).NodeCount;
                    _joinSucceeded = true;
                    _joinFinished = true;
                    break;
            }
        }
        private void _listener_OnReceiveMessage(object sender, ReceiveMessageEventArgs e)
        {
            LogBroker.Log(string.Format("RX {0} from {1}",
                e.Message.Type,
                e.Message.SourceID));

            switch (e.Message.Type)
            {
                case MessageType.KeyDistribute:
                    AddKeys((e.Message as KeyDistributeMessage).Keys);
                    _actualResponses++;
                    break;
            }

            if (_actualResponses == _joinAck.ExpectedResponses)
            {
                var joinAckAckMsg = new JoinAckAckMessage()
                {
                    SourceID = _config.ID,
                    DestinationID = _joinAck.SourceID
                };

                SendMessage(joinAckAckMsg, _group);
            }
        }
        private void AddKeys(List<string> keys)
        {
            foreach (var key in keys)
            {
                if (!_keys.Contains(key))
                    _keys.Add(key);
            }
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
