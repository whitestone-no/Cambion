using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Transport.NetMQ
{
    public class NetMqTransport : ITransport
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private readonly string _publishAddress;
        private readonly string _subscribeAddress;

        private readonly MessageHost _messageHost;
        private readonly object _publishSocketLocker = new object();
        private PublisherSocket _publishSocket;
        private SubscriberSocket _subscribeSocket;

        private Task _subscribeThread;
        private bool _subscribeThreadRunning;
        private Task _pingThread;
        private CancellationTokenSource _subscribeThreadCancellation;
        private CancellationTokenSource _pingThreadCancellation;

        public NetMqTransport(IOptions<NetMqConfig> config)
        {
            _publishAddress = config.Value.PublishAddress;
            _subscribeAddress = config.Value.SubscribeAddress;

            if (config.Value.UseMessageHost)
            {
                _messageHost = new MessageHost(_subscribeAddress, _publishAddress);
            }
        }

        public async Task StartAsync()
        {
            _messageHost?.Start();

            lock (_publishSocketLocker)
            {
                _publishSocket = new PublisherSocket();
                _publishSocket.Connect(_publishAddress);
            }

            _subscribeThreadCancellation = new CancellationTokenSource();
            _pingThreadCancellation = new CancellationTokenSource();

            _subscribeThread = Task.Factory.StartNew(SubscribeThread, TaskCreationOptions.LongRunning);
            _pingThread = Task.Factory.StartNew(PingThread, TaskCreationOptions.LongRunning);

            // Wait until the subscribe thread has actually subscribed before continuing.
            // For some reason a "reset event" doesn't work here.
            while (!_subscribeThreadRunning)
            {
                await Task.Delay(50);
            }
        }

        public Task StopAsync()
        {
            _subscribeThreadCancellation.Cancel();
            _pingThreadCancellation.Cancel();

            _subscribeThread.Wait();
            _pingThread.Wait();

            _subscribeThreadCancellation.Dispose();
            _pingThreadCancellation.Dispose();

            lock (_publishSocketLocker)
            {
                _publishSocket?.Disconnect(_publishAddress);
            }
            _subscribeSocket?.Disconnect(_subscribeAddress);

            lock (_publishSocketLocker)
            {
                _publishSocket?.Dispose();
            }
            _subscribeSocket?.Dispose();

            _messageHost?.Stop();

            return Task.CompletedTask;
        }

        public Task PublishAsync(byte[] messsageBytes)
        {
            if (messsageBytes == null)
            {
                throw new ArgumentNullException(nameof(messsageBytes));
            }

            lock (_publishSocketLocker)
            {
                _publishSocket.SendFrame(messsageBytes);
            }

            return Task.CompletedTask;
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
        private async Task PingThread()
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

                    _subscribeThread.Wait();

                    _subscribeThreadCancellation = new CancellationTokenSource();
                    _subscribeThread = Task.Factory.StartNew(SubscribeThread, TaskCreationOptions.LongRunning);

                    while (!_subscribeThreadRunning)
                    {
                        await Task.Delay(50);
                    }

                    needsReinitialization = false;
                }

                await Task.Delay(5000);
            }
        }

        private Task SubscribeThread()
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
                    if (!_subscribeSocket.TryReceiveFrameBytes(new TimeSpan(0, 0, 0, 0, 200), out byte[] messageBytes))
                    {
                        continue;
                    }

                    MessageReceived?.Invoke(this, new MessageReceivedEventArgs(messageBytes));
                }
                catch (ObjectDisposedException) { }
            }

            _subscribeThreadRunning = false;

            return Task.CompletedTask;
        }
    }
}
