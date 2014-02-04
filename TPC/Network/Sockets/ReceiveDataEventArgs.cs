using System;

namespace TPC.Network.Sockets
{
    public delegate void ReceiveDataDelegate(object sender, ReceiveDataEventArgs e);
    public class ReceiveDataEventArgs : EventArgs
    {
        public byte[] Data { get; set; }
    }
}
