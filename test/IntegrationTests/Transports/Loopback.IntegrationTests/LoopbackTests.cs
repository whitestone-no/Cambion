using System;
using System.Threading;
using System.Threading.Tasks;
using RandomTestValues;
using Whitestone.Cambion.Transport.Loopback;
using Xunit;

namespace Whitestone.Cambion.IntegrationTests.Transports.Loopback
{
    public class LoopbackTransportTests
    {
        private readonly LoopbackTransport _transport;

        public LoopbackTransportTests()
        {
            _transport = new LoopbackTransport();
        }

        [Fact]
        public async Task PublishAsync_NullValue_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await _transport.PublishAsync(null); });
        }

        [Fact]
        public async Task PublishAsync_SameDataReceived_Success()
        {
            // Arrange

            ManualResetEvent mre = new ManualResetEvent(false);
            byte[] expectedBytes = RandomValue.Array<byte>();

            byte[] actualBytes = null;
            _transport.MessageReceived += (sender, e) =>
            {
                actualBytes = e.MessageBytes;
                mre.Set();
            };

            // Act

            await _transport.PublishAsync(expectedBytes);

            // Assert

            bool eventFired = mre.WaitOne(TimeSpan.FromSeconds(5));

            Assert.True(eventFired);
            Assert.Equal(expectedBytes, actualBytes);
        }
    }
}
