using Whitestone.Cambion.Configurations;

namespace Whitestone.Cambion.Interfaces
{
    public interface ICambionConfiguration
    {
        TransportConfiguration Transport { get; }
        SerializerConfiguration Serializer { get; }

        ICambion Create();
    }
}
