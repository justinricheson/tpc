using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPC.Network.Messages
{
    public class ElectionAckMessage : Message
    {
        public ElectionAckMessage()
        {
            Type = MessageType.ElectionAck;
        }
    }
}
