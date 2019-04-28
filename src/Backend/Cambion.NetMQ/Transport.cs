using System;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Interfaces;
using System.Threading;
using System.Net.NetworkInformation;
using System.Text;
using Newtonsoft.Json;

namespace Whitestone.Cambion.Backend.NetMQ
{
    internal class Transport : IBackendTransport, INetMqConfigurator
    {
        public ISerializer Serializer { get; set; }

        public string PublishAddress { get; set; }
        public string SubscribeAddress { get; set; }

        private readonly PublisherSocket _publishSocket;
        private SubscriberSocket _subscribeSocket;

        private Thread _subscribeThread;
        private Thread _pingThread;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public Transport(string publishAddress, string subscribeAddress)
        {
            PublishAddress = publishAddress;
            SubscribeAddress = subscribeAddress;

            _publishSocket = new PublisherSocket();
        }

        public void Start()
        {
            lock (_publishSocket)
            {
                _publishSocket.Connect(PublishAddress);
            }

            _subscribeThread = new Thread(SubscribeThread) { IsBackground = true };
            _subscribeThread.Start();

            _pingThread = new Thread(PingThread) { IsBackground = true };
            _pingThread.Start();
        }

        /// <summary>
        /// This thread is needed to handle network disconnects.
        /// </summary>
        /// <remarks>
        /// If the PC the message host is running on disconnects from the
        /// network, the subscribe thread is never able to reconnect to it by
        /// itself. Therefore we periodically network ping the subscriber
        /// address to check if it responds. If it at one point does not
        /// respond we will need to reconnect to it again when it starts
        /// responding.
        /// </remarks>
        private void PingThread()
        {
            try
            {
                Uri uri = new Uri(SubscribeAddress);
                bool needsReinitialization = false;

                while (true)
                {
                    bool pingable = false;

                    Ping pinger = new Ping();
                    try
                    {
                        PingReply reply = pinger.Send(uri.Host, 1000);
                        pingable = reply != null && (reply.Status == IPStatus.Success);
                    }
                    catch (PingException)
                    {
                        pingable = false;
                    }

                    if (!pingable)
                    {
                        needsReinitialization = true;
                    }

                    if (pingable && needsReinitialization)
                    {
                        _subscribeThread.Abort();

                        _subscribeSocket.Disconnect(SubscribeAddress);
                        _subscribeSocket.Dispose();

                        _subscribeThread.Join();

                        _subscribeThread = new Thread(SubscribeThread) { IsBackground = true };
                        _subscribeThread.Start();

                        needsReinitialization = false;
                    }

                    Thread.Sleep(5000);
                }
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
        }

        private void SubscribeThread()
        {
            try
            {
                // Place subscribe socket initialization inside thread to make all subscribe calls happen on the same thread
                // Trying to prevent "Cannot close an uninitialised Msg" exceptions
                _subscribeSocket = new SubscriberSocket();
                _subscribeSocket.Connect(SubscribeAddress);
                _subscribeSocket.SubscribeToAnyTopic();

                while (true)
                {
                    byte[] messageBytes = _subscribeSocket.ReceiveFrameBytes();

                    MessageWrapper wrapper = Serializer.Deserialize(messageBytes);

                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(wrapper));
                }
                // ReSharper disable once FunctionNeverReturns because this is designed to run forever
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
        }

        public void Publish(MessageWrapper message)
        {
            byte[] rawBytes = Serializer.Serialize(message);

            lock (_publishSocket)
            {
                _publishSocket.SendFrame(rawBytes);
            }
        }

        public void Dispose()
        {
            _subscribeThread.Abort();
            _pingThread.Abort();

            _publishSocket?.Disconnect(PublishAddress);
            _subscribeSocket?.Disconnect(SubscribeAddress);

            _publishSocket?.Dispose();
            _subscribeSocket?.Dispose();
        }
    }
}
