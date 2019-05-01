using System;
using Whitestone.Cambion.Common.Events;
using Whitestone.Cambion.Common.Types;

namespace Whitestone.Cambion.Common.Interfaces
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
