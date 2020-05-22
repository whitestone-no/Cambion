using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Identity.Client;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Interfaces;
using Whitestone.Cambion.Types;

namespace Whitestone.Cambion.Transport.AzureSericeBus
{
    public class AzureServiceBusTransport : ITransport
    {
        public ISerializer Serializer { get; set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private readonly AzureServiceBusConfig _config;
        private TopicClient _topicClient;
        private SubscriptionClient _subscriptionClient;
        private ManagementClient _managementClient;
        private ITokenProvider _tokenProvider;

        public AzureServiceBusTransport(AzureServiceBusConfig config)
        {
            _config = config;
        }

        public void Start()
        {
            AsyncAwaitHelper.RunSync(StartAsync);
        }

        private async Task StartAsync()
        {
            _config.AssertIsValid();

            if (_config.UseManagedIdentity())
            {
                _tokenProvider = TokenProvider.CreateManagedIdentityTokenProvider();
            }
            else
            {
                _tokenProvider = TokenProvider.CreateAzureActiveDirectoryTokenProvider(async (audience, authority, state) =>
                {
                    IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(_config.Autentication.ClientId)
                        .WithAuthority(authority)
                        .WithClientSecret(_config.Autentication.ClientSecret)
                        .Build();

                    Uri serviceBusAudience = new Uri("https://servicebus.azure.net");

                    AuthenticationResult authResult = await app.AcquireTokenForClient(new[] { $"{serviceBusAudience}/.default" }).ExecuteAsync();
                    return authResult.AccessToken;
                }, $"https://login.windows.net/{_config.Autentication.TenantId}");
            }

            _managementClient = new ManagementClient(_config.Endpoint, _tokenProvider);

            if (_config.Topic.AutoCreate) await CreateTopicIfNotExists();
            if (_config.Subscription.AutoCreate) await CreateSubscriptionIfNotExists();

            _topicClient = new TopicClient(_config.Endpoint, _config.Topic.Name, _tokenProvider);
            _subscriptionClient = new SubscriptionClient(_config.Endpoint, _config.Topic.Name, _config.Subscription.Name, _tokenProvider);

            MessageHandlerOptions messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };
            _subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        public void Stop()
        {
            AsyncAwaitHelper.RunSync(StopAsync);
        }

        private async Task StopAsync()
        {
            await _subscriptionClient.CloseAsync();
            if (_config.Subscription.AutoDelete)
            {
                await _managementClient.DeleteSubscriptionAsync(_config.Topic.Name, _config.Subscription.Name);
            }

            await _topicClient.CloseAsync();
            if (_config.Topic.AutoDelete)
            {
                TopicRuntimeInfo topicInfo = await _managementClient.GetTopicRuntimeInfoAsync(_config.Topic.Name);
                if (topicInfo.SubscriptionCount <= 0)
                {
                    await _managementClient.DeleteTopicAsync(_config.Topic.Name);
                }
            }
        }

        public void Publish(MessageWrapper message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            byte[] messageBytes = Serializer.Serialize(message);

            AsyncAwaitHelper.RunSync(() => _topicClient.SendAsync(new Message(messageBytes)));
        }

        private async Task CreateTopicIfNotExists()
        {
            bool topicExists = await _managementClient.TopicExistsAsync(_config.Topic.Name);

            if (!topicExists)
            {
                await _managementClient.CreateTopicAsync(_config.Topic.Name);
            }
        }

        private async Task CreateSubscriptionIfNotExists()
        {
            bool subscriptionExists = await _managementClient.SubscriptionExistsAsync(_config.Topic.Name, _config.Subscription.Name);

            if (!subscriptionExists)
            {
                await _managementClient.CreateSubscriptionAsync(_config.Topic.Name, _config.Subscription.Name);
            }
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message.
            MessageWrapper wrapper = Serializer.Deserialize(message.Body);

            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(wrapper));

            // Complete the message so that it is not received again.
            // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
            if (!token.IsCancellationRequested)
            {
                await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
            }
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");

            ExceptionReceivedContext context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");

            return Task.CompletedTask;
        }
    }
}
