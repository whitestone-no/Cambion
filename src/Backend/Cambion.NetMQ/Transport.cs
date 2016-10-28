using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
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

                    MessageWrapper message = JsonConvert.DeserializeObject<MessageWrapper>(Encoding.Unicode.GetString(messageBytes));

                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
                }
                // ReSharper disable once FunctionNeverReturns because this is designed to run forever
            });
        }

        public void Publish(MessageWrapper data)
        {
            // Use publish endpoint to publish requested data
            string serialized = JsonConvert.SerializeObject(data);
            _publishSocket.SendFrame(Encoding.Unicode.GetBytes(serialized));
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
