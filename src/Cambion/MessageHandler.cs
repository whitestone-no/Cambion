using System;
using System.Text;
using Newtonsoft.Json;
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

        private void Validate()
        {
            if (Transport == null) throw new TypeInitializationException(GetType().FullName, new ArgumentException("Missing transport"));
        }

        public void Reinitialize(Action<IMessageHandlerInitializer> initializer)
        {
            Transport.MessageReceived -= Transport_MessageReceived;
            Transport.Dispose();

            initializer(this);
            Validate();
            Transport.MessageReceived += Transport_MessageReceived;
        }

        public void Publish(object data)
        {
            MessageWrapper wrapper = new MessageWrapper
            {
                Message = data,
                MessageType = data.GetType(),
                SomeOtherMeta = 47
            };

            string json = JsonConvert.SerializeObject(wrapper, Formatting.None);
            byte[] rawBytes = Encoding.ASCII.GetBytes(json);
            Transport.Publish(rawBytes);
        }


        private void Transport_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            string json = Encoding.ASCII.GetString(e.Data);
            MessageWrapper wrapper = JsonConvert.DeserializeObject<MessageWrapper>(json);

            ;
        }
    }
}
