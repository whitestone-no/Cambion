using System;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Whitestone.Cambion.Serializer.JsonNet;
using Whitestone.Cambion.Transport.RabbitMQ;

namespace RabbitMQ.Test
{
    class RabbitMqTransportTests
    {
        private RabbitMqTransport _transport;

        [OneTimeSetUp]
        public void Setup()
        {
            IConfigurationRoot configBuilder = new ConfigurationBuilder()
                .AddUserSecrets("f7583692-a0ea-4e75-a825-9e61b8996832")
                .AddEnvironmentVariables()
                .Build();

            RabbitMqTestConfig config = configBuilder.GetSection("RabbitMQ").Get<RabbitMqTestConfig>();

            _transport = new RabbitMqTransport(config.ConnectionString)
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
        public void Publish_NullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { _transport.Publish(null); });
        }
	}
}
