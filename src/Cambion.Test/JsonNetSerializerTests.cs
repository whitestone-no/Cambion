using NUnit.Framework;
using System;
using Whitestone.Cambion.Interfaces;
using Whitestone.Cambion.Serializer.JsonNet;

namespace Whitestone.Cambion.Test
{
    [Order(1)]
    public class JsonNetSerializerTests
    {
        private ISerializer _serializer;

        [SetUp]
        public void Setup()
        {
            _serializer = new JsonNetSerializer();
        }

        [Test]
        public void Serialize_NullMessage_ThrowsArgumentException()
        {
            Assert.Catch<ArgumentNullException>(() => { _serializer.Serialize(null); });
        }

        [Test]
        public void Serialize_DefaultObject_Equals()
        {
            const string base64Expected = "eyIkdHlwZSI6IldoaXRlc3RvbmUuQ2FtYmlvbi5UeXBlcy5NZXNzYWdlV3JhcHBlciwgV2hpdGVzdG9uZS5DYW1iaW9uIiwiRGF0YSI6MSwiRGF0YVR5cGUiOiJTeXN0ZW0uSW50MzIsIFN5c3RlbS5Qcml2YXRlLkNvcmVMaWIsIFZlcnNpb249NC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj03Y2VjODVkN2JlYTc3OThlIiwiUmVzcG9uc2VUeXBlIjpudWxsLCJNZXNzYWdlVHlwZSI6MCwiQ29ycmVsYXRpb25JZCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCJ9";

            byte[] returns = _serializer.Serialize(new Types.MessageWrapper()
            {
                CorrelationId = Guid.Empty,
                Data = 1,
                DataType = typeof(int),
                MessageType = Types.MessageType.Event,
                ResponseType = null
            });

            string base64Return = Convert.ToBase64String(returns);

            Assert.AreEqual(base64Expected, base64Return);
        }

        [Test]
        public void Deserialize_DefaultObject_Equals()
        {
            Types.MessageWrapper expected = new Types.MessageWrapper()
            {
                CorrelationId = Guid.Empty,
                Data = 1,
                DataType = typeof(int),
                MessageType = Types.MessageType.Event,
                ResponseType = null
            };

            byte[] data = Convert.FromBase64String("eyIkdHlwZSI6IldoaXRlc3RvbmUuQ2FtYmlvbi5UeXBlcy5NZXNzYWdlV3JhcHBlciwgV2hpdGVzdG9uZS5DYW1iaW9uIiwiRGF0YSI6MSwiRGF0YVR5cGUiOiJTeXN0ZW0uSW50MzIsIFN5c3RlbS5Qcml2YXRlLkNvcmVMaWIsIFZlcnNpb249NC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj03Y2VjODVkN2JlYTc3OThlIiwiUmVzcG9uc2VUeXBlIjpudWxsLCJNZXNzYWdlVHlwZSI6MCwiQ29ycmVsYXRpb25JZCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCJ9");

            Types.MessageWrapper actual = _serializer.Deserialize(data);

            Assert.AreEqual(expected, actual);
        }
    }
}