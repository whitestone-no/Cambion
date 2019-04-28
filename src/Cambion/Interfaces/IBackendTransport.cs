using System;
using Whitestone.Cambion.Events;

namespace Whitestone.Cambion.Interfaces
{
    public interface IBackendTransport : IDisposable
    {
        ISerializer Serializer { get; set; }

        void Publish(MessageWrapper message);

        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
