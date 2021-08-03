using System;
using System.Threading.Tasks;
using Whitestone.Cambion.Serializer.MessagePack;
using Whitestone.Cambion.Types;
using Xunit;

namespace Whitestone.Cambion.IntegrationTests.Serializers.MessagePack
{
    public class MessagePackSerializerTests
    {
        private readonly MessagePackSerializer _serializer;

        public MessagePackSerializerTests()
        {
            _serializer = new MessagePackSerializer();
        }

        [Fact]
        public async Task SerializeAsync_NullMessage_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await _serializer.SerializeAsync(null); });
        }

        [Fact]
        public async Task SerializeAsync_DefaultObject_Success()
        {
            const string expectedBase64 = "yAFAZNlyV2hpdGVzdG9uZS5DYW1iaW9uLlR5cGVzLk1lc3NhZ2VXcmFwcGVyLCBXaGl0ZXN0b25lLkNhbWJpb24sIFZlcnNpb249MS4xLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1udWxshaREYXRh0gAAAAGoRGF0YVR5cGXZZ1N5c3RlbS5JbnQzMiwgU3lzdGVtLlByaXZhdGUuQ29yZUxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPTdjZWM4NWQ3YmVhNzc5OGWsUmVzcG9uc2VUeXBlwKtNZXNzYWdlVHlwZQCtQ29ycmVsYXRpb25JZNkkMDAwMDAwMDAtMDAwMC0wMDAwLTAwMDAtMDAwMDAwMDAwMDAw";

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

            byte[] data = Convert.FromBase64String("yAFAZNlyV2hpdGVzdG9uZS5DYW1iaW9uLlR5cGVzLk1lc3NhZ2VXcmFwcGVyLCBXaGl0ZXN0b25lLkNhbWJpb24sIFZlcnNpb249MS4xLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1udWxshaREYXRh0gAAAAGoRGF0YVR5cGXZZ1N5c3RlbS5JbnQzMiwgU3lzdGVtLlByaXZhdGUuQ29yZUxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPTdjZWM4NWQ3YmVhNzc5OGWsUmVzcG9uc2VUeXBlwKtNZXNzYWdlVHlwZQCtQ29ycmVsYXRpb25JZNkkMDAwMDAwMDAtMDAwMC0wMDAwLTAwMDAtMDAwMDAwMDAwMDAw");

            MessageWrapper actual = await _serializer.DeserializeAsync(data);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task DeserializeAsync_InvalidObject_ReturnsNull()
        {
            byte[] data = Convert.FromBase64String("kxcSA03r");

            MessageWrapper actual = await _serializer.DeserializeAsync(data);

            Assert.Null(actual);
        }
    }
}
