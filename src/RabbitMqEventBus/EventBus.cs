using EventBus.Common;
using EventBus.Common.Subscriptions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMqEventBus.Config;
using ServiceStack.Text;
using System;
using System.Net.Sockets;
using System.Text;

namespace RabbitMqEventBus
{
    public class EventBus : IEventBus
    {
        #region Fields
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ISubscriptionsManager _subscriptionsManager;
        private readonly ILogger<EventBus> _logger;
        private readonly RabbitMqConfig _config;

        #endregion

        #region ctor

        public EventBus(IRabbitMQPersistentConnection persistentConnection, ILogger<EventBus> logger, ISubscriptionsManager subscriptionsManager,  RabbitMqConfig config)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _subscriptionsManager = subscriptionsManager ?? throw new ArgumentNullException(nameof(subscriptionsManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config;
        }

        #endregion

        public void Publish(IntegrationEvent @event)
        {
            TryConnectIfDisconnected();
            var addToQueuePolicy = RetryPolicy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_config.MaxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, time) => _logger.LogWarning(ex.ToString()));

            using (var channel = _persistentConnection.CreateModel())
            {
                var eventName = @event.GetType().Name;

                channel.ExchangeDeclare(_config.BrokerName, _config.ExchangeType);
                var message = JsonSerializer.SerializeToString(@event);
                var body = Encoding.UTF8.GetBytes(message);

                addToQueuePolicy.Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2; // persistent

                    channel.BasicPublish(exchange: _config.BrokerName,
                                     routingKey: eventName,
                                     mandatory: true,
                                     basicProperties: properties,
                                     body: body);
                });
            }
        }

        public void Subscribe<TIntegrationEvent, TIntegrationEventHandler>()
            where TIntegrationEvent : IntegrationEvent
            where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
        {
            var eventName = _subscriptionsManager.GetEventKey<TIntegrationEvent>();
            DoInternalSubscription(eventName);
            _subscriptionsManager.AddSubscription<TIntegrationEvent, TIntegrationEventHandler>();

            throw new NotImplementedException();
        }

        public void SubscribeDynamic<TDynamicIntegrationEventHandler>(string eventName) where TDynamicIntegrationEventHandler : IDynamicIntegrationEventHandler
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe<TIntegrationEvent, TIntegrationEventHandler>()
            where TIntegrationEvent : IntegrationEvent
            where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeDynamic<TDynamicIntegrationEventHandler>(string eventName) where TDynamicIntegrationEventHandler : IDynamicIntegrationEventHandler
        {
            throw new NotImplementedException();
        }

        #region Utilities
        private void TryConnectIfDisconnected()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }
        }

        private void DoInternalSubscription(string eventName)
        {
            var containsKey = _subscriptionsManager.HasSubscriptionsForEvent(eventName);
            if (!containsKey)
            {
                if (!_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }

                using (var channel = _persistentConnection.CreateModel())
                {
                    channel.QueueBind(queue: _config.QueueName,
                                      exchange: _config.BrokerName,
                                      routingKey: eventName);
                }
            }
        }

        #endregion
    }
}
