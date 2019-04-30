namespace Whitestone.Cambion.Common.Interfaces
{
    public interface ISynchronizedHandler
    {
    }

    public interface ISynchronizedHandler<in TInput, out TOutput> : ISynchronizedHandler
    {
        TOutput HandleSynchronized(TInput input);
    }
}
