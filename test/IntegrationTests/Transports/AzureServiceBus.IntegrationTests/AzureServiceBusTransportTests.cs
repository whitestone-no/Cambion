﻿using System;
using System.Threading;
using System.Threading.Tasks;
using RandomTestValues;
using Xunit;

namespace Whitestone.Cambion.IntegrationTests.Transports.AzureServiceBus
{
    public class AzureServiceBusTransportTests : IClassFixture<AzureServiceBusFixture>
    {
        private readonly AzureServiceBusFixture _fixture;

        public AzureServiceBusTransportTests(AzureServiceBusFixture fixture)
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
