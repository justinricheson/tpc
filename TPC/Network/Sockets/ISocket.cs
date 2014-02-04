using System;
namespace TPC.Network.Sockets
{
    public interface ISocket : IDisposable
    {
        void Send(byte[] data);
        event ReceiveMessageDelegate OnReceiveMessage;
    }
}
