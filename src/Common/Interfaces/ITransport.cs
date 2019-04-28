using System;
using Whitestone.Cambion.Common.Events;
using Whitestone.Cambion.Common.Types;

namespace Whitestone.Cambion.Common.Interfaces
{
    public interface ITransport : IDisposable
    {
        ISerializer Serializer { get; set; }

        void Publish(MessageWrapper message);

        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
