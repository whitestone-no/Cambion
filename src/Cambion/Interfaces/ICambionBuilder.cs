using Microsoft.Extensions.DependencyInjection;

namespace Whitestone.Cambion.Interfaces
{
    public interface ICambionBuilder
    {
        IServiceCollection Services { get; }
    }
}
