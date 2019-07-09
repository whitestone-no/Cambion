using System;
using System.IO;

namespace Whitestone.Cambion.Interfaces
{
    public interface ICambion
    {
        event EventHandler<ErrorEventArgs> UnhandledException;

        /// <summary>
        /// Restarts the <see cref="Whitestone.Cambion.Interfaces.ITransport"/> associated
        /// with this instance of Cambion.
        /// </summary>
        void Reinitialize();

        /// <summary>
        /// Use "interface subscription" to automatically add all <see cref="IEventHandler"/>
        /// and <see cref="ISynchronizedHandler"/> implementations as subscriptions.
        /// </summary>
        /// <param name="handler">An object that implements one or more of <see cref="IEventHandler"/>
        /// and/or <see cref="ISynchronizedHandler"/>.</param>
        void Register(object handler);

        /// <summary>
        /// Use "direct subscription" to manually add an event subscriber to
        /// handle events that provide a <typeparamref name="TEvent"/> object.
        /// </summary>
        /// <typeparam name="TEvent">The event type to subscribe to.</typeparam>
        /// <param name="callback">The action that will be called when an event
        /// of the specific type is received.</param>
        void AddEventHandler<TEvent>(Action<TEvent> callback);

        /// <summary>
        /// Publish an event with a <typeparamref name="TEvent"/> object to all subscribers of this type.
        /// </summary>
        /// <typeparam name="TEvent">The object type to publish</typeparam>
        /// <param name="data">The data to publish</param>
        void PublishEvent<TEvent>(TEvent data);

        /// <summary>
        /// Use "direct subscription" to manually add a synchronized subscriber
        /// to handle synchronized messages that provide a <typeparamref name="TRequest"/>
        /// object as a request and a <typeparamref name="TResponse"/> object as the response.
        /// </summary>
        /// <typeparam name="TRequest">The synchronized request type to subscribe to.</typeparam>
        /// <typeparam name="TResponse">The synchronized response type to subscribe to.</typeparam>
        /// <param name="callback">The function that will be called when a synchronized is received.</param>
        /// <remarks>The <typeparamref name="TRequest"/> and <typeparamref name="TResponse"/>
        /// types are unique identifiers. Multiple synchronizers can handle the same
        /// <typeparamref name="TRequest"/>, but there should be only one that provides
        /// both <typeparamref name="TRequest"/> and <typeparamref name="TResponse"/>.
        /// If multiple synchronized handlers with identical <typeparamref name="TRequest"/>
        /// and <typeparamref name="TResponse"/> are added, only the response from one
        /// of them will be received by whoever calls <see cref="Whitestone.Cambion.Cambion.CallSynchronizedHandler{TRequest, TResponse}(TRequest, int)"/>,
        /// and you will never know which handler has responded.</remarks>
        void AddSynchronizedHandler<TRequest, TResponse>(Func<TRequest, TResponse> callback);

        /// <summary>
        /// Call a synchronized handler subscribed using the specific <typeparamref name="TRequest"/> and <typeparamref name="TResponse"/>.
        /// </summary>
        /// <typeparam name="TRequest">The synchronized request type the handler subscribed to.</typeparam>
        /// <typeparam name="TResponse">The synchronized response type the handler subscribed to.</typeparam>
        /// <param name="request">The <typeparamref name="TRequest"/> object.</param>
        /// <param name="timeout">How long to wait for a response, in milliseconds. Defaults to 10 seconds.</param>
        /// <returns>A <typeparamref name="TResponse"/> object.</returns>
        /// <remarks>If there is no response within <paramref name="timeout"/> period, a <see cref="System.TimeoutException"/> is thrown</remarks>
        TResponse CallSynchronizedHandler<TRequest, TResponse>(TRequest request, int timeout = 10000);
    }
}
