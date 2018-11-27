using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DacLib.Generic;

namespace DacLib.Hoxis.Server
{
    class HoxisConnection : IReusable
    {
        public bool isOccupied { get; set; }

        private Socket _socket;

        public HoxisConnection()
        {

        }

        public void OnRequest(object state)
        {
            _socket = (Socket)state;
        }

        public void OnRelease()
        {

        }
    }
}
