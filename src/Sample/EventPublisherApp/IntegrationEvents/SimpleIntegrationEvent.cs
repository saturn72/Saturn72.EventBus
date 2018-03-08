using EventBus.Common;

namespace EventPublisherApp.IntegrationEvents
{
    public class SimpleIntegrationEvent:IntegrationEvent
    {
        public string Content { get; set; }
    }
}
