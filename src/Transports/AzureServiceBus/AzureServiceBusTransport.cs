using System;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Whitestone.Cambion.Events;
using Whitestone.Cambion.Interfaces;

namespace Whitestone.Cambion.Transport.AzureSericeBus
{
    public class AzureServiceBusTransport : ITransport
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private readonly AzureServiceBusConfig _config;
        private readonly ILogger<AzureServiceBusTransport> _logger;
        private ServiceBusSender _senderClient;
        private ServiceBusProcessor _processorClient;
        private ServiceBusAdministrationClient _managementClient;
        private ServiceBusClient _client;
        private TokenCredential _tokenCredentials;

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
                _tokenCredentials = new ManagedIdentityCredential();
            }
            else
            {
                _tokenCredentials = new ClientSecretCredential(_config.Autentication.TenantId, _config.Autentication.ClientId, _config.Autentication.ClientSecret);
            }

            _managementClient = new ServiceBusAdministrationClient(_config.Endpoint, _tokenCredentials);
            _client = new ServiceBusClient(_config.Endpoint, _tokenCredentials);

            if (_config.Topic.AutoCreate) await CreateTopicIfNotExists().ConfigureAwait(false);
            if (_config.Subscription.AutoCreate) await CreateSubscriptionIfNotExists().ConfigureAwait(false);

            _senderClient = _client.CreateSender(_config.Topic.Name);

            var options = new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 1,
                AutoCompleteMessages = false
            };

            _processorClient = _client.CreateProcessor(_config.Topic.Name, _config.Subscription.Name, options);
            _processorClient.ProcessMessageAsync += MessageHandler;
            _processorClient.ProcessErrorAsync += ExceptionReceivedHandler;

            await _processorClient.StartProcessingAsync().ConfigureAwait(false);
        }

        public async Task StopAsync()
        {
            await _processorClient.StopProcessingAsync().ConfigureAwait(false);
            await _processorClient.CloseAsync().ConfigureAwait(false);
            if (_config.Subscription.AutoDelete)
            {
                await _managementClient.DeleteSubscriptionAsync(_config.Topic.Name, _config.Subscription.Name).ConfigureAwait(false);
            }

            if (_config.Topic.AutoDelete)
            {
                Response<TopicRuntimeProperties> topicInfo = await _managementClient.GetTopicRuntimePropertiesAsync(_config.Topic.Name).ConfigureAwait(false);
                if (topicInfo.Value.SubscriptionCount < 1)
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

            await _senderClient.SendMessageAsync(new ServiceBusMessage(BinaryData.FromBytes(messageBytes))).ConfigureAwait(false);
        }

        private async Task CreateTopicIfNotExists()
        {
            bool topicExists = await _managementClient.TopicExistsAsync(_config.Topic.Name).ConfigureAwait(false);

            if (!topicExists)
            {
                CreateTopicOptions topicOptions;
                if (_config.Topic.Details == null)
                {
                    topicOptions = new CreateTopicOptions(_config.Topic.Name);
                }
                else
                {
                    topicOptions = _config.Topic.Details;
                    topicOptions.Name = _config.Topic.Name;
                }
                await _managementClient.CreateTopicAsync(topicOptions).ConfigureAwait(false);
            }
        }

        private async Task CreateSubscriptionIfNotExists()
        {
            bool subscriptionExists = await _managementClient.SubscriptionExistsAsync(_config.Topic.Name, _config.Subscription.Name).ConfigureAwait(false);

            if (!subscriptionExists)
            {
                CreateSubscriptionOptions subscriptionOptions;
                if (_config.Subscription.Details == null)
                {
                    subscriptionOptions = new CreateSubscriptionOptions(_config.Topic.Name, _config.Subscription.Name);
                }
                else
                {
                    subscriptionOptions = _config.Subscription.Details;
                    subscriptionOptions.TopicName = _config.Topic.Name;
                    subscriptionOptions.SubscriptionName = _config.Subscription.Name;
                }
                await _managementClient.CreateSubscriptionAsync(subscriptionOptions).ConfigureAwait(false);
            }
        }

        private async Task MessageHandler(ProcessMessageEventArgs args)

        {
            // Process the message.
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(args.Message.Body.ToArray()));

            // Complete the message so that it is not received again.
            // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
            await args.CompleteMessageAsync(args.Message);
        }

        private Task ExceptionReceivedHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Unhandled exception in AzureServiceBusTransport");

            return Task.CompletedTask;
        }
    }
}
