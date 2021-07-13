using System;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Interfaces;
using Whitestone.Cambion.Types;

namespace Whitestone.Cambion.Transport.Loopback
{
    public class LoopbackTransport : ITransport
    {
        public ISerializer Serializer { get; set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public LoopbackTransport(ISerializer serializer)
        {
            Serializer = serializer;
        }

        public void Start() { }
        public void Stop() { }

        public void Publish(MessageWrapper message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            byte[] bytes = Serializer.Serialize(message);

            MessageWrapper receivedMessage = Serializer.Deserialize(bytes);

            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(receivedMessage));
        }
    }
}
