using System;
using System.Threading.Tasks;
using Whitestone.Cambion.Serializer.JsonNet;
using Whitestone.Cambion.Types;
using Xunit;

namespace Whitestone.Cambion.IntegrationTests.Serializers.JsonNet
{
    public class LoopbackTransportTests
    {
        private readonly JsonNetSerializer _serializer;

        public LoopbackTransportTests()
        {
            _serializer = new JsonNetSerializer();
        }

        [Fact]
        public async Task SerializeAsync_NullMessage_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await _serializer.SerializeAsync(null); });
        }

        [Fact]
        public async Task SerializeAsync_DefaultObject_Success()
        {
            const string expectedBase64 = "eyIkdHlwZSI6IldoaXRlc3RvbmUuQ2FtYmlvbi5UeXBlcy5NZXNzYWdlV3JhcHBlciwgV2hpdGVzdG9uZS5DYW1iaW9uIiwiRGF0YSI6MSwiRGF0YVR5cGUiOiJTeXN0ZW0uSW50MzIsIFN5c3RlbS5Qcml2YXRlLkNvcmVMaWIsIFZlcnNpb249NC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj03Y2VjODVkN2JlYTc3OThlIiwiUmVzcG9uc2VUeXBlIjpudWxsLCJNZXNzYWdlVHlwZSI6MCwiQ29ycmVsYXRpb25JZCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCJ9";

            MessageWrapper messageWrapper = new MessageWrapper
            {
                CorrelationId = Guid.Empty,
                Data = 1,
                DataType = typeof(int),
                MessageType = MessageType.Event,
                ResponseType = null
            };

            byte[] result = await _serializer.SerializeAsync(messageWrapper);

            string resultBase64 = Convert.ToBase64String(result);

            Assert.Equal(expectedBase64, resultBase64);
        }

        [Fact]
        public async Task DeserializeAsync_DefaultObject_Success()
        {
            MessageWrapper expected = new MessageWrapper
            {
                CorrelationId = Guid.Empty,
                Data = 1,
                DataType = typeof(int),
                MessageType = MessageType.Event,
                ResponseType = null
            };

            byte[] data = Convert.FromBase64String("eyIkdHlwZSI6IldoaXRlc3RvbmUuQ2FtYmlvbi5UeXBlcy5NZXNzYWdlV3JhcHBlciwgV2hpdGVzdG9uZS5DYW1iaW9uIiwiRGF0YSI6MSwiRGF0YVR5cGUiOiJTeXN0ZW0uSW50MzIsIFN5c3RlbS5Qcml2YXRlLkNvcmVMaWIsIFZlcnNpb249NC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj03Y2VjODVkN2JlYTc3OThlIiwiUmVzcG9uc2VUeXBlIjpudWxsLCJNZXNzYWdlVHlwZSI6MCwiQ29ycmVsYXRpb25JZCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCJ9");

            MessageWrapper actual = await _serializer.DeserializeAsync(data);

            Assert.Equal(expected, actual);
        }
    }
}
