using System;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Types;

namespace Whitestone.Cambion.Interfaces
{
    public interface ITransport
    {
        ISerializer Serializer { get; set; }

        void Start();
        void Stop();

        void Publish(MessageWrapper message);

        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
