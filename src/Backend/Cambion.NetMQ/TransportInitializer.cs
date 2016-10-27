using System;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Backend.NetMQ
{
    public static class TransportInitializer
    {
        public static void UseNetMq(this IMessageHandlerInitializer initializer, string publishAddress, string subscribeAddress, Action<INetMqConfigurator> initializationHandler = null)
        {
            Transport transport = new Transport(publishAddress, subscribeAddress);

            initializationHandler?.Invoke(transport);

            initializer.Transport = transport;
        }

        public static void StartMessageHost(this INetMqConfigurator busConfigurator)
        {
            MessageHost host = new MessageHost(busConfigurator.SubscribeAddress, busConfigurator.PublishAddress);
            host.Start();
        }
    }
}
