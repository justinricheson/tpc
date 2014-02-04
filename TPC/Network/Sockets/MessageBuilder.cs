using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TPC.Network.Messages;
using TPC.Utilities;

namespace TPC.Network.Sockets
{
    public class MessageBuilder
    {
        #region Vars
        private List<byte> _buffer;
        private Dictionary<string, FifoQueue> _fifoQueues;
        #endregion

        #region Constructor
        public MessageBuilder()
        {
            _buffer = new List<byte>();
            _fifoQueues = new Dictionary<string, FifoQueue>();
        }
        #endregion

        #region Public
        public event ReceiveMessageDelegate OnReceiveMessage;
        public void Append(byte[] data)
        {
            _buffer.AddRange(data);

            while (true)
            {
                if (_buffer.Count < 5)
                    break;

                int length = BitConverter.ToInt32(_buffer.Skip(1).Take(4).ToArray(), 0);

                if (_buffer.Count >= length + 5)
                {
                    var message = _buffer.Take(length + 5).ToArray();
                    _buffer.RemoveRange(0, length + 5);

                    Queue(message);
                }
                else
                {
                    LogBroker.Log("RX message fragment");
                    break;
                }
            }
        }
        public void Clear()
        {
            _buffer.Clear();
            _fifoQueues.Clear();
        }
        #endregion

        #region Private
        private void queue_OnReceiveData(object sender, ReceiveDataEventArgs e)
        {
            if (OnReceiveMessage != null)
            {
                var args = new ReceiveMessageEventArgs() { Message = Message.Decode(e.Data) };
                OnReceiveMessage(this, args);
            }
        }
        private void Queue(byte[] message)
        {
            // Disable FIFO for now
            //int seqNo = BitConverter.ToInt32(message.Skip(5).Take(4).ToArray(), 0);
            //string id = Encoding.ASCII.GetString(message.Skip(9).Take(36).ToArray());

            //if (!_fifoQueues.ContainsKey(id))
            //{
            //    var queue = new FifoQueue();
            //    queue.OnReceiveData += queue_OnReceiveData;
            //    _fifoQueues.Add(id, queue);
            //}
            //_fifoQueues[id].Queue(message, seqNo);

            if (_fifoQueues.Keys.Count < 1)
            {
                _fifoQueues.Add("", new FifoQueue());
                _fifoQueues.Values.First().OnReceiveData += queue_OnReceiveData;
            }

            _fifoQueues.Values.First().Queue(message, 1);
        }
        #endregion
    }
}
