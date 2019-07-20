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

        [Test]
        public void Publish_TestObject_SameDataReceived()
        {
            ManualResetEvent mre = new ManualResetEvent(false);
            MessageWrapper mwOut = new MessageWrapper
            {
                CorrelationId = Guid.NewGuid(),
                Data = 47,
                DataType = typeof(int),
                MessageType = MessageType.Event,
                ResponseType = typeof(DateTime)
            };
            MessageWrapper mwIn = null;

            _transportWithHost.MessageReceived += (sender, e) =>
            {
                mwIn = e.Message;
                mre.Set();
            };

            _transportWithHost.Publish(mwOut);

            mre.WaitOne(new TimeSpan(0, 0, 5));

            Assert.AreEqual(mwOut, mwIn);
        }

        [Test]
        public void PublishOnHost_ReceiveOnNonHost()
        {
            NetMqTransport transportWithoutHost = new NetMqTransport("tcp://localhost:9990", "tcp://localhost:9991", false);
            transportWithoutHost.Serializer = new JsonNetSerializer();
            transportWithoutHost.Start();

            ManualResetEvent mre = new ManualResetEvent(false);

            transportWithoutHost.MessageReceived += (sender, e) => { mre.Set(); };

            _transportWithHost.Publish(new MessageWrapper());

            bool eventFired = mre.WaitOne(new TimeSpan(0, 0, 5));

            transportWithoutHost.Stop();

            Assert.True(eventFired);
        }
    }
}
