using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Serializers.JsonNet
{
    public static class SerializerInitializer
    {
        public static void UseJsonNet(this IMessageHandlerInitializer initializer)
        {
            Serializer serializer = new Serializer();
            initializer.Serializer = serializer;
        }
    }
}
