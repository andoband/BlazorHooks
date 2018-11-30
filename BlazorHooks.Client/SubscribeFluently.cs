using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace BlazorHooks.Client
{
    // Compared to other two base classes:
    //   Win: Nice looking component code
    //   Win: Automatically knows how to unsubscribe
    //   Win: Helpful API
    //   Win: More compiler help
    //   Drawback: Lots of "messy" base class code
    //   Drawback: developer must provide magic string for event delegate name (wanted to use expressions but they can't contain event delegates)
    //   Drawback: tiny perf hit for whizzing through api each OnInit() of component

    public abstract class SubscribeFluently : BlazorComponent, IDisposable
    {
        private readonly List<EventSubscription> subscriptions = new List<EventSubscription>();
        private readonly List<Action> afterRenderActions = new List<Action>();

        public IFluentEvents ListenTo(object source)
        {
            return new EventSubscriptionBuilder(source, subscriptions);
        }

        public void Dispose()
        {
            // Unsubscribe all the event handlers
            foreach (var subscription in subscriptions)
                subscription.Unsubscribe();
        }

        protected void AfterRender(Action action)
        {
            afterRenderActions.Add(action);
        }

        protected override void OnAfterRender()
        {
            foreach (var action in afterRenderActions)
                action();
            base.OnAfterRender();
        }
    }

    public interface IFluentEvents
    {
        IFluentEvent On(string eventDelegateName);
        IFluentEvent On<T>(Func<T, object> eventDelegate);
    }

    public interface IFluentEvent
    {
        IFluentEvents Do(Action handler);
        IFluentEvents Do<T>(Action<T> handler);
        IFluentEvents Do<T1, T2>(Action<T1, T2> handler);
        IFluentEvents Do<T1, T2, T3>(Action<T1, T2, T3> handler);
        IFluentEvents Do<T1, T2, T3, T4>(Action<T1, T2, T3, T4> handler);
    }

    public class EventSubscriptionBuilder : IFluentEvents, IFluentEvent
    {
        private readonly object source;
        private readonly List<EventSubscription> subscriptions;
        private EventInfo eventInfo;

        public EventSubscriptionBuilder(object source, List<EventSubscription> subscriptions)
        {
            this.source = source;
            this.subscriptions = subscriptions;
        }

        IFluentEvent IFluentEvents.On(string eventDelegateName)
        {
            eventInfo = source.GetType().GetEvent(eventDelegateName);
            return this;
        }
        IFluentEvent IFluentEvents.On<T>(Func<T, object> eventDelegate)
        {
            throw new NotImplementedException();
        }

        IFluentEvents IFluentEvent.Do(Action handler) => AddSubscription(handler);
        IFluentEvents IFluentEvent.Do<T>(Action<T> handler) => AddSubscription(handler);
        IFluentEvents IFluentEvent.Do<T1, T2>(Action<T1, T2> handler) => AddSubscription(handler);
        IFluentEvents IFluentEvent.Do<T1, T2, T3>(Action<T1, T2, T3> handler) => AddSubscription(handler);
        IFluentEvents IFluentEvent.Do<T1, T2, T3, T4>(Action<T1, T2, T3, T4> handler) => AddSubscription(handler);

        private IFluentEvents AddSubscription(Delegate handler)
        {
            var subscription = new EventSubscription(eventInfo, source, handler);
            subscription.Subscribe();
            subscriptions.Add(subscription);
            return this;
        }
    }

    public class EventSubscription
    {
        private readonly EventInfo eventInfo;
        private readonly object source;
        private readonly Delegate handler;

        public EventSubscription(EventInfo eventInfo, object source, Delegate handler)
        {
            this.eventInfo = eventInfo;
            this.source = source;
            this.handler = handler;
        }

        public void Subscribe() =>
            eventInfo.AddEventHandler(source, handler);

        public void Unsubscribe() =>
            eventInfo.RemoveEventHandler(source, handler);
    }
}
