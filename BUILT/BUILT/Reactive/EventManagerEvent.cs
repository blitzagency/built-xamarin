using System;
using System.Collections.Generic;


namespace BUILT.Reactive
{
    public class EventManagerEvent<T>
    {
        public WeakReference Target {get; set;}
        public string EventName {get; set;}
        public Action<T> Action{get; set;}
        public bool Once {get; set;}
        public bool SkipInitial {get; set;}
        public List<IDisposable> Subscriptions {get; set;} = new List<IDisposable>();
    }
}