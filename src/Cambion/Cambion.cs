using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Whitestone.Cambion.Interfaces;
using Whitestone.Cambion.Types;
using EventHandler = Whitestone.Cambion.Types.EventHandler;

namespace Whitestone.Cambion
{
    public class Cambion : ICambion
    {
        private readonly ITransport _transport;
        private readonly ISerializer _serializer;
        private readonly ILogger<Cambion> _logger;

        private readonly Dictionary<Type, List<EventHandler>> _eventHandlers = new Dictionary<Type, List<EventHandler>>();
        private readonly Dictionary<SynchronizedHandlerKey, SynchronizedHandler> _synchronizedHandlers = new Dictionary<SynchronizedHandlerKey, SynchronizedHandler>();
        private readonly Dictionary<Guid, SynchronizedDataPackage> _synchronizationPackages = new Dictionary<Guid, SynchronizedDataPackage>();

        private bool _disposed;

        public event EventHandler<ErrorEventArgs> UnhandledException;

        public Cambion(ITransport transport, ISerializer serializer, ILogger<Cambion> logger)
        {
            _transport = transport;
            _serializer = serializer;
            _logger = logger;

            Validate();

            _transport.MessageReceived += Transport_MessageReceived;
            _transport.Start();
        }

        private void Validate()
        {
            if (_transport == null)
                throw new TypeInitializationException(GetType().FullName, new ArgumentException("Missing transport"));
            if (_serializer == null)
                throw new TypeInitializationException(GetType().FullName, new ArgumentException("Missing serializer"));
        }

        public void Reinitialize()
        {
            Validate();

            _transport.MessageReceived -= Transport_MessageReceived;
            _transport.Stop();

            _transport.MessageReceived += Transport_MessageReceived;
            _transport.Start();
        }

        public void Register(object handler)
        {
            Validate();

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            // Look for, and save, any IEventHandler implementations
            Type handlerType = handler.GetType();
            IEnumerable<Type> eventInterfaces = handlerType.GetInterfaces()
                .Where(x => typeof(IEventHandler).IsAssignableFrom(x) && x.IsGenericType);

            lock (_eventHandlers)
            {
                foreach (var @interface in eventInterfaces)
                {
                    Type type = @interface.GetGenericArguments()[0];
                    MethodInfo method = @interface.GetMethod("HandleEvent", new[] { type });

                    if (method != null)
                    {
                        Delegate @delegate = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(type), handler, method);

                        EventHandler eventHandler = new EventHandler(@delegate);

                        if (!_eventHandlers.ContainsKey(type))
                        {
                            _eventHandlers[type] = new List<EventHandler>();
                        }

                        if (!_eventHandlers[type].Contains(eventHandler))
                        {
                            _eventHandlers[type].Add(eventHandler);
                        }

                        _logger.LogInformation("Registered {handlerType} as event handler for <{handler}>", handlerType.FullName, type.FullName);
                    }
                }
            }

            // Look for, and save, any ISynchronizedHandler implementations
            IEnumerable<Type> synchronizedInterfaces = handlerType.GetInterfaces()
                .Where(x => typeof(ISynchronizedHandler).IsAssignableFrom(x) && x.IsGenericType);

            lock (_synchronizedHandlers)
            {
                foreach (var @interface in synchronizedInterfaces)
                {
                    Type requestType = @interface.GetGenericArguments()[0];
                    Type responseType = @interface.GetGenericArguments()[1];

                    MethodInfo method = @interface.GetMethod("HandleSynchronized", new[] { requestType });

                    if (method != null && method.ReturnType.IsAssignableFrom(responseType))
                    {
                        Delegate @delegate = Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(requestType, responseType), handler, method);

                        SynchronizedHandlerKey key = new SynchronizedHandlerKey(requestType, responseType);

                        SynchronizedHandler synchronizedHandler = new SynchronizedHandler(@delegate);

                        lock (_synchronizedHandlers)
                        {
                            if (_synchronizedHandlers.ContainsKey(key))
                            {
                                throw new ArgumentException($"A SynchronizedHandler already exists for request type {requestType} and response type {responseType}", nameof(@delegate));
                            }

                            _synchronizedHandlers[key] = synchronizedHandler;
                        }

                        _logger.LogInformation("Registered {handlerType} as synchronized handler for <{request}, {response}>", handlerType.FullName, requestType.FullName, responseType.FullName);
                    }
                }
            }
        }

        public void AddEventHandler<TEvent>(Action<TEvent> callback)
        {
            Validate();

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (callback.Target == null)
            {
                throw new ArgumentException("Can't use static methods in callbacks.", nameof(callback));
            }
            
            Type type = typeof(TEvent);

            EventHandler eventHandler = new EventHandler(callback);

            lock (_eventHandlers)
            {
                if (!_eventHandlers.ContainsKey(type))
                {
                    _eventHandlers[type] = new List<EventHandler>();
                }

                if (!_eventHandlers[type].Contains(eventHandler))
                {
                    _eventHandlers[type].Add(eventHandler);
                }
            }

            _logger.LogInformation("Added {handlerType} as event handler for <{handler}>", callback.Target.GetType().FullName, type.FullName);
        }

        public void AddSynchronizedHandler<TRequest, TResponse>(Func<TRequest, TResponse> callback)
        {
            Validate();

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (callback.Target == null)
            {
                throw new ArgumentException("Can't use static methods in callbacks.", nameof(callback));
            }

            Type requestType = typeof(TRequest);
            Type responseType = typeof(TResponse);

            SynchronizedHandlerKey key = new SynchronizedHandlerKey(requestType, responseType);

            SynchronizedHandler synchronizedHandler = new SynchronizedHandler(callback);


            lock (_synchronizedHandlers)
            {
                if (_synchronizedHandlers.ContainsKey(key))
                {
                    throw new ArgumentException($"A SynchronizedHandler already exists for request type {requestType} and response type {responseType}", nameof(callback));
                }

                _synchronizedHandlers[key] = synchronizedHandler;
            }

            _logger.LogInformation("Added {handlerType} as synchronized handler for <{request}, {response}>", callback.Target.GetType().FullName, requestType.FullName, responseType.FullName);
        }

        public void PublishEvent<TEvent>(TEvent data)
        {
            Validate();

            MessageWrapper wrapper = new MessageWrapper
            {
                Data = data,
                DataType = data.GetType(),
                MessageType = MessageType.Event
            };

            byte[] wrapperBytes = _serializer.Serialize(wrapper);

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Publishing event {eventType} with data {data}", typeof(TEvent).FullName, Convert.ToBase64String(wrapperBytes));
            }
            else
            {
                _logger.LogDebug("Publishing event {eventType}", typeof(TEvent).FullName);
            }

            _transport.Publish(wrapperBytes);
        }

        public TResponse CallSynchronizedHandler<TRequest, TResponse>(TRequest request, int timeout = 10000)
        {
            Validate();

            Guid correlationId = Guid.NewGuid();

            ManualResetEvent mre = new ManualResetEvent(false);
            SynchronizedDataPackage pkg = new SynchronizedDataPackage(mre);

            lock (_synchronizationPackages)
            {
                while (_synchronizationPackages.ContainsKey(correlationId))
                {
                    correlationId = Guid.NewGuid();
                }

                _synchronizationPackages[correlationId] = pkg;
            }

            MessageWrapper wrapper = new MessageWrapper
            {
                MessageType = MessageType.SynchronizedRequest,
                Data = request,
                DataType = typeof(TRequest),
                ResponseType = typeof(TResponse),
                CorrelationId = correlationId
            };

            byte[] wrapperBytes = _serializer.Serialize(wrapper);

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Publishing synchronized {eventType} with data {data}", typeof(TRequest).FullName, Convert.ToBase64String(wrapperBytes));
            }
            else
            {
                _logger.LogDebug("Publishing synchronized {eventType}", typeof(TRequest).FullName);
            }

            _transport.Publish(wrapperBytes);

            if (mre.WaitOne(timeout))
            {
                lock (_synchronizationPackages)
                {
                    TResponse result = (TResponse)_synchronizationPackages[correlationId].Data;

                    _synchronizationPackages.Remove(correlationId);

                    return result;
                }
            }

            lock (_synchronizationPackages)
            {
                _synchronizationPackages.Remove(correlationId);
            }

            throw new TimeoutException("Timeout waiting for synchronous call");
        }

        private void Transport_MessageReceived(object sender, Events.MessageReceivedEventArgs e)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("Received message with data {data}", Convert.ToBase64String(e.MessageBytes));
                }

                MessageWrapper wrapper = _serializer.Deserialize(e.MessageBytes);

                if (wrapper.MessageType == MessageType.Event)
                {
                    lock (_eventHandlers)
                    {
                        EventHandler[] handlers = _eventHandlers.Where(h => h.Key.IsAssignableFrom(wrapper.DataType)).SelectMany(h => h.Value).ToArray();
                        foreach (EventHandler handler in handlers)
                        {
                            try
                            {
                                new Thread(() =>
                                {
                                    try
                                    {
                                        if (!handler.Invoke(wrapper.Data))
                                        {
                                            _eventHandlers[wrapper.DataType].Remove(handler);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        PublishUnhandledException(ex);
                                    }
                                }).Start();
                            }
                            catch (Exception ex)
                            {
                                PublishUnhandledException(ex);
                            }
                        }
                    }
                }
                else if (wrapper.MessageType == MessageType.SynchronizedRequest)
                {
                    SynchronizedHandler handler = null;

                    lock (_synchronizedHandlers)
                    {
                        SynchronizedHandlerKey key = new SynchronizedHandlerKey(wrapper.DataType, wrapper.ResponseType);

                        if (_synchronizedHandlers.ContainsKey(key))
                        {
                            handler = _synchronizedHandlers[key];

                            if (!handler.IsAlive)
                            {
                                _synchronizedHandlers.Remove(key);
                            }
                        }
                    }

                    if (handler != null && handler.IsAlive)
                    {
                        new Thread(() =>
                        {
                            try
                            {
                                object result = handler.Invoke(wrapper.Data);

                                MessageWrapper replyWrapper = new MessageWrapper
                                {
                                    MessageType = MessageType.SynchronizedResponse,
                                    Data = result,
                                    DataType = wrapper.DataType,
                                    ResponseType = wrapper.ResponseType,
                                    CorrelationId = wrapper.CorrelationId
                                };

                                byte[] replyWrapperBytes = _serializer.Serialize(replyWrapper);

                                _transport.Publish(replyWrapperBytes);
                            }
                            catch (Exception ex)
                            {
                                PublishUnhandledException(ex);
                            }
                        }).Start();
                    }
                }
                else if (wrapper.MessageType == MessageType.SynchronizedResponse)
                {
                    lock (_synchronizationPackages)
                    {
                        if (_synchronizationPackages.ContainsKey(wrapper.CorrelationId))
                        {
                            SynchronizedDataPackage pkg = _synchronizationPackages[wrapper.CorrelationId];

                            pkg.Data = wrapper.Data;
                            pkg.ResetEvent.Set();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PublishUnhandledException(ex);
            }
        }

        private void PublishUnhandledException(Exception ex)
        {
            EventHandler<ErrorEventArgs> eh = UnhandledException;
            eh?.Invoke(this, new ErrorEventArgs(ex));
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed)
            {
                return;
            }

            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                // Dispose managed resources.
                _transport.MessageReceived -= Transport_MessageReceived;
                _transport.Stop();
            }

            // Note disposing has been done.
            _disposed = true;
        }

        ~Cambion()
        {
            Dispose(false);
        }
    }
}
