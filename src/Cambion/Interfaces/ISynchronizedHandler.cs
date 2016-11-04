namespace Whitestone.Cambion.Interfaces
{
    public interface ISynchronizedHandler
    {
    }

    public interface ISynchronizedHandler<in TInput, out TOutput> : ISynchronizedHandler
    {
        TOutput Handle(TInput input);
    }
}
