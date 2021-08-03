using Microsoft.Extensions.Logging;
using Moq;
using Whitestone.Cambion.Serializer.JsonNet;
using Whitestone.Cambion.Transport.Loopback;

namespace Whitestone.Cambion.IntegrationTests.Cambion
{
    public class CambionTests
    {
        private readonly Whitestone.Cambion.Cambion _cambion;

        public CambionTests()
        {
            Mock<ILogger<Whitestone.Cambion.Cambion>> logger = new Mock<ILogger<Whitestone.Cambion.Cambion>>();

            _cambion = new Whitestone.Cambion.Cambion(new LoopbackTransport(), new JsonNetSerializer(), logger.Object);
        }
    }
}
