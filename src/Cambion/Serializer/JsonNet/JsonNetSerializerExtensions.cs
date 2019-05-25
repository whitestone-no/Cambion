using Whitestone.Cambion.Common.Configurations;
using Whitestone.Cambion.Common.Interfaces;

namespace Whitestone.Cambion.Serializer.JsonNet
{
    public static class JsonNetSerializerExtensions
    {
        public static ICambionConfiguration UseJsonNet(this SerializerConfiguration serializerConfiguration)
        {
            JsonNetSerializer serializer = new JsonNetSerializer();
            return serializerConfiguration.Serializer(serializer);
        }
    }
}
