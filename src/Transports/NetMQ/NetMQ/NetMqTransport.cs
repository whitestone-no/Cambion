using System;
using System.Net.NetworkInformation;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Interfaces;
using Whitestone.Cambion.Types;

namespace Whitestone.Cambion.Transport.NetMQ
{
    public class NetMqTransport : ITransport
    {
        public ISerializer Serializer { get; set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private readonly string _publishAddress;
        private readonly string _subscribeAddress;

        private readonly PublisherSocket _publishSocket;
        private SubscriberSocket _subscribeSocket;
        private readonly MessageHost _messageHost;

        private Thread _subscribeThread;
        private bool _subscribeThreadRunning;
        private Thread _pingThread;
        private CancellationTokenSource _subscribeThreadCancellation = new CancellationTokenSource();
        private CancellationTokenSource _pingThreadCancellation = new CancellationTokenSource();


        public NetMqTransport(string publishAddress, string subscribeAddress, bool useMessageHost)
        {
            _publishAddress = publishAddress;
            _subscribeAddress = subscribeAddress;

            if (useMessageHost)
            {
                _messageHost = new MessageHost(subscribeAddress, publishAddress);
            }

            _publishSocket = new PublisherSocket();
        }

        public void Start()
        {
            _messageHost?.Start();

            lock (_publishSocket)
            {
                _publishSocket.Connect(_publishAddress);
            }

            _subscribeThread = new Thread(SubscribeThread) { IsBackground = true };
            _subscribeThread.Start();

            _pingThread = new Thread(PingThread) { IsBackground = true };
            _pingThread.Start();

            // Wait until the subscribe thread has actually subscribed before continuing.
            // For some reason a "reset event" doesn't work here.
            while (!_subscribeThreadRunning)
            {
                Thread.Sleep(50);
            }
        }

        public void Stop()
        {
            _subscribeThreadCancellation.Cancel();
            _pingThreadCancellation.Cancel();

            lock (_publishSocket)
            {
                _publishSocket?.Disconnect(_publishAddress);
            }
            _subscribeSocket?.Disconnect(_subscribeAddress);

            lock (_publishSocket)
            {
                _publishSocket?.Dispose();
            }
            _subscribeSocket?.Dispose();

            _messageHost?.Stop();
        }

        public void Publish(MessageWrapper message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            byte[] rawBytes = Serializer.Serialize(message);

            lock (_publishSocket)
            {
                _publishSocket.SendFrame(rawBytes);
            }
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
            Uri uri = new Uri(_subscribeAddress);
            bool needsReinitialization = false;

            while (!_pingThreadCancellation.IsCancellationRequested)
            {
                bool pingable;

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
                    _subscribeThreadCancellation.Cancel();

                    _subscribeSocket.Disconnect(_subscribeAddress);
                    _subscribeSocket.Dispose();

                    _subscribeThread.Join();

                    _subscribeThreadCancellation = new CancellationTokenSource();
                    _subscribeThread = new Thread(SubscribeThread) { IsBackground = true };
                    _subscribeThread.Start();

                    while (!_subscribeThreadRunning)
                    {
                        Thread.Sleep(50);
                    }

                    needsReinitialization = false;
                }

                Thread.Sleep(5000);
            }
        }

        private void SubscribeThread()
        {
            // Place subscribe socket initialization inside thread to make all subscribe calls happen on the same thread
            // Trying to prevent "Cannot close an uninitialised Msg" exceptions
            _subscribeSocket = new SubscriberSocket();
            _subscribeSocket.Connect(_subscribeAddress);
            _subscribeSocket.SubscribeToAnyTopic();

            _subscribeThreadRunning = true;

            while (!_subscribeThreadCancellation.IsCancellationRequested)
            {
                try
                {
                    byte[] messageBytes = _subscribeSocket.ReceiveFrameBytes();

                    MessageWrapper wrapper = Serializer.Deserialize(messageBytes);

                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(wrapper));
                }
                catch (ObjectDisposedException) { }
            }

            _subscribeThreadRunning = false;
        }
    }
}
