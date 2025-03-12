using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whitestone.Cambion.Serializer.MessagePack;
using Whitestone.Cambion.Types;
using Xunit;

namespace Whitestone.Cambion.IntegrationTests.Serializers.MessagePack
{
    public class MessagePackSerializerTests
    {
        private readonly MessagePackSerializer _serializer = new();

        [Fact]
        public async Task SerializeAsync_NullMessage_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await _serializer.SerializeAsync(null); });
        }

        [Fact]
        public async Task SerializeAsync_DefaultObject_Success()
        {
            const string expectedBase64 = "yAFAZNlyV2hpdGVzdG9uZS5DYW1iaW9uLlR5cGVzLk1lc3NhZ2VXcmFwcGVyLCBXaGl0ZXN0b25lLkNhbWJpb24sIFZlcnNpb249MS4xLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1udWxshaREYXRh0gAAAAGoRGF0YVR5cGXZZ1N5c3RlbS5JbnQzMiwgU3lzdGVtLlByaXZhdGUuQ29yZUxpYiwgVmVyc2lvbj04LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPTdjZWM4NWQ3YmVhNzc5OGWsUmVzcG9uc2VUeXBlwKtNZXNzYWdlVHlwZQCtQ29ycmVsYXRpb25JZNkkMDAwMDAwMDAtMDAwMC0wMDAwLTAwMDAtMDAwMDAwMDAwMDAw";

            var messageWrapper = new MessageWrapper
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

            byte[] data = Convert.FromBase64String("yAFAZNlyV2hpdGVzdG9uZS5DYW1iaW9uLlR5cGVzLk1lc3NhZ2VXcmFwcGVyLCBXaGl0ZXN0b25lLkNhbWJpb24sIFZlcnNpb249MS4xLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1udWxshaREYXRh0gAAAAGoRGF0YVR5cGXZZ1N5c3RlbS5JbnQzMiwgU3lzdGVtLlByaXZhdGUuQ29yZUxpYiwgVmVyc2lvbj04LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPTdjZWM4NWQ3YmVhNzc5OGWsUmVzcG9uc2VUeXBlwKtNZXNzYWdlVHlwZQCtQ29ycmVsYXRpb25JZNkkMDAwMDAwMDAtMDAwMC0wMDAwLTAwMDAtMDAwMDAwMDAwMDAw");

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

        [Fact]
        public async Task SerializeAsync_And_DeserializeAsync_ComplexObject_ReturnsIdentical()
        {
            ComplexObject expectedObj = new()
            {
                InnerList =
                [
                    new SimpleObject
                    {
                        IntValue = 1,
                        StringValue = "One"
                    },
                    new SimpleObject
                    {
                        IntValue = 2,
                        StringValue = "Two"
                    }
                ],
                InnerObject = new SimpleObject
                {
                    IntValue = 3,
                    StringValue = "Three"
                }
            };
            MessageWrapper expected = new()
            {
                CorrelationId = Guid.Empty,
                Data = expectedObj,
                DataType = typeof(ComplexObject),
                MessageType = MessageType.Event,
                ResponseType = null
            };

            byte[] result = await _serializer.SerializeAsync(expected);

            MessageWrapper actual = await _serializer.DeserializeAsync(result);

            Assert.IsType<ComplexObject>(actual.Data);

            var actualObj = (ComplexObject)actual.Data;
            Assert.Equal(expectedObj.InnerObject.IntValue, actualObj.InnerObject.IntValue);
            Assert.Equal(expectedObj.InnerObject.StringValue, actualObj.InnerObject.StringValue);
            Assert.Equal(expectedObj.InnerList.Count, actualObj.InnerList.Count);
            Assert.Equal(expectedObj.InnerList[0].IntValue, actualObj.InnerList[0].IntValue);
            Assert.Equal(expectedObj.InnerList[0].StringValue, actualObj.InnerList[0].StringValue);
            Assert.Equal(expectedObj.InnerList[1].IntValue, actualObj.InnerList[1].IntValue);
            Assert.Equal(expectedObj.InnerList[1].StringValue, actualObj.InnerList[1].StringValue);
        }
    }

    public class ComplexObject
    {
        public SimpleObject InnerObject { get; set; }
        public List<SimpleObject> InnerList { get; set; }
    }

    public class SimpleObject
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
    }
}
