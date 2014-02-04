using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TPC.Utilities;

namespace TPC.Network.Sockets
{
    public class TCPServer : IDisposable
    {
        #region Vars
        private List<TCPSocket> _connections;
        private IPAddress _address;
        private int _port;
        private Thread _t;
        private bool _running;
        private TcpListener _listener;
        #endregion

        #region Constructor
        public TCPServer(IPAddress address, int port)
        {
            _address = address;
            _port = port;
            _connections = new List<TCPSocket>();
            _listener = new TcpListener(address, port);
            _listener.Start();

            _running = true;
            _t = new Thread(Listen);
            _t.Start();
        }
        #endregion

        #region Private
        private void Listen()
        {
            while (_running)
            {
                try
                {
                    var client = _listener.AcceptTcpClient();
                    var socket = new TCPSocket(client);
                    socket.OnClose += socket_OnClose;
                    socket.OnReceiveMessage += socket_OnReceiveMessage;
                    _connections.Add(socket);
                }
                catch
                {
                    //LogBroker.Log("Exception in TCP Server listen thread: probably server.Dispose()");
                }
            }
        }
        private void socket_OnReceiveMessage(object sender, ReceiveMessageEventArgs e)
        {
            if (OnReceiveMessage != null)
                OnReceiveMessage(this, e);
        }
        private void socket_OnClose(object sender, EventArgs e)
        {
            _connections.Remove(sender as TCPSocket);
        }
        #endregion

        #region Public
        public event ReceiveMessageDelegate OnReceiveMessage;
        public void Dispose()
        {
            //LogBroker.Log("TCP Server disposed");

            _running = false;
            try
            {
                _listener.Stop();
            }
            catch { }

            foreach (var socket in _connections)
                socket.Dispose();
        }
        #endregion
    }
}
