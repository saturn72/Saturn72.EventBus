using System;
using System.Linq;
using System.Threading.Tasks;
using EventBus.Common.Subscriptions;
using Shouldly;
using Xunit;

namespace EventBus.Common.UnitTests
{
    public class InMemorySubscriptionsManagerUT
    {
        [Fact]
        public void InMemorySubscriptionsManager_GetEventKey()
        {
            var sm = new InMemorySubscriptionsManager();
            sm.GetEventKey<TestIntegrationEvent1>().ShouldBe(typeof(TestIntegrationEvent1).Name);
        }

        [Fact]
        public void InMemorySubscriptionsManager_GetEventTypeByName()
        {
            var sm = new InMemorySubscriptionsManager();
            sm.GetEventTypeByName(typeof(TestIntegrationEvent1).Name).ShouldBeNull();
        }
        [Fact]
        public void InMemorySubscriptionsManager_HasSubscriptionsForEvent()
        {
            var sm = new InMemorySubscriptionsManager();
            sm.HasSubscriptionsForEvent(typeof(TestIntegrationEvent1).Name).ShouldBeFalse();
            sm.HasSubscriptionsForEvent<TestIntegrationEvent1>().ShouldBeFalse();
        }

        [Fact]
        public void InMemorySubscriptionsManager_Clear()
        {
            var sm = new InMemorySubscriptionsManager();
            sm.AddSubscription<TestIntegrationEvent1, TestIntegrationEventHandler1>();
            sm.AddSubscription<TestIntegrationEvent1, TestIntegrationEventHandler2>();

            sm.Clear();
            sm.HasSubscriptionsForEvent(typeof(TestIntegrationEvent1).Name).ShouldBeFalse();
        }

        #region Add/remove subscritions

        [Fact]
        public void InMemorySubscriptionsManager_AddDynamicSubscription()
        {
            var eventName = typeof(TestIntegrationEvent1).Name;
            var sm = new InMemorySubscriptionsManager();
            sm.AddDynamicSubscription<TestDynamicIntegrationEventHandler>(eventName);
            sm.IsEmpty.ShouldBeFalse();
            sm.GetEventTypeByName(eventName).ShouldBeNull();
            sm.HasSubscriptionsForEvent(eventName).ShouldBeTrue();
            sm.HasSubscriptionsForEvent<TestIntegrationEvent1>().ShouldBeTrue();

            //Throws on double registration
            Should.Throw<ArgumentException>(() => sm.AddDynamicSubscription<TestDynamicIntegrationEventHandler>(eventName));
        }

        [Fact]
        public void InMemorySubscriptionsManager_RemoveDynamicSubscription()
        {
            var removedEventName = "";
            var eventName = typeof(TestIntegrationEvent1).Name;
            var sm = new InMemorySubscriptionsManager();
            sm.OnEventRemoved += (sender, e) => removedEventName = e;
            sm.AddDynamicSubscription<TestDynamicIntegrationEventHandler>(eventName);

            sm.RemoveDynamicSubscription<TestDynamicIntegrationEventHandler>(eventName);
            removedEventName.ShouldBe(eventName);
            sm.IsEmpty.ShouldBeTrue();
            sm.GetEventTypeByName(eventName).ShouldBeNull();
            sm.HasSubscriptionsForEvent(eventName).ShouldBeFalse();
            sm.HasSubscriptionsForEvent<TestIntegrationEvent1>().ShouldBeFalse();

            //remove not exists - does nothing          
            sm.AddDynamicSubscription<TestDynamicIntegrationEventHandler>(eventName);

        }

        [Fact]
        public void InMemorySubscriptionsManager_AddSubscription()
        {
            var eventName = typeof(TestIntegrationEvent1).Name;
            var sm = new InMemorySubscriptionsManager();
            sm.AddSubscription<TestIntegrationEvent1, TestIntegrationEventHandler1>();
            sm.IsEmpty.ShouldBeFalse();
            sm.GetEventTypeByName(eventName).Name.ShouldBe(eventName);
            sm.HasSubscriptionsForEvent(eventName).ShouldBeTrue();
            sm.HasSubscriptionsForEvent<TestIntegrationEvent1>().ShouldBeTrue();

            //Throws on double registration
            Should.Throw<ArgumentException>(() => sm.AddSubscription<TestIntegrationEvent1, TestIntegrationEventHandler1>());
        }

        [Fact]
        public void InMemorySubscriptionsManager_RemoveSubscription()
        {
            var removedEventName = "";
            var eventName = typeof(TestIntegrationEvent1).Name;
            var sm = new InMemorySubscriptionsManager();
            sm.OnEventRemoved += (sender, e) => removedEventName = e;
            sm.AddSubscription<TestIntegrationEvent1, TestIntegrationEventHandler1>();

            sm.RemoveSubscription<TestIntegrationEventHandler1, TestIntegrationEvent1>();
            removedEventName.ShouldBe(eventName);
            sm.IsEmpty.ShouldBeTrue();
            sm.GetEventTypeByName(eventName).ShouldBeNull();
            sm.HasSubscriptionsForEvent(eventName).ShouldBeFalse();
            sm.HasSubscriptionsForEvent<TestIntegrationEvent1>().ShouldBeFalse();

            //remove not exists - does nothing
            sm.RemoveSubscription<TestIntegrationEventHandler1, TestIntegrationEvent1>();
        }

        #endregion 
        [Fact]
        public void InMemorySubscriptionsManager_GetHandlersForEvent()
        {
            var eventName = typeof(TestIntegrationEvent1).Name;
            var sm = new InMemorySubscriptionsManager();
            sm.GetHandlersForEvent(eventName).ShouldBeNull();
            sm.GetHandlersForEvent< TestIntegrationEvent1>().ShouldBeNull();

            sm.AddSubscription<TestIntegrationEvent1, TestIntegrationEventHandler1>();
            var handlers0 = sm.GetHandlersForEvent(eventName);
            handlers0.Count().ShouldBe(1);
            handlers0.ShouldContain(x => x.HandlerType == typeof(TestIntegrationEventHandler1));

            sm.AddSubscription<TestIntegrationEvent1, TestIntegrationEventHandler2>();
            var handlers1 = sm.GetHandlersForEvent(eventName);
            handlers1.Count().ShouldBe(2);
            handlers1.ShouldContain(x => x.HandlerType == typeof(TestIntegrationEventHandler1));
            handlers1.ShouldContain(x => x.HandlerType == typeof(TestIntegrationEventHandler2));
        }
    }

    internal class TestIntegrationEvent1: IntegrationEvent
    {

    }

    internal class TestIntegrationEventHandler1 : IIntegrationEventHandler<TestIntegrationEvent1>
    {
        public Task Handle(TestIntegrationEvent1 @event)
        {
            throw new System.NotImplementedException();
        }
    }
    internal class TestIntegrationEventHandler2 : IIntegrationEventHandler<TestIntegrationEvent1>
    {
        public Task Handle(TestIntegrationEvent1 @event)
        {
            throw new System.NotImplementedException();
        }
    }
    internal class TestDynamicIntegrationEventHandler : IDynamicIntegrationEventHandler
    {
        public Task Handle(dynamic eventData)
        {
            throw new NotImplementedException();
        }
    }

}
