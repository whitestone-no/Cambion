using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Whitestone.Cambion.Transport.RabbitMQ;

namespace Whitestone.Cambion.IntegrationTests.Transports.RabbitMQ
{
    public class RabbitMqConnectionStringConfigFixture : IDisposable
    {
        public RabbitMqTransport Transport { get; }

        public RabbitMqConnectionStringConfigFixture()
        {
            IConfigurationRoot configBuilder = new ConfigurationBuilder()
                .AddUserSecrets("f7583692-a0ea-4e75-a825-9e61b8996832")
                .AddEnvironmentVariables("RABBITMQTEST_")
                .Build();

            RabbitMqTestConfig testConfig = configBuilder.GetSection("RabbitMQ").Get<RabbitMqTestConfig>();

            RabbitMqConfig config = new RabbitMqConfig
            {
                Connection =
                {
                    ConnectionString = new Uri(testConfig.ConnectionString)
                }
            };

            Mock<IOptions<RabbitMqConfig>> options = new Mock<IOptions<RabbitMqConfig>>();
            options.SetupGet(x => x.Value).Returns(config);

            Transport = new RabbitMqTransport(options.Object);
            Transport.StartAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            Transport.StopAsync().GetAwaiter().GetResult();
        }
    }
}
