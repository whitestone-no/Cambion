using System;
using RabbitMQ.Client;

namespace Whitestone.Cambion.Transport.RabbitMQ
{
    public class RabbitMqConfig
    {
        public RabbitMqConnection Connection { get; set; } = new RabbitMqConnection();
        public RabbitMqExchange Exchange { get; set; } = new RabbitMqExchange();
        public RabbitMqQueue Queue { get; set; } = new RabbitMqQueue();

        internal void AssertIsValid()
        {
            if (Connection.ConnectionString != null)
            {
                return;
            }

            if (Connection.Hostname == null)
            {
                throw new ArgumentNullException(nameof(Connection.Hostname));
            }
            if (Connection.Username == null)
            {
                throw new ArgumentNullException(nameof(Connection.Username));
            }
            if (Connection.Password == null)
            {
                throw new ArgumentNullException(nameof(Connection.Password));
            }
        }

        public class RabbitMqConnection
        {
            public string Hostname { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string VirtualHost { get; set; } = "/";
            public int Port { get; set; } = 5672;
            internal Uri ConnectionString { get; set; }
        }

        public class RabbitMqExchange
        {
            public string Name { get; set; } = "cambion.fanout";
            public string Type { get; set; } = ExchangeType.Fanout;
            public bool Durable { get; set; } = true;
            public bool AutoDelete { get; set; } = true;
        }

        public class RabbitMqQueue
        {
            public string Name { get; set; } = "cambion-" + Guid.NewGuid().ToString("N");
            public bool Durable { get; set; } = true;
            public bool Exclusive { get; set; } = true;
            public bool AutoDelete { get; set; } = true;
        }
    }
}
