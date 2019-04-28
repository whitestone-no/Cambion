using Whitestone.Cambion.Common.Configurations;

namespace Whitestone.Cambion.Common.Interfaces
{
    public interface ICambionConfiguration
    {
        TransportConfiguration Transport { get; }
        SerializerConfiguration Serializer { get; }

        ICambion Create();
    }
}
