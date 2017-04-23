using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Interfaces;
using EventHandler = Whitestone.Cambion.Handlers.EventHandler;
using System.IO;
using System.Threading;

namespace Whitestone.Cambion
{
    [Export(typeof(ICambion))]
    public class CambionMessageHandler : IMessageHandlerInitializer, ICambion
    {
        public IBackendTransport Transport { get; set; }

        private readonly Dictionary<Type, List<EventHandler>> _eventHandlers = new Dictionary<Type, List<EventHandler>>();

        public event EventHandler<ErrorEventArgs> UnhandledException;

        public void Initialize(Action<IMessageHandlerInitializer> initializer)
        {
            initializer(this);
            Validate();
            Transport.MessageReceived += Transport_MessageReceived;
        }

        public void Reinitialize(Action<IMessageHandlerInitializer> initializer)
        {
            Transport.MessageReceived -= Transport_MessageReceived;
            Transport.Dispose();

            Initialize(initializer);
        }

        private void Validate()
        {
            if (Transport == null)
                throw new TypeInitializationException(GetType().FullName, new ArgumentException("Missing transport"));
        }


        public void Register(object handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            IEnumerable<Type> interfaces = handler.GetType().GetInterfaces()
                .Where(x => typeof(IEventHandler).IsAssignableFrom(x) && x.IsGenericType);

            lock (_eventHandlers)
            {
                foreach (var @interface in interfaces)
                {
                    Type type = @interface.GetGenericArguments()[0];
                    MethodInfo method = @interface.GetMethod("Handle", new Type[] { type });

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
                    }
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
        }

        public void PublishEvent<TEvent>(TEvent data)
        {
            MessageWrapper wrapper = new MessageWrapper
            {
                Data = data,
                DataType = data.GetType(),
                MessageType = MessageType.Event
            };
            BusPublish(wrapper);
        }

        private void Transport_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                string json = Encoding.ASCII.GetString(e.Data);
                MessageWrapper wrapper = JsonConvert.DeserializeObject<MessageWrapper>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

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

        private void BusPublish(MessageWrapper wrapper)
        {
            string json = JsonConvert.SerializeObject(wrapper, Formatting.None,
                            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            byte[] rawBytes = Encoding.ASCII.GetBytes(json);
            Transport.Publish(rawBytes);
        }
    }
}
