using System;
using System.Reactive;

namespace BUILT.Reactive
{
    public interface IEventManager
    {
        void ListenTo<T>(object target, string eventName, Action<T> action, bool skipInitialValue=true, bool register=true);
        void ListenTo(object target, string eventName, Action<Unit> action, bool skipInitialValue=true, bool register=true);
        void ListenToOnce<T>(object target, string eventName, Action<T> action, bool skipInitialValue=true, bool register=true);
        void ListenToOnce(object target, string eventName, Action<Unit> action, bool skipInitialValue=true, bool register=true);
    }
}

