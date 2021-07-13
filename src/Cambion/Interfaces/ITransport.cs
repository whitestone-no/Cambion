using System;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Types;

namespace Whitestone.Cambion.Interfaces
{
    public interface ITransport
    {
        void Start();
        void Stop();

        void Publish(byte[] messageBytes);

        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
