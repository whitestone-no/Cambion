using NUnit.Framework;
using System;
using Whitestone.Cambion.Common.Interfaces;
using Whitestone.Cambion.Serializers.JsonNet;

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
            string base64expected = "eyIkdHlwZSI6IldoaXRlc3RvbmUuQ2FtYmlvbi5Db21tb24uVHlwZXMuTWVzc2FnZVdyYXBwZXIsIFdoaXRlc3RvbmUuQ2FtYmlvbi5Db21tb24iLCJEYXRhIjoxLCJEYXRhVHlwZSI6IlN5c3RlbS5JbnQzMiwgU3lzdGVtLlByaXZhdGUuQ29yZUxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPTdjZWM4NWQ3YmVhNzc5OGUiLCJSZXNwb25zZVR5cGUiOm51bGwsIk1lc3NhZ2VUeXBlIjowLCJDb3JyZWxhdGlvbklkIjoiMDAwMDAwMDAtMDAwMC0wMDAwLTAwMDAtMDAwMDAwMDAwMDAwIn0=";

            byte[] returns = _serializer.Serialize(new Common.Types.MessageWrapper()
            {
                CorrelationId = Guid.Empty,
                Data = 1,
                DataType = typeof(int),
                MessageType = Common.Types.MessageType.Event,
                ResponseType = null
            });

            string base64return = System.Convert.ToBase64String(returns);

            Assert.AreEqual(base64expected, base64return);
        }

        [Test]
        public void Deserialize_DefaultObject_Equals()
        {
            Common.Types.MessageWrapper expected = new Common.Types.MessageWrapper()
            {
                CorrelationId = Guid.Empty,
                Data = 1,
                DataType = typeof(int),
                MessageType = Common.Types.MessageType.Event,
                ResponseType = null
            };

            byte[] data = Convert.FromBase64String("eyIkdHlwZSI6IldoaXRlc3RvbmUuQ2FtYmlvbi5Db21tb24uVHlwZXMuTWVzc2FnZVdyYXBwZXIsIFdoaXRlc3RvbmUuQ2FtYmlvbi5Db21tb24iLCJEYXRhIjoxLCJEYXRhVHlwZSI6IlN5c3RlbS5JbnQzMiwgU3lzdGVtLlByaXZhdGUuQ29yZUxpYiwgVmVyc2lvbj00LjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPTdjZWM4NWQ3YmVhNzc5OGUiLCJSZXNwb25zZVR5cGUiOm51bGwsIk1lc3NhZ2VUeXBlIjowLCJDb3JyZWxhdGlvbklkIjoiMDAwMDAwMDAtMDAwMC0wMDAwLTAwMDAtMDAwMDAwMDAwMDAwIn0=");

            Common.Types.MessageWrapper actual = _serializer.Deserialize(data);

            Assert.AreEqual(expected, actual);
        }
    }
}