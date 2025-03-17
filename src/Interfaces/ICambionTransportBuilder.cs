using Microsoft.Extensions.DependencyInjection;

namespace Whitestone.Cambion.Interfaces
{
    public interface ICambionTransportBuilder
    {
        IServiceCollection Services { get; }
    }
}
