using NetMQ;
using NetMQ.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Whitestone.Cambion.Transports.NetMQ
{
    internal class MessageHost
    {
        private XPublisherSocket _toSocket;
        private XSubscriberSocket _fromSocket;
        private Proxy _proxy;

        private readonly string _toAddress;
        private readonly string _fromAddress;

        public MessageHost(string toAddress, string fromAddress)
        {
            _toAddress = toAddress;
            _fromAddress = fromAddress;
        }

        public void Start()
        {
            ManualResetEvent mre = new ManualResetEvent(false);

            // NetMQ.Bind to Publish and Subscribe addresses
            Task.Factory.StartNew(() =>
            {
                using (_toSocket = new XPublisherSocket())
                using (_fromSocket = new XSubscriberSocket())
                {
                    _toSocket.Bind(_toAddress);
                    _fromSocket.Bind(_fromAddress);

                    _proxy = new Proxy(_fromSocket, _toSocket);

                    mre.Set();

                    _proxy.Start();
                }
            });

            // Wait until the message host is actually started before returning
            mre.WaitOne(-1);
        }

        public void Stop()
        {
            _proxy.Stop();
            _fromSocket.Unbind(_fromAddress);
            _fromSocket.Close();
            _fromSocket.Dispose();
            _toSocket.Unbind(_toAddress);
            _toSocket.Close();
            _toSocket.Dispose();
        }
    }
}
