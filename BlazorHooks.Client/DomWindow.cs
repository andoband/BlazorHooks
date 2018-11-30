using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorHooks.Client
{
    public class DomWindow
    {
        private IJSInProcessRuntime js;

        public int Width { get; set; }
        public event Action OnWindowResized;

        public void Init()
        {
            js = (IJSInProcessRuntime)JSRuntime.Current;
            Width = js.Invoke<int>("domWindow.init", new DotNetObjectRef(this));
        }

        public void SetTitle(string title)
        {
            js.Invoke<object>("domWindow.setTitle", title);
        }

        [JSInvokable]
        public void DomWindowResized(int width)
        {
            Width  = width;
            OnWindowResized?.Invoke();
        }


    }
}
