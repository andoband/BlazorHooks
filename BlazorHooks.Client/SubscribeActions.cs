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
    //   Win: Decent looking component code
    //   Win: Helpful API
    //   Win: Most compiler help
    //   Win: Cleanest base class
    //   Win: Tiniest of perf hits
    //   Drawback: Developer must explicitly write unsubscribe code

    public abstract class SubscribeActions : BlazorComponent, IDisposable
    {
        private readonly List<Action> unsubscribes = new List<Action>();
        private readonly List<Action> afterRenderActions = new List<Action>();

        public void Dispose()
        {
            foreach (var unsubscribe in unsubscribes)
                unsubscribe();
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

        protected void Subscribe(Action subscribe, Action unsubscribe)
        {
            subscribe();
            unsubscribes.Add(unsubscribe);
        }
    }
}
