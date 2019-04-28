using Whitestone.Cambion.Common.Interfaces;

namespace Whitestone.Cambion
{
    internal class Cambion : ICambion
    {
        private ITransport _transport;
        private ISerializer _serializer;

        public Cambion(ITransport transport, ISerializer serializer)
        {
            _transport = transport;
            _serializer = serializer;
        }
    }
}
