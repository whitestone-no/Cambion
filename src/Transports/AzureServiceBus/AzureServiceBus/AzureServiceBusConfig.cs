using System;

namespace Whitestone.Cambion.Transport.AzureSericeBus
{
    public class AzureServiceBusConfig
    {
        public string Endpoint { get; }
        public AzureServiceBusEntity Topic { get; } = new AzureServiceBusEntity("cambion");

        public AzureServiceBusEntity Subscription { get; } = new AzureServiceBusEntity("cambion-" + Guid.NewGuid().ToString("N"));
        public AzureServiceBusAuthentication Autentication { get; } = new AzureServiceBusAuthentication();

        public AzureServiceBusConfig(string endpoint)
        {
            Endpoint = endpoint;
        }

        public class AzureServiceBusEntity
        {
            public AzureServiceBusEntity(string name)
            {
                Name = name;
            }

            public string Name { get; set; }
            public bool AutoCreate { get; set; } = true;
            public bool AutoDelete { get; set; } = true;
        }
        
        public class AzureServiceBusAuthentication
        {
            public string TenantId{ get; set; }
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }

        internal void AssertIsValid()
        {
            if (string.IsNullOrEmpty(Endpoint))
            {
                throw new ArgumentNullException(nameof(Endpoint));
            }
            if (string.IsNullOrEmpty(Topic.Name))
            {
                throw new ArgumentNullException(nameof(Topic.Name));
            }
            if (string.IsNullOrEmpty(Subscription.Name))
            {
                throw new ArgumentNullException(nameof(Subscription.Name));
            }

            if (!UseManagedIdentity())
            {
                if (string.IsNullOrEmpty(Autentication.TenantId))
                {
                    throw new ArgumentNullException(nameof(Autentication.TenantId));
                }
                if (string.IsNullOrEmpty(Autentication.ClientId))
                {
                    throw new ArgumentNullException(nameof(Autentication.ClientId));
                }
                if (string.IsNullOrEmpty(Autentication.ClientSecret))
                {
                    throw new ArgumentNullException(nameof(Autentication.ClientSecret));
                }
            }
        }

        internal bool UseManagedIdentity() =>
            string.IsNullOrEmpty(Autentication.TenantId) &&
            string.IsNullOrEmpty(Autentication.ClientId) &&
            string.IsNullOrEmpty(Autentication.ClientSecret);
    }
}
