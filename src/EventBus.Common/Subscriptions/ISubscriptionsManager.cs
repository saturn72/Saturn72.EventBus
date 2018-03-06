using System;
using System.Collections.Generic;

namespace EventBus.Common.Subscriptions
{
    public interface ISubscriptionsManager
    {
        bool IsEmpty { get; }
        event EventHandler<string> OnEventRemoved;
        void AddDynamicSubscription<TDynamicIntegrationEventHandler>(string eventName)
           where TDynamicIntegrationEventHandler : IDynamicIntegrationEventHandler;

        void AddSubscription<TIntegrationEvent, TIntegrationEventHandler>()
           where TIntegrationEvent : IntegrationEvent
           where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>;

        void RemoveSubscription<TIntegrationEventHandler, TIntegrationEvent>()
            where TIntegrationEventHandler : IIntegrationEventHandler<TIntegrationEvent>
            where TIntegrationEvent : IntegrationEvent;

        void RemoveDynamicSubscription<TDynamicIntegrationEventHandler>(string eventName)
            where TDynamicIntegrationEventHandler : IDynamicIntegrationEventHandler;

        bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent;
        bool HasSubscriptionsForEvent(string eventName);
        Type GetEventTypeByName(string eventName);
        void Clear();
        IEnumerable<SubscriptionInfo> GetHandlersForEvent<TIntegrationEvent>() where TIntegrationEvent : IntegrationEvent;
        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);
        string GetEventKey<T>();
    }
}
