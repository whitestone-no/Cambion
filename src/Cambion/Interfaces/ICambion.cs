using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whitestone.Cambion.Interfaces
{
    public interface ICambion
    {
        void Initialize(Action<IMessageHandlerInitializer> initializer);
        void Reinitialize(Action<IMessageHandlerInitializer> initializer);

        void Register(object handler);

        void AddEventHandler<TEvent>(Action<TEvent> callback);
        void PublishEvent<TEvent>(TEvent data);

        void AddSynchronizedHandler<TRequest, TResponse>(Func<TRequest, TResponse> callback);
        TResponse CallSynchronizedHandler<TRequest, TResponse>(TRequest request, int timeout = 10000);
    }
}
