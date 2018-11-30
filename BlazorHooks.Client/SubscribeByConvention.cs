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
    //   Drawback: some "messy" base class code
    //   Drawback: not as C#-y, compiler can't catch errors
    //   Drawback: tiny perf hit for whizzing through all fields
    //   Drawback: convention-based means things could unknowingly get borked by accidently using convention when it was not intended
    //   Drawback: subclasses must call base.OnInit() if they override OnInit()

    public abstract class SubscribeByConvention : BlazorComponent, IDisposable
    {
        private static readonly Dictionary<Type, List<EventSubscriberInfo>> subscribersByType = new Dictionary<Type, List<EventSubscriberInfo>>();

        public SubscribeByConvention() : base()
        {
            var componentType = GetType();

            // Find all the event handlers for this type
            if (!subscribersByType.ContainsKey(componentType))
            {
                lock (subscribersByType)
                {
                    if (!subscribersByType.ContainsKey(componentType))
                        subscribersByType[componentType] = BuildSubscribers(componentType);
                }
            }
        }

        protected override void OnInit()
        {
            // Subscribe all the conventionally-defined event handlers
            foreach (var subscriber in subscribersByType[GetType()])
                subscriber.Subscribe(this);

            base.OnInit();
        }

        public void Dispose()
        {
            // Unsubscribe all the event handlers
            foreach (var subscriber in subscribersByType[GetType()])
                subscriber.Unsubscribe(this);
        }

        private List<EventSubscriberInfo> BuildSubscribers(Type type)
        {
            var connectors = new List<EventSubscriberInfo>();

            // Gather info for of all props that match the convention:
            //   Action sourcePropName_eventDelegateName => handler;
            // Could also be Action<T>, Action<T1, T2>, etc....
            foreach (var prop in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var nameParts = prop.Name.Split('_');

                if (nameParts.Length != 2)
                    continue;

                // Get the prop that holds the value of the event source
                var sourceProp = type.GetProperty(nameParts[0], BindingFlags.NonPublic | BindingFlags.Instance);
                if (sourceProp == null)
                    continue;

                var sourceType = sourceProp.PropertyType;
                if (sourceType == null)
                    continue;

                // Get the eventInfo for the event
                var sourceEvt = sourceType.GetEvent(nameParts[1]);
                if (sourceEvt == null)
                    continue;

                connectors.Add(new EventSubscriberInfo
                {
                    EventInfo = sourceEvt,
                    HandlerProperty = prop,
                    SourceProperty = sourceProp
                });
            }

            return connectors;
        }
    }

    public class EventSubscriberInfo
    {
        public EventInfo EventInfo { get; set; }
        public PropertyInfo HandlerProperty { get; set; }
        public PropertyInfo SourceProperty { get; set; }

        public void Subscribe(object component)
        {
            EventInfo.AddEventHandler(SourceProperty.GetValue(component), (Delegate)HandlerProperty.GetValue(component));
        }

        public void Unsubscribe(object component)
        {
            EventInfo.RemoveEventHandler(SourceProperty.GetValue(component), (Delegate)HandlerProperty.GetValue(component));
        }
    }
}
