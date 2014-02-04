using System;
using TPC.Network.Messages;

namespace TPC.Network.Sockets
{
    public delegate void ReceiveMessageDelegate(object sender, ReceiveMessageEventArgs e);
    public class ReceiveMessageEventArgs : EventArgs
    {
        public Message Message { get; set; }
    }
}
