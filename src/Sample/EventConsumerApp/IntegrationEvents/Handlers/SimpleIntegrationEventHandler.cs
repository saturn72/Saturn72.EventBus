using EventBus.Common;
using System;
using System.Threading.Tasks;

namespace EventConsumerApp.IntegrationEvents.Handlers
{
    public class SimpleIntegrationEventHandler : IIntegrationEventHandler<SimpleIntegrationEvent>
    {
        public Task Handle(SimpleIntegrationEvent @event)
        {
            throw new NotImplementedException();
        }
    }
}
