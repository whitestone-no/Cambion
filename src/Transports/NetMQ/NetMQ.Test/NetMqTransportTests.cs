using System;
using NUnit.Framework;
using Whitestone.Cambion.Transport.NetMQ;

namespace NetMQ.Test
{
    class NetMqTransportTests
    {
        private NetMqTransport _transport;

        [SetUp]
        public void Setup()
        {
            _transport = new NetMqTransport("tcp://localhost:9990", "tcp://localhost:9991", true);
        }

        [Test]
        public void Publish_NullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { _transport.Publish(null); });
        }
    }
}
