using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using RandomTestValues;
using Whitestone.Cambion.Transport.NetMQ;
using Xunit;

namespace Whitestone.Cambion.IntegrationTests.Transports.NetMQ
{
    public class NetMqTransportTests : IClassFixture<NetMqHostFixture>
    {
        private readonly NetMqHostFixture _fixture;

        public NetMqTransportTests(NetMqHostFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Publish_NullValue_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await _fixture.Transport.PublishAsync(null); });
        }

        [Fact]
        public async Task PublishAsync_SameDataReceived_Success()
        {
            // Arrange

            ManualResetEvent mre = new ManualResetEvent(false);
            byte[] expectedBytes = RandomValue.Array<byte>();

            byte[] actualBytes = null;
            _fixture.Transport.MessageReceived += (sender, e) =>
            {
                actualBytes = e.MessageBytes;
                mre.Set();
            };

            // Act

            await _fixture.Transport.PublishAsync(expectedBytes);

            // Assert

            bool eventFired = mre.WaitOne(TimeSpan.FromSeconds(5));

            Assert.True(eventFired);
            Assert.Equal(expectedBytes, actualBytes);
        }

        [Fact]
        public async Task PublishOnHost_ReceiveOnNonHost_Success()
        {
            // Arrange

            NetMqConfig config = new NetMqConfig
            {
                PublishAddress = "tcp://localhost:9990",
                SubscribeAddress = "tcp://localhost:9991",
                UseMessageHost = false
            };

            Mock<IOptions<NetMqConfig>> options = new Mock<IOptions<NetMqConfig>>();
            options.SetupGet(x => x.Value).Returns(config);

            NetMqTransport transport = new NetMqTransport(options.Object);

            await transport.StartAsync();

            byte[] expectedBytes = RandomValue.Array<byte>();

            ManualResetEvent mre = new ManualResetEvent(false);
            byte[] actualBytes = null;
            _fixture.Transport.MessageReceived += (sender, e) =>
            {
                actualBytes = e.MessageBytes;
                mre.Set();
            };

            // Act

            await _fixture.Transport.PublishAsync(expectedBytes);

            // Assert

            bool eventFired = mre.WaitOne(new TimeSpan(0, 0, 5));

            Assert.True(eventFired);
            Assert.Equal(expectedBytes, actualBytes);

            await transport.StopAsync();
        }
    }
}
