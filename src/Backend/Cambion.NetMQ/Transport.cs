using System;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Backend.NetMQ
{
    public class Transport : IBackendTransport, INetMqConfigurator
    {
        public string PublishAddress { get; set; }
        public string SubscribeAddress { get; set; }

        private readonly PublisherSocket _publishSocket;
        private readonly SubscriberSocket _subscribeSocket;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public Transport(string publishAddress, string subscribeAddress)
        {
            PublishAddress = publishAddress;
            SubscribeAddress = subscribeAddress;

            _publishSocket = new PublisherSocket();
            _subscribeSocket = new SubscriberSocket();

            _publishSocket.Connect(PublishAddress);
            _subscribeSocket.Connect(SubscribeAddress);

            _subscribeSocket.SubscribeToAnyTopic();

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    byte[] messageBytes = _subscribeSocket.ReceiveFrameBytes();

                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(messageBytes));
                }
                // ReSharper disable once FunctionNeverReturns because this is designed to run forever
            });
        }

        public void Publish(byte[] data)
        {
            _publishSocket.SendFrame(data);
        }

        public void Dispose()
        {
            _publishSocket?.Disconnect(PublishAddress);
            _subscribeSocket?.Disconnect(SubscribeAddress);

            _publishSocket?.Dispose();
            _subscribeSocket?.Dispose();
        }
    }
}
