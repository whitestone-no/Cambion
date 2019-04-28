using System;
using Whitestone.Cambion.Common.Interfaces;

namespace Whitestone.Cambion.Common.Configurations
{
    public class TransportConfiguration
    {
        readonly ICambionConfiguration _cambionConfiguration;
        readonly Action<ITransport> _addTransport;

        public TransportConfiguration(ICambionConfiguration cambionConfiguration, Action<ITransport> addTransport)
        {
            _cambionConfiguration = cambionConfiguration;
            _addTransport = addTransport;
        }

        public ICambionConfiguration Transport(ITransport transport)
        {
            _addTransport(transport);
            return _cambionConfiguration;
        }
    }
}
