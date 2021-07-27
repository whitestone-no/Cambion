using System;
using System.Threading.Tasks;
using Whitestone.Cambion.Events;

namespace Whitestone.Cambion.Interfaces
{
    public interface ITransport
    {
        Task StartAsync();
        Task StopAsync();

        void Publish(byte[] messageBytes);

        event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}
