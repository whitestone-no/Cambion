using Microsoft.Extensions.DependencyInjection;

namespace Whitestone.Cambion.Interfaces
{
    public interface ICambionSerializerBuilder
    {
        IServiceCollection Services { get; }
    }
}
