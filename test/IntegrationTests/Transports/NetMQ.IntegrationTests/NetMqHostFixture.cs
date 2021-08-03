using System;
using Microsoft.Extensions.Options;
using Moq;
using Whitestone.Cambion.Transport.NetMQ;

namespace Whitestone.Cambion.IntegrationTests.Transports.NetMQ
{
    public class NetMqHostFixture : IDisposable
    {
        public NetMqTransport Transport { get; }

        public NetMqHostFixture()
        {
            NetMqConfig config = new NetMqConfig
            {
                PublishAddress = "tcp://localhost:9990",
                SubscribeAddress = "tcp://localhost:9991",
                UseMessageHost = true
            };

            Mock<IOptions<NetMqConfig>> options = new Mock<IOptions<NetMqConfig>>();
            options.SetupGet(x => x.Value).Returns(config);

            Transport = new NetMqTransport(options.Object);

            Transport.StartAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            Transport.StopAsync().GetAwaiter().GetResult();
        }
    }
}
