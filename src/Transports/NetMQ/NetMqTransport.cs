using System;
using Whitestone.Cambion.Common.Events;
using Whitestone.Cambion.Common.Interfaces;
using Whitestone.Cambion.Common.Types;

namespace Whitestone.Cambion.Transports.NetMQ
{
    public class NetMqTransport : ITransport
    {
        public ISerializer Serializer { get; set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public NetMqTransport(string publishAddress, string subscribeAddress)
        {

        }

        public void Publish(MessageWrapper message)
        {

        }

        public void Dispose()
        {
            
        }
    }
}
