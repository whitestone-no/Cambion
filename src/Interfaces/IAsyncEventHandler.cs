using System.Threading.Tasks;

namespace Whitestone.Cambion.Interfaces
{
    public interface IAsyncEventHandler
    {
    }

    public interface IAsyncEventHandler<in TInput> : IAsyncEventHandler
    {
        Task HandleEventAsync(TInput input);
    }
}
