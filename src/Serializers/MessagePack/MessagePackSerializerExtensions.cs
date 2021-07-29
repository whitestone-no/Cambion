using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Serializer.MessagePack
{
    public static class MessagePackSerializerExtensions
    {
        public static ICambionBuilder UseMessagePackSerializer(this ICambionBuilder builder)
        {
            builder.Services.Replace(new ServiceDescriptor(typeof(ISerializer), typeof(MessagePackSerializer), ServiceLifetime.Singleton));

            return builder;
        }
    }
}
