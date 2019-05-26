using System;
using System.Threading;
using NUnit.Framework;
using Whitestone.Cambion.Serializer.JsonNet;
using Whitestone.Cambion.Transport.NetMQ;
using Whitestone.Cambion.Types;

namespace NetMQ.Test
{
    class NetMqTransportTests
    {
        private NetMqTransport _transportWithHost;

        [OneTimeSetUp]
        public void Setup()
        {
            _transportWithHost = new NetMqTransport("tcp://localhost:9990", "tcp://localhost:9991", true);
            _transportWithHost.Serializer = new JsonNetSerializer();
            _transportWithHost.Start();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _transportWithHost.Stop();
        }

        [Test]
        public void Publish_NullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { _transportWithHost.Publish(null); });
        }

        [Test]
        public void Publish_DefaultObject_EventReceived()
        {
            ManualResetEvent mre = new ManualResetEvent(false);

            _transportWithHost.MessageReceived += (sender, e) =>
            {
                mre.Set();
            };

            _transportWithHost.Publish(new MessageWrapper());

            bool eventFired = mre.WaitOne(new TimeSpan(0, 0, 5));

            Assert.True(eventFired);
        }
    }
}
