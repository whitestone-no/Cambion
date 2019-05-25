using NUnit.Framework;
using System;
using System.Threading;
using Whitestone.Cambion.Common.Events;
using Whitestone.Cambion.Common.Types;
using Whitestone.Cambion.Transport.Loopback;

namespace Whitestone.Cambion.Test
{
    [Order(2)]
    class LoopbackTransportTests
    {
        private LoopbackTransport _transport;

        [SetUp]
        public void Setup()
        {
            _transport = new LoopbackTransport();
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