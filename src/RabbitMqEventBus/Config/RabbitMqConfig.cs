namespace RabbitMqEventBus.Config
{
    public class RabbitMqConfig
    {
        public string BrokerName { get; set; }
        public string ExchangeType { get; set; }
        public string QueueName { get; set; }
        public int MaxRetries { get; set; }
    }
}
