using System;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Backend.Loopback
{
    public class Transport : IBackendTransport
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public void Publish(byte[] data)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(data));
        }

        public void Dispose()
        {
        }
    }
}
