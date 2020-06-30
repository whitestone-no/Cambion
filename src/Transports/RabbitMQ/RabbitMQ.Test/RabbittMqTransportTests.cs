using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Whitestone.Cambion.Serializer.JsonNet;
using Whitestone.Cambion.Transport.RabbitMQ;
using Whitestone.Cambion.Types;

namespace RabbitMQ.Test
{
    class RabbitMqTransportTests
    {
        private RabbitMqTransport _transport;
        private RabbitMqTestConfig _config;

        [OneTimeSetUp]
        public void Setup()
        {
            IConfigurationRoot configBuilder = new ConfigurationBuilder()
                .AddUserSecrets("f7583692-a0ea-4e75-a825-9e61b8996832")
                .AddEnvironmentVariables("RABBITMQTEST")
                .Build();

            _config = configBuilder.GetSection("RabbitMQ").Get<RabbitMqTestConfig>();

            _transport = new RabbitMqTransport(_config.ConnectionString)
            {
                Serializer = new JsonNetSerializer()
            };
            _transport.Start();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _transport.Stop();
        }

        [Test]
        [Order(1)]
        public void Publish_DefaultObject_EventReceived()
        {
            ManualResetEvent mre = new ManualResetEvent(false);

            _transport.MessageReceived += (sender, e) =>
            {
                mre.Set();
            };

            _transport.Publish(new MessageWrapper());

            bool eventFired = mre.WaitOne(new TimeSpan(0, 0, 5));

            Assert.True(eventFired);
        }

        [Test]
        [Order(2)]
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

            _transport.MessageReceived += (sender, e) =>
            {
                mwIn = e.Message;
                mre.Set();
            };

            _transport.Publish(mwOut);

            mre.WaitOne(new TimeSpan(0, 0, 5));

            Assert.AreEqual(mwOut, mwIn);
        }

        [Test]
        [Order(3)]
        public void Publish_HostWithCustomConfig_EventReceived()
        {
            RabbitMqConfig config = new RabbitMqConfig
            {
                Connection =
                {
                    Hostname = _config.Hostname,
                    Username = _config.Username,
                    Password = _config.Password,
                    VirtualHost = _config.VirtualHost
                }
            };
            RabbitMqTransport transport = new RabbitMqTransport(config)
            {
                Serializer = new JsonNetSerializer()
            };
            transport.Start();

            ManualResetEvent mre = new ManualResetEvent(false);

            _transport.MessageReceived += (sender, e) =>
            {
                mre.Set();
            };

            _transport.Publish(new MessageWrapper());

            bool eventFired = mre.WaitOne(new TimeSpan(0, 0, 5));

            Assert.True(eventFired);

            _transport.Stop();
        }

        [Test]
        [Order(4)]
        public void Publish_NullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { _transport.Publish(null); });
        }
    }
}
