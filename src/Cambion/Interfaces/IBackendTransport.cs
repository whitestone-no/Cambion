using System;
using Whitestone.Cambion.Events;

namespace Whitestone.Cambion.Interfaces
{
    public interface IBackendTransport : IDisposable
    {
        void Publish(byte[] data);

        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
