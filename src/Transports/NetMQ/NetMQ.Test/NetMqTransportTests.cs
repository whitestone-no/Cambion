using System;
using System.Threading;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Whitestone.Cambion.Interfaces;
using Whitestone.Cambion.Transport.NetMQ;
using Whitestone.Cambion.Types;

namespace NetMQ.Test
{
    class NetMqTransportTests
    {
        private NetMqTransport _transportWithHost;
        
        private Mock<IOptions<NetMqConfig>> _options;
        private NetMqConfig _config;

        [OneTimeSetUp]
        public void Setup()
        {
            _options = new Mock<IOptions<NetMqConfig>>();
            _config = new NetMqConfig
            {
                PublishAddress = "tcp://localhost:9990",
                SubscribeAddress = "tcp://localhost:9991",
                UseMessageHost = true
            };

            _options.SetupGet(x => x.Value).Returns(_config);

            _transportWithHost = new NetMqTransport(_options.Object);
            _transportWithHost.StartAsync();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _transportWithHost.StopAsync();
        }

        [Test]
        public void Publish_NullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { _transportWithHost.PublishAsync(null); });
        }

        [Test]
        public void Publish_DefaultObject_EventReceived()
        {
            ManualResetEvent mre = new ManualResetEvent(false);

            _transportWithHost.MessageReceived += (sender, e) =>
            {
                mre.Set();
            };

            _transportWithHost.PublishAsync(new byte[0]);

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
                mre.Set();
            };

            _transportWithHost.PublishAsync(new byte[0]);

            mre.WaitOne(new TimeSpan(0, 0, 5));

            Assert.AreEqual(mwOut, mwIn);
        }

        [Test]
        public void PublishOnHost_ReceiveOnNonHost()
        {
            ManualResetEvent mre = new ManualResetEvent(false);

            _transportWithHost.MessageReceived += (sender, e) => { mre.Set(); };

            _transportWithHost.PublishAsync(new byte[0]);

            bool eventFired = mre.WaitOne(new TimeSpan(0, 0, 5));

            _transportWithHost.StopAsync();

            Assert.True(eventFired);
        }
    }
}
