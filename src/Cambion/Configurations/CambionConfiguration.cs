using System;
using Whitestone.Cambion.Common.Configurations;
using Whitestone.Cambion.Common.Interfaces;
using Whitestone.Cambion.Serializers.JsonNet;
using Whitestone.Cambion.Transports.Loopback;

namespace Whitestone.Cambion.Configurations
{
    public class CambionConfiguration : ICambionConfiguration
    {
        private ITransport _transport;
        private ISerializer _serializer;
        private bool _cambionCreated;

        public TransportConfiguration Transport { get; internal set; }
        public SerializerConfiguration Serializer { get; internal set; }

        public CambionConfiguration()
        {
            Transport = new TransportConfiguration(this, a => _transport = a);
            Serializer = new SerializerConfiguration(this, a => _serializer = a);
        }

        public ICambion Create()
        {
            if (_cambionCreated)
            {
                throw new InvalidOperationException("Create() has already been called.");
            }
            _cambionCreated = true;

            if (_serializer == null)
            {
                Serializer.UseJsonNet();
            }

            if (_transport == null)
            {
                Transport.UseLoopback();
            }

            return new Cambion(_transport, _serializer);
        }
    }
}
