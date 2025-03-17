using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Whitestone.Cambion.Events;
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

        private readonly bool _useLoopback;
        private readonly Dictionary<Type, List<EventHandler>> _eventHandlers = new Dictionary<Type, List<EventHandler>>();
        private readonly Dictionary<Type, List<AsyncEventHandler>> _asyncEventHandlers = new Dictionary<Type, List<AsyncEventHandler>>();
        private readonly Dictionary<SynchronizedHandlerKey, SynchronizedHandler> _synchronizedHandlers = new Dictionary<SynchronizedHandlerKey, SynchronizedHandler>();
        private readonly Dictionary<AsyncSynchronizedHandlerKey, AsyncSynchronizedHandler> _asyncSynchronizedHandlers = new Dictionary<AsyncSynchronizedHandlerKey, AsyncSynchronizedHandler>();
        // ReSharper disable once InconsistentNaming
        // Variable is internal only so that it is available to the unit test project. Don't need to change the variable naming for this.
        internal readonly Dictionary<Guid, SynchronizedDataPackage> _synchronizationPackages = new Dictionary<Guid, SynchronizedDataPackage>();

        public event EventHandler<ErrorEventArgs> UnhandledException;

        public Cambion(IServiceProvider serviceProvider, ILogger<Cambion> logger)
        {
            var transport = serviceProvider.GetService<ITransport>();
            var serializer = serviceProvider.GetService<ISerializer>();

            _transport = transport;
            _serializer = serializer;
            _logger = logger;

            Validate();

            _useLoopback = _transport == null || _serializer == null;
        }

        private void Validate()
        {
            if (_logger == null)
            {
                throw new TypeInitializationException(GetType().FullName, new ArgumentException("Missing logger"));
            }

            if (_transport != null && _serializer != null)
            {
                return;
            }

            if (_transport == null)
            {
                _logger.LogInformation("No transport found. Falling back to loopback implementation.");
            }

            if (_serializer == null)
            {
                _logger.LogInformation("No serializer found. Falling back to loopback implementation.");
            }
        }

        public async Task ReinitializeAsync()
        {
            if (_useLoopback)
            {
                _logger.LogWarning("No transport or serializer defined. Using fallback. Nothing to reinitialize.");
                return;
            }

            _logger.LogInformation("Reinitializing Transport {transport}", _transport.GetType().FullName);

            _transport.MessageReceived -= Transport_MessageReceived;
            await _transport.StopAsync().ConfigureAwait(false);

            _transport.MessageReceived += Transport_MessageReceived;
            await _transport.StartAsync().ConfigureAwait(false);
        }

        public void Register(object handler)
        {
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
                foreach (Type @interface in eventInterfaces)
                {
                    Type type = @interface.GetGenericArguments()[0];
                    MethodInfo method = @interface.GetMethod("HandleEvent", new[] { type });

                    if (method == null)
                    {
                        continue;
                    }

                    var @delegate = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(type), handler, method);

                    var eventHandler = new EventHandler(@delegate);

                    if (!_eventHandlers.ContainsKey(type))
                    {
                        _eventHandlers[type] = new List<EventHandler>();
                    }

                    if (!_eventHandlers[type].Contains(eventHandler))
                    {
                        _eventHandlers[type].Add(eventHandler);
                    }

                    _logger.LogInformation("Registered <{handlerType}> as event handler for <{handler}>", handlerType.FullName, type.FullName);
                }
            }

            // Look for, and save, any IAsyncEventHandler implementations
            Type asyncHandlerType = handler.GetType();
            IEnumerable<Type> asyncEventInterfaces = asyncHandlerType.GetInterfaces()
                .Where(x => typeof(IAsyncEventHandler).IsAssignableFrom(x) && x.IsGenericType);

            lock (_asyncEventHandlers)
            {
                foreach (Type @interface in asyncEventInterfaces)
                {
                    Type type = @interface.GetGenericArguments()[0];
                    MethodInfo method = @interface.GetMethod("HandleEventAsync", new[] { type });

                    if (method == null)
                    {
                        continue;
                    }

                    var @delegate = Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(type, typeof(Task)), handler, method);

                    var asyncEventHandler = new AsyncEventHandler(@delegate);

                    if (!_asyncEventHandlers.ContainsKey(type))
                    {
                        _asyncEventHandlers[type] = new List<AsyncEventHandler>();
                    }

                    if (!_asyncEventHandlers[type].Contains(asyncEventHandler))
                    {
                        _asyncEventHandlers[type].Add(asyncEventHandler);
                    }

                    _logger.LogInformation("Registered <{handlerType}> as async event handler for <{handler}>", handlerType.FullName, type.FullName);
                }
            }

            // Look for, and save, any ISynchronizedHandler implementations
            IEnumerable<Type> synchronizedInterfaces = handlerType.GetInterfaces()
                .Where(x => typeof(ISynchronizedHandler).IsAssignableFrom(x) && x.IsGenericType);

            lock (_synchronizedHandlers)
            {
                foreach (Type @interface in synchronizedInterfaces)
                {
                    Type requestType = @interface.GetGenericArguments()[0];
                    Type responseType = @interface.GetGenericArguments()[1];

                    MethodInfo method = @interface.GetMethod("HandleSynchronized", new[] { requestType });

                    if (method == null || !method.ReturnType.IsAssignableFrom(responseType))
                    {
                        continue;
                    }

                    var @delegate = Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(requestType, responseType), handler, method);

                    var key = new SynchronizedHandlerKey(requestType, responseType);

                    var synchronizedHandler = new SynchronizedHandler(@delegate);

                    if (_synchronizedHandlers.ContainsKey(key))
                    {
                        throw new ArgumentException($"A SynchronizedHandler already exists for request type {requestType} and response type {responseType}", nameof(@delegate));
                    }

                    _synchronizedHandlers[key] = synchronizedHandler;

                    _logger.LogInformation("Registered <{handlerType}> as synchronized handler for <{request}, {response}>", handlerType.FullName, requestType.FullName, responseType.FullName);
                }
            }

            // Look for, and save, any IAsyncSynchronizedHandler implementations
            IEnumerable<Type> asyncSynchronizedInterfaces = handlerType.GetInterfaces()
                .Where(x => typeof(IAsyncSynchronizedHandler).IsAssignableFrom(x) && x.IsGenericType);

            lock (_asyncSynchronizedHandlers)
            {
                foreach (Type @interface in asyncSynchronizedInterfaces)
                {
                    Type requestType = @interface.GetGenericArguments()[0];
                    Type responseType = @interface.GetGenericArguments()[1];

                    MethodInfo method = @interface.GetMethod("HandleSynchronizedAsync", new[] { requestType });

                    if (method == null || !method.ReturnType.IsAssignableFrom(typeof(Task<>).MakeGenericType(responseType)))
                    {
                        continue;
                    }

                    var @delegate = Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(requestType, typeof(Task<>).MakeGenericType(responseType)), handler, method);

                    var key = new AsyncSynchronizedHandlerKey(requestType, responseType);

                    var asyncSynchronizedHandler = new AsyncSynchronizedHandler(@delegate);

                    if (_asyncSynchronizedHandlers.ContainsKey(key))
                    {
                        throw new ArgumentException($"An AsyncSynchronizedHandler already exists for request type {requestType} and response type {responseType}", nameof(@delegate));
                    }

                    _asyncSynchronizedHandlers[key] = asyncSynchronizedHandler;

                    _logger.LogInformation("Registered <{handlerType}> as async synchronized handler for <{request}, {response}>", handlerType.FullName, requestType.FullName, responseType.FullName);
                }
            }
        }

        public void AddEventHandler<TEvent>(Action<TEvent> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (callback.Target == null)
            {
                throw new ArgumentException("Can't use static methods in callbacks.", nameof(callback));
            }

            Type type = typeof(TEvent);

            var eventHandler = new EventHandler(callback);

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

            _logger.LogInformation("Added <{handlerType}> as event handler for <{handler}>", callback.Target.GetType().FullName, type.FullName);
        }

        public void AddAsyncEventHandler<TEvent>(Func<TEvent, Task> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (callback.Target == null)
            {
                throw new ArgumentException("Can't use static methods in callbacks.", nameof(callback));
            }

            Type type = typeof(TEvent);

            var asyncEventHandler = new AsyncEventHandler(callback);

            lock (_asyncEventHandlers)
            {
                if (!_asyncEventHandlers.ContainsKey(type))
                {
                    _asyncEventHandlers[type] = new List<AsyncEventHandler>();
                }

                if (!_asyncEventHandlers[type].Contains(asyncEventHandler))
                {
                    _asyncEventHandlers[type].Add(asyncEventHandler);
                }
            }

            _logger.LogInformation("Added <{handlerType}> as async event handler for <{handler}>", callback.Target.GetType().FullName, type.FullName);
        }

        public void AddSynchronizedHandler<TRequest, TResponse>(Func<TRequest, TResponse> callback)
        {
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

            var key = new SynchronizedHandlerKey(requestType, responseType);

            var synchronizedHandler = new SynchronizedHandler(callback);


            lock (_synchronizedHandlers)
            {
                if (_synchronizedHandlers.ContainsKey(key))
                {
                    throw new ArgumentException($"A SynchronizedHandler already exists for request type {requestType} and response type {responseType}", nameof(callback));
                }

                _synchronizedHandlers[key] = synchronizedHandler;
            }

            _logger.LogInformation("Added <{handlerType}> as synchronized handler for <{request}, {response}>", callback.Target.GetType().FullName, requestType.FullName, responseType.FullName);
        }

        public void AddAsyncSynchronizedHandler<TRequest, TResponse>(Func<TRequest, Task<TResponse>> callback)
        {
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

            var key = new AsyncSynchronizedHandlerKey(requestType, responseType);

            var asyncSynchronizedHandler = new AsyncSynchronizedHandler(callback);


            lock (_asyncSynchronizedHandlers)
            {
                if (_asyncSynchronizedHandlers.ContainsKey(key))
                {
                    throw new ArgumentException($"An AsyncSynchronizedHandler already exists for request type {requestType} and response type {responseType}", nameof(callback));
                }

                _asyncSynchronizedHandlers[key] = asyncSynchronizedHandler;
            }

            _logger.LogInformation("Added <{handlerType}> as async synchronized handler for <{request}, {response}>", callback.Target.GetType().FullName, requestType.FullName, responseType.FullName);
        }

        public async Task PublishEventAsync<TEvent>(TEvent data)
        {
            var wrapper = new MessageWrapper()
            {
                Data = data,
                DataType = data.GetType(),
                MessageType = MessageType.Event
            };

            if (_useLoopback)
            {
                _ = Task.Run(() =>
                {
                    HandleMessage(wrapper);
                });
                return;
            }

            byte[] wrapperBytes = await _serializer.SerializeAsync(wrapper).ConfigureAwait(false);

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Publishing event <{eventType}> to Transport with data {data}", typeof(TEvent).FullName, Convert.ToBase64String(wrapperBytes));
            }
            else
            {
                _logger.LogDebug("Publishing event <{eventType}> to Transport", typeof(TEvent).FullName);
            }

            await _transport.PublishAsync(wrapperBytes).ConfigureAwait(false);
        }

        public async Task<TResponse> CallSynchronizedHandlerAsync<TRequest, TResponse>(TRequest request, int timeout = 10000)
        {
            var correlationId = Guid.NewGuid();

            var mre = new ManualResetEvent(false);
            var pkg = new SynchronizedDataPackage(mre);

            lock (_synchronizationPackages)
            {
                while (_synchronizationPackages.ContainsKey(correlationId))
                {
                    correlationId = Guid.NewGuid();
                }

                _synchronizationPackages[correlationId] = pkg;
            }

            var wrapper = new MessageWrapper()
            {
                MessageType = MessageType.SynchronizedRequest,
                Data = request,
                DataType = typeof(TRequest),
                ResponseType = typeof(TResponse),
                CorrelationId = correlationId
            };

            if (_useLoopback)
            {
                _ = Task.Run(() =>
                {
                    HandleMessage(wrapper);
                });
            }
            else
            {
                byte[] wrapperBytes = await _serializer.SerializeAsync(wrapper).ConfigureAwait(false);

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("Publishing synchronized <{eventType}> to Transport with data {data}", typeof(TRequest).FullName, Convert.ToBase64String(wrapperBytes));
                }
                else
                {
                    _logger.LogDebug("Publishing synchronized <{eventType}> to Transport", typeof(TRequest).FullName);
                }

                await _transport.PublishAsync(wrapperBytes).ConfigureAwait(false);
            }

            if (mre.WaitOne(timeout))
            {
                lock (_synchronizationPackages)
                {
                    var result = (TResponse)_synchronizationPackages[correlationId].Data;

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

        // Method could be private but is made internal so that unit tests can access and test it.
        internal async void Transport_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("Received message from Transport with data {data}", Convert.ToBase64String(e.MessageBytes));
                }

                MessageWrapper wrapper = await _serializer.DeserializeAsync(e.MessageBytes).ConfigureAwait(false);

                HandleMessage(wrapper);
            }
            catch (Exception ex)
            {
                PublishUnhandledException(ex);
            }
        }

        private void HandleMessage(MessageWrapper wrapper)
        {
            try
            {
                if (wrapper.MessageType == MessageType.Event)
                {
                    lock (_eventHandlers)
                    {
                        EventHandler[] handlers = _eventHandlers.Where(h => h.Key.IsAssignableFrom(wrapper.DataType)).SelectMany(h => h.Value).ToArray();
                        foreach (EventHandler handler in handlers)
                        {
                            try
                            {
                                _ = Task.Run(() =>
                                {
                                    try
                                    {
                                        if (handler.Invoke(wrapper.Data))
                                        {
                                            return;
                                        }

                                        lock (_eventHandlers)
                                        {
                                            _eventHandlers[wrapper.DataType].Remove(handler);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        PublishUnhandledException(ex);
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                PublishUnhandledException(ex);
                            }
                        }
                    }
                    lock (_asyncEventHandlers)
                    {
                        AsyncEventHandler[] handlers = _asyncEventHandlers.Where(h => h.Key.IsAssignableFrom(wrapper.DataType)).SelectMany(h => h.Value).ToArray();
                        foreach (AsyncEventHandler handler in handlers)
                        {
                            try
                            {
                                _ = Task.Run(async () =>
                                {
                                    try
                                    {
                                        if (await handler.InvokeAsync(wrapper.Data))
                                        {
                                            return;
                                        }
                                        lock (_asyncEventHandlers)
                                        {
                                            _asyncEventHandlers[wrapper.DataType].Remove(handler);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        PublishUnhandledException(ex);
                                    }
                                });
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
                    AsyncSynchronizedHandler asyncHandler = null;

                    lock (_asyncSynchronizedHandlers)
                    {
                        var key = new AsyncSynchronizedHandlerKey(wrapper.DataType, wrapper.ResponseType);
                        if (_asyncSynchronizedHandlers.TryGetValue(key, out AsyncSynchronizedHandler synchronizedHandler))
                        {
                            asyncHandler = synchronizedHandler;

                            if (!asyncHandler.IsAlive)
                            {
                                _asyncSynchronizedHandlers.Remove(key);
                            }
                        }
                    }

                    if (asyncHandler?.IsAlive == true)
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                object result = await asyncHandler.InvokeAsync(wrapper.Data);

                                var replyWrapper = new MessageWrapper()
                                {
                                    MessageType = MessageType.SynchronizedResponse,
                                    Data = result,
                                    DataType = wrapper.DataType,
                                    ResponseType = wrapper.ResponseType,
                                    CorrelationId = wrapper.CorrelationId
                                };

                                if (_useLoopback)
                                {
                                    _ = Task.Run(() =>
                                    {
                                        HandleMessage(replyWrapper);
                                    });
                                    return;
                                }

                                byte[] replyWrapperBytes = await _serializer.SerializeAsync(replyWrapper).ConfigureAwait(false);

                                if (_logger.IsEnabled(LogLevel.Trace))
                                {
                                    _logger.LogTrace("Publishing async synchronized reply <{eventType}> to Transport with data {data}", result.GetType().FullName, Convert.ToBase64String(replyWrapperBytes));
                                }
                                else
                                {
                                    _logger.LogDebug("Publishing async synchronized reply <{eventType}> to Transport", result.GetType().FullName);
                                }

                                await _transport.PublishAsync(replyWrapperBytes).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                PublishUnhandledException(ex);
                            }
                        });

                        return;
                    }

                    SynchronizedHandler handler = null;

                    lock (_synchronizedHandlers)
                    {
                        var key = new SynchronizedHandlerKey(wrapper.DataType, wrapper.ResponseType);

                        if (_synchronizedHandlers.TryGetValue(key, out SynchronizedHandler synchronizedHandler))
                        {
                            handler = synchronizedHandler;

                            if (!handler.IsAlive)
                            {
                                _synchronizedHandlers.Remove(key);
                            }
                        }
                    }

                    if (handler?.IsAlive == true)
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                object result = handler.Invoke(wrapper.Data);

                                var replyWrapper = new MessageWrapper()
                                {
                                    MessageType = MessageType.SynchronizedResponse,
                                    Data = result,
                                    DataType = wrapper.DataType,
                                    ResponseType = wrapper.ResponseType,
                                    CorrelationId = wrapper.CorrelationId
                                };

                                if (_useLoopback)
                                {
                                    _ = Task.Run(() =>
                                    {
                                        HandleMessage(replyWrapper);
                                    });
                                    return;
                                }

                                byte[] replyWrapperBytes = await _serializer.SerializeAsync(replyWrapper).ConfigureAwait(false);

                                if (_logger.IsEnabled(LogLevel.Trace))
                                {
                                    _logger.LogTrace("Publishing synchronized reply <{eventType}> to Transport with data {data}", result.GetType().FullName, Convert.ToBase64String(replyWrapperBytes));
                                }
                                else
                                {
                                    _logger.LogDebug("Publishing synchronized reply <{eventType}> to Transport", result.GetType().FullName);
                                }

                                await _transport.PublishAsync(replyWrapperBytes).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                PublishUnhandledException(ex);
                            }
                        });
                    }
                }
                else if (wrapper.MessageType == MessageType.SynchronizedResponse)
                {
                    lock (_synchronizationPackages)
                    {
                        if (!_synchronizationPackages.TryGetValue(wrapper.CorrelationId, out SynchronizedDataPackage pkg))
                        {
                            return;
                        }

                        pkg.Data = wrapper.Data;
                        pkg.ResetEvent.Set();
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
            _logger.LogWarning(ex, "Background exception in a Cambion handler. Notifying subscribers to the UnhandledException event.");

            EventHandler<ErrorEventArgs> eh = UnhandledException;
            eh?.Invoke(this, new ErrorEventArgs(ex));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_useLoopback)
            {
                _logger.LogInformation("Starting Cambion with loopback");
                return;
            }

            _logger.LogInformation("Starting Cambion with Transport <{transport}> and Serializer <{serializer}>", _transport.GetType().FullName, _serializer.GetType().FullName);

            _transport.MessageReceived += Transport_MessageReceived;
            await _transport.StartAsync().ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Cambion");

            if (_useLoopback)
            {
                return;
            }

            _transport.MessageReceived -= Transport_MessageReceived;
            await _transport.StopAsync().ConfigureAwait(false);
        }
    }
}
