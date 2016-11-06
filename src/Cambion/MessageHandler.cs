using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion
{
    [Export(typeof(ICambion))]
    public class MessageHandler : IMessageHandlerInitializer, ICambion
    {
        public IBackendTransport Transport { get; set; }

        private Dictionary<Type, List<Action<object>>> _eventHandlers = new Dictionary<Type, List<Action<object>>>();

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
                        Action<object> methodAction = request => @delegate.DynamicInvoke(request);

                        if (!_eventHandlers.ContainsKey(type))
                        {
                            _eventHandlers[type] = new List<Action<object>>();
                        }

                        if (!_eventHandlers[type].Contains(methodAction))
                        {
                            _eventHandlers[type].Add(methodAction);
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

            Type type = typeof(TEvent);

            lock (_eventHandlers)
            {
                if (!_eventHandlers.ContainsKey(type))
                {
                    _eventHandlers[type] = new List<Action<object>>();
                }

                Action<object> handler = request => callback((TEvent)request);

                if (!_eventHandlers[type].Contains(handler))
                {
                    _eventHandlers[type].Add(handler);
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

            string json = JsonConvert.SerializeObject(wrapper, Formatting.None,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            byte[] rawBytes = Encoding.ASCII.GetBytes(json);
            Transport.Publish(rawBytes);
        }


        private void Transport_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            string json = Encoding.ASCII.GetString(e.Data);
            MessageWrapper wrapper = JsonConvert.DeserializeObject<MessageWrapper>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

            if (wrapper.MessageType == MessageType.Event)
            {
                lock (_eventHandlers)
                {
                    Action<object>[] actions = _eventHandlers.Where(h => h.Key == wrapper.DataType).SelectMany(h => h.Value).ToArray();
                    foreach (Action<object> action in actions)
                    {
                        // TODO: This way of checking if the action still exists is not a good way to do it as NULLing the object (and running GC (like in CambionTester)) does not actually remove the object. Need to add some WeakReference objects both to Register and AddEventHandler.
                        try
                        {
                            action(wrapper.Data);
                        }
                        catch
                        {
                            _eventHandlers[wrapper.DataType].Remove(action);
                        }
                    }
                }
            }
        }
    }
}
