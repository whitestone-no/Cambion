using System;
using Whitestone.Cambion.Common.Interfaces;

namespace Whitestone.Cambion.Common.Configurations
{
    public class SerializerConfiguration
    {
        readonly ICambionConfiguration _cambionConfiguration;
        readonly Action<ISerializer> _addSerializer;

        public SerializerConfiguration(ICambionConfiguration cambionConfiguration, Action<ISerializer> addSerializer)
        {
            _cambionConfiguration = cambionConfiguration;
            _addSerializer = addSerializer;
        }

        public ICambionConfiguration Serializer(ISerializer serializer)
        {
            _addSerializer(serializer);
            return _cambionConfiguration;
        }
    }
}
