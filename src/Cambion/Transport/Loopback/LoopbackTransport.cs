using System;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Transport.Loopback
{
    public class LoopbackTransport : ITransport
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public void Start() { }
        public void Stop() { }

        public void Publish(byte[] messageBytes)
        {
            if (messageBytes == null)
            {
                throw new ArgumentNullException(nameof(messageBytes));
            }

            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(messageBytes));
        }
    }
}
