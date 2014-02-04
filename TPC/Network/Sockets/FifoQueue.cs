using System.Collections.Generic;

namespace TPC.Network.Sockets
{
    public class FifoQueue
    {
        #region Vars
        private int _nextSeqNo;
        private Dictionary<int, byte[]> _queue;
        #endregion

        #region Constructor
        public FifoQueue()
        {
            _nextSeqNo = 0;
            _queue = new Dictionary<int, byte[]>();
        }
        #endregion

        #region Public
        public event ReceiveDataDelegate OnReceiveData;
        public void Queue(byte[] message, int seqNo)
        {
            // Disable FIFO for now
            NotifyReceiveData(message);
            return;

            if (seqNo == _nextSeqNo)
            {
                _nextSeqNo++;
                NotifyReceiveData(message);

                while (_queue.ContainsKey(_nextSeqNo))
                {
                    var data = _queue[_nextSeqNo];
                    _queue.Remove(_nextSeqNo);

                    NotifyReceiveData(data);

                    _nextSeqNo++;
                }
            }
            else
                _queue.Add(seqNo, message);
        }
        #endregion

        #region Private
        public void NotifyReceiveData(byte[] data)
        {
            if (OnReceiveData != null)
                OnReceiveData(this, new ReceiveDataEventArgs() { Data = data });
        }
        #endregion
    }
}
