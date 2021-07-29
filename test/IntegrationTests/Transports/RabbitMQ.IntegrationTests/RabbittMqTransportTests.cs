using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Whitestone.Cambion.Interfaces;
using Whitestone.Cambion.Transport.RabbitMQ;
using Whitestone.Cambion.Types;

namespace RabbitMQ.Test
{
    class RabbitMqTransportTests
    {
        private RabbitMqTransport _transport;
        
        private Mock<IOptions<RabbitMqConfig>> _options;
        private RabbitMqConfig _config;

        [OneTimeSetUp]
        public void Setup()
        {
            IConfigurationRoot configBuilder = new ConfigurationBuilder()
                .AddUserSecrets("f7583692-a0ea-4e75-a825-9e61b8996832")
                .AddEnvironmentVariables("RABBITMQTEST_")
                .Build();

            RabbitMqTestConfig testConfig = configBuilder.GetSection("RabbitMQ").Get<RabbitMqTestConfig>();
            _config = new RabbitMqConfig
            {
                Connection =
                {
                    Hostname = testConfig.Hostname,
                    Username = testConfig.Username,
                    Password = testConfig.Password,
                    VirtualHost = testConfig.VirtualHost
                }
            };
            
            _options = new Mock<IOptions<RabbitMqConfig>>();

            _options.SetupGet(x => x.Value).Returns(_config);

            _transport = new RabbitMqTransport(_options.Object);
            _transport.StartAsync();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _transport.StopAsync();
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

            _transport.PublishAsync(new byte[0]);

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
                mre.Set();
            };

            _transport.PublishAsync(new byte[0]);

            mre.WaitOne(new TimeSpan(0, 0, 5));

            Assert.AreEqual(mwOut, mwIn);
        }

        [Test]
        [Order(3)]
        public void Publish_HostWithCustomConfig_EventReceived()
        {
            _transport.StartAsync();

            ManualResetEvent mre = new ManualResetEvent(false);

            _transport.MessageReceived += (sender, e) =>
            {
                mre.Set();
            };

            _transport.PublishAsync(new byte[0]);

            bool eventFired = mre.WaitOne(new TimeSpan(0, 0, 5));

            Assert.True(eventFired);

            _transport.StopAsync();
        }

        [Test]
        [Order(4)]
        public void Publish_NullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { _transport.PublishAsync(null); });
        }
    }
}
