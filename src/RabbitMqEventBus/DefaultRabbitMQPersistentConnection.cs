using RabbitMQ.Client;

namespace RabbitMqEventBus
{
    public class DefaultRabbitMQPersistentConnection : IRabbitMQPersistentConnection
    {
        private bool _disposed;
        IConnection _connection;


        public bool IsConnected => _connection != null && _connection.IsOpen() && !_disposed;

        public IModel CreateModel()
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public bool TryConnect()
        {
            throw new System.NotImplementedException();
        }
    }
}