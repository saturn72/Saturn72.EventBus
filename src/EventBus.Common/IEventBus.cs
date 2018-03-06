namespace EventBus.Common
{
    public interface IEventBus
    {
        void Publish(IntegrationEvent @event);

        void Subscribe<TIntegrationEvent, TIntegrationEventHandler>()
            where TIntegrationEvent : IntegrationEvent
            where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>;

        void SubscribeDynamic<TDynamicIntegrationEventHandler>(string eventName)
            where TDynamicIntegrationEventHandler  : IDynamicIntegrationEventHandler;

        void UnsubscribeDynamic<TDynamicIntegrationEventHandler>(string eventName)
            where TDynamicIntegrationEventHandler : IDynamicIntegrationEventHandler;

        void Unsubscribe<TIntegrationEvent, TIntegrationEventHandler>()
            where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
            where TIntegrationEvent : IntegrationEvent;
    }
}
