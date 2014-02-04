using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TPC.Utilities;

namespace TPC.Network.Sockets
{
    public class MulticastSocket : IDisposable, ISocket
    {
        #region Vars
        private Socket _sender;
        private UdpClient _receiver;
        private Thread _t;
        private MessageBuilder _builder;
        private bool _running;
        #endregion
        
        #region Public
        public event ReceiveMessageDelegate OnReceiveMessage;
        public MulticastSocket(IPAddress mcastAddress, int port)
        {
            _running = true;
            _builder = new MessageBuilder(); // TODO inject the interface later
            _builder.OnReceiveMessage += _builder_OnReceiveMessage;

            _sender = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp);

            _sender.SetSocketOption(
                SocketOptionLevel.IP,
                SocketOptionName.AddMembership,
                new MulticastOption(mcastAddress, IPAddress.Any));

            _sender.SetSocketOption(
                SocketOptionLevel.IP,
                SocketOptionName.MulticastTimeToLive,
                2);

            _sender.Connect(
                new IPEndPoint(mcastAddress, port));

            _receiver = new UdpClient();

            _receiver.Client.Bind(
                new IPEndPoint(IPAddress.Any, port));

            _receiver.JoinMulticastGroup(mcastAddress);

            _t = new Thread(Receive);
            _t.Name = "MulticastSocket";
            _t.Start();
        }
        public void Send(byte[] data)
        {
            _sender.Send(data, data.Length, SocketFlags.None);
        }
        public void Dispose()
        {
            //LogBroker.Log("Multicast Socket disposed");

            _running = false;

            try
            {
                _sender.Close();
                _receiver.Close();
            }
            catch { }
        }
        #endregion

        #region Private
        private void Receive()
        {
            while (_running)
            {
                try
                {
                    int c;
                    var buffer = new byte[8192];
                    if ((c = _receiver.Client.Receive(buffer)) > 0)
                    {
                        buffer = buffer.Take(c).ToArray();
                        _builder.Append(buffer);
                    }
                }
                catch (SocketException se)
                {
                    if (se.ErrorCode != 10004)
                        LogBroker.Log("SocketException in multicast socket receive thread");
                    //else
                        //LogBroker.Log("SocketException in multicast socket receive thread: Probably socket.Dispose()");
                }
                catch (Exception)
                {
                    LogBroker.Log("Exception in multicast socket receive thread");
                }
            }
        }
        private void _builder_OnReceiveMessage(object sender, ReceiveMessageEventArgs e)
        {
            if (OnReceiveMessage != null)
                OnReceiveMessage(this, e);
        }
        #endregion Private
    }
}
