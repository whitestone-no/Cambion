using System;
using System.Threading;
using System.Threading.Tasks;
using RandomTestValues;
using Xunit;

namespace Whitestone.Cambion.IntegrationTests.Transports.RabbitMQ
{
    // Use a collection to not run these tests in parallel with RabbittMqTransportCustomConfigTests
    // as these will interfere with eachother
    [Collection("RabbitMQ Integration Tests")]
    public class RabbittMqTransportConnectionStringConfigTests : IClassFixture<RabbitMqConnectionStringConfigFixture>
    {
        private readonly RabbitMqConnectionStringConfigFixture _fixture;

        public RabbittMqTransportConnectionStringConfigTests(RabbitMqConnectionStringConfigFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task PublishAsync_NullValue_ThrowsArgumentNullException()
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
    }
}
