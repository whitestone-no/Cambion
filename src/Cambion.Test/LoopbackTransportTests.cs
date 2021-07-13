using NUnit.Framework;
using System;
using System.Threading;
using Moq;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Interfaces;
using Whitestone.Cambion.Types;
using Whitestone.Cambion.Transport.Loopback;

namespace Whitestone.Cambion.Test
{
    [Order(2)]
    class LoopbackTransportTests
    {
        private LoopbackTransport _transport;

        private Mock<ISerializer> _serializer;

        [SetUp]
        public void Setup()
        {
            _serializer = new Mock<ISerializer>();

            _transport = new LoopbackTransport(_serializer.Object);
        }

        [Test]
        public void Publish_NullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { _transport.Publish(null); });
        }

        [Test]
        public void Publish_DefaultObject_EventReceived()
        {
            ManualResetEvent mre = new ManualResetEvent(false);

            _transport.MessageReceived += (sender, e) =>
            {
                mre.Set();
            };

            _transport.Publish(new MessageWrapper());

            bool eventFired = mre.WaitOne(new TimeSpan(0, 0, 1));

            Assert.True(eventFired);
        }

        [Test]
        public void Publish_DefaultObject_EventReceivedSameData()
        {
            MessageWrapper expected = new MessageWrapper();
            MessageWrapper actual = null;
            ManualResetEvent mre = new ManualResetEvent(false);
            EventHandler<MessageReceivedEventArgs> handler = (sender, e) =>
            {
                actual = e.Message;
                mre.Set();
            };

            _transport.MessageReceived += handler;

            _transport.Publish(new MessageWrapper());

            Assert.True(mre.WaitOne(new TimeSpan(0, 0, 1)));

            Assert.AreEqual(expected, actual);

            _transport.MessageReceived -= handler;
        }

    }
}