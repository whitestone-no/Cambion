using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Serializer.MessagePack
{
    public static class MessagePackSerializerExtensions
    {
        public static void WithMessagePackSerializer(this ICambionSerializerBuilder builder)
        {
            builder.Services.Replace(new ServiceDescriptor(typeof(ISerializer), typeof(MessagePackSerializer), ServiceLifetime.Singleton));
        }
    }
}
