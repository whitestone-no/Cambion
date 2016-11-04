using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Backend.Loopback
{
    public static class TransportInitializer
    {
        public static void UseLoopback(this IMessageHandlerInitializer initializer)
        {
            Transport transport = new Transport();
            initializer.Transport = transport;
        }
    }
}
