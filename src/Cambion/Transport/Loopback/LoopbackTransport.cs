using System;
using System.Threading.Tasks;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Transport.Loopback
{
    public class LoopbackTransport : ITransport
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public Task StartAsync() => Task.CompletedTask;

        public Task StopAsync() => Task.CompletedTask;

        public void Publish(byte[] messageBytes)
        {
            if (messageBytes == null)
            {
                throw new ArgumentNullException(nameof(messageBytes));
            }

            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(messageBytes));
        }
    }
}
