using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whitestone.Cambion.Interfaces
{
    public interface ICambion
    {
        /// <summary>
        /// Initialize Cambion with the specified initializer.
        /// </summary>
        /// <param name="initializer">The initializer to use.</param>
        /// <remarks>This method is required for Cambion to function correctly. Without having initialized Cambion, no other methods will work.</remarks>
        void Initialize(Action<IMessageHandlerInitializer> initializer);

        /// <summary>
        /// Reinitalize Cambion. If the application loses connection to i.e. NetMQ, use this method to reconnect.
        /// </summary>
        /// <param name="initializer">The initializer to restart.</param>
        void Reinitialize(Action<IMessageHandlerInitializer> initializer);


        /// <summary>
        /// Register a handler object with Cambion.
        /// </summary>
        /// <param name="handler">An object implementing <see cref="Interfaces.IEventHandler{TInput}"/> or <see cref="Interfaces.ISynchronizedHandler{TInput, TOutput}"/>.</param>
        void Register(object handler);


        /// <summary>
        /// Manually add an event handler to Cambion.
        /// </summary>
        /// <typeparam name="TEvent">The event type this handler will listen for.</typeparam>
        /// <param name="callback">The method that will be called by Cambion if an event of type TEvent is published.</param>
        void AddEventHandler<TEvent>(Action<TEvent> callback);

        /// <summary>
        /// Publish an event in Cambion.
        /// </summary>
        /// <typeparam name="TEvent">The input type to the callback function.</typeparam>
        /// <param name="data">The event data to publish to the handlers.</param>
        void PublishEvent<TEvent>(TEvent data);


        /// <summary>
        /// Manually add a synchronized handler to Cambion.
        /// </summary>
        /// <typeparam name="TRequest">The input type this handler will use.</typeparam>
        /// <typeparam name="TResponse">This is the type returned from the handler.</typeparam>
        /// <param name="callback">The method that will be called by Cambion if someone publishes a synchronized request with TRequest and TResponse.</param>
        void AddSynchronizedHandler<TRequest, TResponse>(Func<TRequest, TResponse> callback);

        /// <summary>
        /// Publish a synchronized request in Cambion
        /// </summary>
        /// <typeparam name="TRequest">The input type to the callback method.</typeparam>
        /// <typeparam name="TResponse">The return type from the callback method.</typeparam>
        /// <param name="request">Data to send into the callback method.</param>
        /// <param name="timeout">How long to wait (in milliseconds) for a synchronized reply before giving up. Default value is 10 seconds.</param>
        /// <returns>The object returned from the callback method.</returns>
        TResponse CallSynchronizedHandler<TRequest, TResponse>(TRequest request, int timeout = 10000);
    }
}
