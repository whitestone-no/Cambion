using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion
{
    [Export(typeof(ICambion))]
    public class MessageHandler : IMessageHandlerInitializer, ICambion
    {
        public IBackendTransport Transport { get; set; }

        readonly List<EventHandler> _eventHandlers = new List<EventHandler>();

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
            if (Transport == null) throw new TypeInitializationException(GetType().FullName, new ArgumentException("Missing transport"));
        }

        public void Register(object handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            lock (_eventHandlers)
            {
                if (_eventHandlers.Any(x => x.Matches(handler)))
                {
                    return;
                }

                _eventHandlers.Add(new EventHandler(handler));
            }
        }

        public void Publish(object data)
        {
            MessageWrapper wrapper = new MessageWrapper
            {
                Data = data,
                DataType = data.GetType(),
                MessageType = MessageType.Event
            };

            string json = JsonConvert.SerializeObject(wrapper, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            byte[] rawBytes = Encoding.ASCII.GetBytes(json);
            Transport.Publish(rawBytes);
        }


        private void Transport_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            string json = Encoding.ASCII.GetString(e.Data);
            MessageWrapper wrapper = JsonConvert.DeserializeObject<MessageWrapper>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

            if (wrapper.MessageType == MessageType.Event)
            {
                EventHandler[] toNotify;
                lock (_eventHandlers)
                {
                    toNotify = _eventHandlers.ToArray();
                }

                List<EventHandler> dead = toNotify
                    .Where(handler => !handler.Handle(wrapper.DataType, wrapper.Data))
                    .ToList();

                if (!dead.Any()) return;

                lock (_eventHandlers)
                {
                    foreach (EventHandler handler in dead)
                    {
                        _eventHandlers.Remove(handler);
                    }
                }
            }
        }
    }
}
