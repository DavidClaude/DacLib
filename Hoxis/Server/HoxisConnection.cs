using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DacLib.Generic;

namespace DacLib.Hoxis.Server
{
    class HoxisConnection : IReceivable
    {
        public bool isUpdated { get; set; }

        private Socket _socket;

        public HoxisConnection(Socket socketArg)
        {
            _socket = socketArg;
        }

        public void OnAccept()
        {
            
        }

        public void OnDecline()
        {
            
        }

        public void OnService()
        {
            
        }
    }
}
