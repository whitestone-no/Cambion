using System;
using Whitestone.Cambion.Configurations;
using Whitestone.Cambion.Interfaces;
using Whitestone.Cambion.Serializer.JsonNet;
using Whitestone.Cambion.Transport.Loopback;

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
