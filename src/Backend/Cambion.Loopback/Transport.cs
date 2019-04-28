using System;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Backend.Loopback
{
    internal class Transport : IBackendTransport
    {
        public ISerializer Serializer { get; set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public void Publish(MessageWrapper message)
        {
            // This is a loopback, so no need to serialize the message.
            // Just fire the message received event at once.

            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
        }

        public void Dispose()
        {
        }
    }
}
