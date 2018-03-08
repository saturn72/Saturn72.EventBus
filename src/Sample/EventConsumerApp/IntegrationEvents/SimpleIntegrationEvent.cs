using EventBus.Common;

namespace EventConsumerApp.IntegrationEvents
{
    public class SimpleIntegrationEvent:IntegrationEvent
    {
        public string Content { get; set; }
    }
}
