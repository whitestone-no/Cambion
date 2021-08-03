using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Whitestone.Cambion.Transport.AzureSericeBus;

namespace Whitestone.Cambion.IntegrationTests.Transports.AzureServiceBus
{
    public class AzureServiceBusFixture : IDisposable
    {
        public AzureServiceBusTransport Transport { get; }

        public AzureServiceBusFixture()
        {
            IConfigurationRoot configBuilder = new ConfigurationBuilder()
                .AddUserSecrets("8b33aa49-f340-4d93-bebf-cf21af9bad33")
                .AddEnvironmentVariables("AZURESERVICEBUSTEST_")
                .Build();

            AzureServiceBusTestConfig testConfig = configBuilder.GetSection("AzureServiceBus").Get<AzureServiceBusTestConfig>();

            AzureServiceBusConfig config = new AzureServiceBusConfig
            {
                Autentication =
                {
                    TenantId = testConfig.TenantId,
                    ClientId = testConfig.ClientId,
                    ClientSecret = testConfig.ClientSecret
                },
                Endpoint = testConfig.Endpoint,
                Topic =
                {
                    Name = testConfig.TopicName,
                    AutoCreate = true,
                    AutoDelete = true
                },
                Subscription =
                {
                    Name = testConfig.SubscriptionName,
                    AutoCreate = true,
                    AutoDelete = true
                }
            };

            Mock<ILogger<AzureServiceBusTransport>> logger = new Mock<ILogger<AzureServiceBusTransport>>();
            Mock<IOptions<AzureServiceBusConfig>> options = new Mock<IOptions<AzureServiceBusConfig>>();
            options.SetupGet(x => x.Value).Returns(config);

            Transport = new AzureServiceBusTransport(options.Object, logger.Object);
            Transport.StartAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            Transport.StopAsync().GetAwaiter().GetResult();
        }
    }
}
