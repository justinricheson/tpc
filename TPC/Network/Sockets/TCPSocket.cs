using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TPC.Utilities;

namespace TPC.Network.Sockets
{
    public class TCPSocket : ISocket, IDisposable
    {
        #region Vars
        private MessageBuilder _builder;
        private TcpClient _client;
        private NetworkStream _stream;
        private Thread _t;
        private bool _running;
        #endregion

        #region Constructor
        public TCPSocket(TcpClient client)
        {
            _running = true;
            _client = client;
            _stream = _client.GetStream();
            _builder = new MessageBuilder();
            _builder.OnReceiveMessage += _builder_OnReceiveMessage;
            _t = new Thread(Receive);
            _t.Start();
        }
        public TCPSocket(IPAddress srcAddress, int srcPort, IPAddress dstAddress, int dstPort)
            : this(CreateClient(srcAddress, srcPort, dstAddress, dstPort))
        {
        }
        private static TcpClient CreateClient(IPAddress srcAddress, int srcPort, IPAddress dstAddress, int dstPort)
        {
            var client = new TcpClient(
                new IPEndPoint(srcAddress, srcPort));

            client.LingerState = new LingerOption(false, 0);

            client.Connect(
                new IPEndPoint(dstAddress, dstPort));

            return client;
        }
        #endregion

        #region Private
        private void _builder_OnReceiveMessage(object sender, ReceiveMessageEventArgs e)
        {
            if (OnReceiveMessage != null)
                OnReceiveMessage(this, e);
        }
        private void Receive()
        {
            while (_running)
            {
                try
                {
                    int c;
                    var buffer = new byte[8192];
                    if ((c = _stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        buffer = buffer.Take(c).ToArray();
                        _builder.Append(buffer);
                    }
                    else
                    {
                        Dispose();
                    }
                }
                catch
                {
                    //LogBroker.Log("Exception in TCP socket receive thread: probably socket.Dispose()");
                }
            }
        }
        #endregion

        #region Public
        public void Send(byte[] data)
        {
            _stream.Write(data, 0, data.Length);
            _stream.Flush();
        }
        public void Dispose()
        {
            //LogBroker.Log("TCP Socket disposed");

            _running = false;

            try
            {
                _client.Close();
            }
            catch { }

            if (OnClose != null)
                OnClose(this, new EventArgs());
        }
        public event ReceiveMessageDelegate OnReceiveMessage;
        public event EventHandler OnClose;
        #endregion
    }
}
