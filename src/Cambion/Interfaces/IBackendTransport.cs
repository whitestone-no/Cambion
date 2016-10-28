using System;
using Whitestone.Cambion.Events;

namespace Whitestone.Cambion.Interfaces
{
    public interface IBackendTransport : IDisposable
    {
        void Publish(MessageWrapper data);

        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
