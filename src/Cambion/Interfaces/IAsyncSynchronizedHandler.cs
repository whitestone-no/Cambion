using System.Threading.Tasks;

namespace Whitestone.Cambion.Interfaces
{
    public interface IAsyncSynchronizedHandler
    {
    }

    public interface IAsyncSynchronizedHandler<in TInput, TOutput> : IAsyncSynchronizedHandler
    {
        Task<TOutput> HandleSynchronizedAsync(TInput input);
    }
}
