using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Transport.AzureSericeBus
{
    public class AzureServiceBusTransport : ITransport
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private readonly AzureServiceBusConfig _config;
        private readonly ILogger<AzureServiceBusTransport> _logger;
        private TopicClient _topicClient;
        private SubscriptionClient _subscriptionClient;
        private ManagementClient _managementClient;
        private ITokenProvider _tokenProvider;

        public AzureServiceBusTransport(IOptions<AzureServiceBusConfig> config, ILogger<AzureServiceBusTransport> logger)
        {
            _logger = logger;
            _config = config.Value;
        }

        public async Task StartAsync()
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

                    AuthenticationResult authResult = await app.AcquireTokenForClient(new[] { $"{serviceBusAudience}/.default" }).ExecuteAsync().ConfigureAwait(false);
                    return authResult.AccessToken;
                }, $"https://login.windows.net/{_config.Autentication.TenantId}");
            }

            _managementClient = new ManagementClient(_config.Endpoint, _tokenProvider);

            if (_config.Topic.AutoCreate) await CreateTopicIfNotExists().ConfigureAwait(false);
            if (_config.Subscription.AutoCreate) await CreateSubscriptionIfNotExists().ConfigureAwait(false);

            _topicClient = new TopicClient(_config.Endpoint, _config.Topic.Name, _tokenProvider);
            _subscriptionClient = new SubscriptionClient(_config.Endpoint, _config.Topic.Name, _config.Subscription.Name, _tokenProvider);

            MessageHandlerOptions messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };
            _subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        public async Task StopAsync()
        {
            await _subscriptionClient.CloseAsync().ConfigureAwait(false);
            if (_config.Subscription.AutoDelete)
            {
                await _managementClient.DeleteSubscriptionAsync(_config.Topic.Name, _config.Subscription.Name).ConfigureAwait(false);
            }

            await _topicClient.CloseAsync().ConfigureAwait(false);
            if (_config.Topic.AutoDelete)
            {
                TopicRuntimeInfo topicInfo = await _managementClient.GetTopicRuntimeInfoAsync(_config.Topic.Name).ConfigureAwait(false);
                if (topicInfo.SubscriptionCount < 1)
                {
                    await _managementClient.DeleteTopicAsync(_config.Topic.Name).ConfigureAwait(false);
                }
            }
        }

        public async Task PublishAsync(byte[] messageBytes)
        {
            if (messageBytes == null)
            {
                throw new ArgumentNullException(nameof(messageBytes));
            }

            await _topicClient.SendAsync(new Message(messageBytes)).ConfigureAwait(false);
        }

        private async Task CreateTopicIfNotExists()
        {
            bool topicExists = await _managementClient.TopicExistsAsync(_config.Topic.Name).ConfigureAwait(false);

            if (!topicExists)
            {
                TopicDescription topic;
                if (_config.Topic.Details == null)
                {
                    topic = new TopicDescription(_config.Topic.Name);
                }
                else
                {
                    topic = _config.Topic.Details;
                    topic.Path = _config.Topic.Name;
                }
                await _managementClient.CreateTopicAsync(topic).ConfigureAwait(false);
            }
        }

        private async Task CreateSubscriptionIfNotExists()
        {
            bool subscriptionExists = await _managementClient.SubscriptionExistsAsync(_config.Topic.Name, _config.Subscription.Name).ConfigureAwait(false);

            if (!subscriptionExists)
            {
                SubscriptionDescription subscription;
                if (_config.Subscription.Details == null)
                {
                    subscription = new SubscriptionDescription(_config.Topic.Name, _config.Subscription.Name);
                }
                else
                {
                    subscription = _config.Subscription.Details;
                    subscription.TopicPath = _config.Topic.Name;
                    subscription.SubscriptionName = _config.Subscription.Name;
                }
                await _managementClient.CreateSubscriptionAsync(subscription).ConfigureAwait(false);
            }
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message.
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message.Body));

            // Complete the message so that it is not received again.
            // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
            if (!token.IsCancellationRequested)
            {
                await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken).ConfigureAwait(false);
            }
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError(exceptionReceivedEventArgs.Exception, "Unhandled exception in AzureServiceBusTransport");

            return Task.CompletedTask;
        }
    }
}
