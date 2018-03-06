namespace RabbitMqEventBus.Config
{
   public  class RabbitMqConfig
    {
        public RabbitMqConfig(string brokerName, string exchangeType, string queueName, uint maxRetries)
        {
            BrokerName = brokerName;
            ExchangeType = exchangeType;
            QueueName = queueName;
            MaxRetries = (int)maxRetries;
        }

        public string BrokerName { get; }
        public string ExchangeType { get; }
        public string QueueName { get; }
        public int MaxRetries { get; }
    }
}
