using NUnit.Framework;
using System;
using Whitestone.Cambion.Transports.NetMQ;

namespace Whitestone.Cambion.Test
{
    [Order(3)]
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
