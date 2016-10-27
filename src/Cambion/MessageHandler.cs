using System;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion
{
    public class MessageHandler : IMessageHandlerInitializer
    {
        public IBackendTransport Transport { get; set; }

        public MessageHandler(Action<IMessageHandlerInitializer> initializer)
        {
            initializer(this);

            Validate();

            Transport.MessageReceived += Transport_MessageReceived;
        }

        public void Publish(object data)
        {
            MessageWrapper wrapper = new MessageWrapper
            {
                Message = data,
                SomeMeta = data.GetType().ToString(),
                SomeOtherMeta = 47
            };

            Transport.Publish(wrapper);
        }

        private void Validate()
        {
            if (Transport == null) throw new TypeInitializationException(GetType().FullName, new ArgumentException("Missing transport"));
        }

        private void Transport_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            ;
        }
    }
}
