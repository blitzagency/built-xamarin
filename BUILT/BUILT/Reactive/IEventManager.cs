using System;
using System.Reactive;

namespace BUILT.Reactive
{
    public interface IEventManager
    {
        void ListenTo<T>(object target, string eventName, Action<T> action);
        void ListenTo(object target, string eventName, Action<Unit> action);
        void ListenToOnce<T>(object target, string eventName, Action<T> action);
        void ListenToOnce(object target, string eventName, Action<Unit> action);
    }
}

