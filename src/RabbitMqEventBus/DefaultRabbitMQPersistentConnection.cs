using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMqEventBus.Config;
using System;
using System.IO;
using System.Net.Sockets;

namespace RabbitMqEventBus
{
    public class DefaultRabbitMQPersistentConnection : IRabbitMQPersistentConnection
    {
        #region Fields

        private bool _disposed;
        IConnection _connection;

        object sync_root = new object();

        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<DefaultRabbitMQPersistentConnection> _logger;
        private readonly RabbitMqConfig _config;

        #endregion
        
        #region ctor

        public DefaultRabbitMQPersistentConnection(IConnectionFactory connectionFactory, ILogger<DefaultRabbitMQPersistentConnection> logger, RabbitMqConfig config)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        #endregion
        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }

            return _connection.CreateModel();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                _connection.Dispose();
            }
            catch (IOException ex)
            {
                _logger.LogCritical(ex.ToString());
            }
        }

        public bool TryConnect()
        {
            _logger.LogInformation("RabbitMQ Client is trying to connect");

            lock (sync_root)
            {
                var policy = RetryPolicy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(_config.MaxRetries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        _logger.LogWarning(ex.ToString());
                    }
                );

                policy.Execute(() =>
                {
                    _connection = _connectionFactory
                          .CreateConnection();
                });

                if (IsConnected)
                {
                    _connection.ConnectionShutdown += (sender, reason) => LogWarningAndTryConnect("A RabbitMQ connection is on shutdown. Trying to re-connect...");
                    _connection.CallbackException += (sender, eventArgs) => LogWarningAndTryConnect("A RabbitMQ connection throw exception. Trying to re-connect...");
                    _connection.ConnectionBlocked += (sender, eventArgs) => LogWarningAndTryConnect("A RabbitMQ connection is shutdown. Trying to re-connect..."); ;

                    _logger.LogInformation($"RabbitMQ persistent connection acquired a connection {_connection.Endpoint.HostName} and is subscribed to failure events");

                    return true;
                }
                else
                {
                    _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");

                    return false;
                }
            }
        }

        #region Utitlites

        private void LogWarningAndTryConnect(string message)
        {
            if (_disposed) return;

            _logger.LogWarning(message);

            TryConnect();
        }
        
        #endregion
    }
}