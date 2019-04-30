using System;
using System.IO;

namespace Whitestone.Cambion.Common.Interfaces
{
    public interface ICambion
    {
        event EventHandler<ErrorEventArgs> UnhandledException;

        void Register(object handler);
        void AddEventHandler<TEvent>(Action<TEvent> callback);
        void PublishEvent<TEvent>(TEvent data);
        void AddSynchronizedHandler<TRequest, TResponse>(Func<TRequest, TResponse> callback);
        TResponse CallSynchronizedHandler<TRequest, TResponse>(TRequest request, int timeout = 10000);
    }
}
