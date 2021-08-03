namespace Whitestone.Cambion.IntegrationTests.Transports.AzureServiceBus
{
    internal class AzureServiceBusTestConfig
    {
        public string Endpoint { get; set; }
        public string TopicName { get; set; }
        public string SubscriptionName { get; set; }
        public string TenantId{ get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
