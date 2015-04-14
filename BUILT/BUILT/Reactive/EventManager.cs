using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Reflection;
using ReactiveUI;


namespace BUILT.Reactive
{
    public class EventManager: IDisposable
    {
        protected Dictionary<Type, List<object>> registration = new Dictionary<Type, List<object>>();
        protected bool _eventsDelegated = false;

        public void DelegateEvents()
        {
            // do we need to lock the registration dict here
            // for this enumeration?
            foreach (var kvp in registration)
            {
                var type = kvp.Key;
                MethodInfo delegateEvent = this.GetType().GetMethod("DelegateEvent").MakeGenericMethod(type);

                foreach (var obj in kvp.Value)
                    delegateEvent.Invoke(this, new object[]{ obj });
            }

            _eventsDelegated = true;
        }

        public void UndelegateEvents()
        {
            // do we need to lock the registration dict here
            // for this enumeration?
            foreach (var kvp in registration)
            {
                var type = kvp.Key;
                MethodInfo delegateEvent = this.GetType().GetMethod("UndelegateEvent").MakeGenericMethod(type);

                foreach (var obj in kvp.Value)
                    delegateEvent.Invoke(this, new object[]{ obj });
            }

            _eventsDelegated = false;
        }

        public void DelegateEvent<T>(EventManagerEvent<T> obj)
        {
            if (obj.Target.IsAlive == false)
            {
                UnregisterEvent<T>(obj);
                return;
            }

            // ensure we are only adding new subscriptions:
            foreach (var sub in obj.Subscriptions)
                sub.Dispose();

            obj.Subscriptions.Clear();

            // create a new subscription
            if (obj.Once)
                createOnceSubscription<T>(obj);
            else
                createSubscription<T>(obj);
        }

        protected void createOnceSubscription<T>(EventManagerEvent<T> obj)
        {
            if (obj.Target.IsAlive == false)
            {
                UnregisterEvent<T>(obj);
                return;
            }

            var subscription = onceSubscriptionForEventModel<T>(obj);

            if(subscription != null)
                obj.Subscriptions.Add(subscription);
        }

        protected void createSubscription<T>(EventManagerEvent<T> obj)
        {
            if (obj.Target.IsAlive == false)
            {
                UnregisterEvent<T>(obj);
                return;
            }

            var subscription = subscriptionForEventModel<T>(obj);

            if(subscription != null)
                obj.Subscriptions.Add(subscription);
        }

        public Expression<Func<TObj, TRet>> BuildLambda<TObj, TRet, TEvt>(EventManagerEvent<TEvt> obj)
        {
            var param = Expression.Parameter(typeof(TObj), "x");
            var access = Expression.PropertyOrField(param, obj.EventName);
            var lambda = Expression.Lambda<Func<TObj, TRet>>(access, param);
            return lambda;
        }

        protected IDisposable onceSubscriptionForEventModel<T>(EventManagerEvent<T> obj)
        {
            IDisposable subscription = null;

            var target = obj.Target.Target;
            var type = target.GetType();
            var property = type.GetRuntimeProperty(obj.EventName);
            var candidate = property.GetValue(target);

            // this is a property on the object
            if (target is IReactiveObject && property != null)
            {
                var propertyType = property.PropertyType;


                MethodInfo build = this.GetType()
                    .GetMethod("BuildLambda")
                    .MakeGenericMethod(type, propertyType, typeof(T));
                
                var o = build.Invoke(this, new object[]{ obj });

                var whenAnyValue = typeof(WhenAnyMixin)
                    .GetMethods()
                    .Single(x => x.Name == "WhenAnyValue" && x.GetParameters().Length == 2)
                    .MakeGenericMethod(type, propertyType);
                
                var o2 = (IObservable<T>)whenAnyValue.Invoke(null, new object[]{ target, o });

                if(obj.SkipInitial)
                    subscription = o2.Skip(1)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(x => {
                        obj.Action((T)x);
                        UnregisterEvent<T>(obj);
                    });
                else
                    subscription = o2
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(x => {
                            obj.Action((T)x);
                            UnregisterEvent<T>(obj);
                    });
            }
            else if (candidate as IObservable<EventPattern<T>> != null)
            {
                var observable = candidate as IObservable<EventPattern<T>>;
                subscription = observable
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => {
                    obj.Action((T)x.EventArgs);
                    UnregisterEvent<T>(obj);
                });
            }
            else if (candidate as IObservable<T> != null)
            {
                var observable = candidate as IObservable<T>;
                subscription = observable
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => {
                    obj.Action((T)x);
                    UnregisterEvent<T>(obj);
                });
            }
            else if (typeof(T).Equals(typeof(Unit)))
            {
                var evt = obj as EventManagerEvent<Unit>;
                var observable = candidate as IObservable<object>;

                subscription = observable
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => {
                    evt.Action(Unit.Default);
                    UnregisterEvent<T>(obj);
                });
            }

            return subscription;
        }

        protected IDisposable subscriptionForEventModel<T>(EventManagerEvent<T> obj)
        {
            IDisposable subscription = null;

            var target = obj.Target.Target;
            var type = target.GetType();
            var property = type.GetRuntimeProperty(obj.EventName);
            var candidate = property.GetValue(target);

            // this is a property on the object
            if (target is IReactiveObject && property != null)
            {
                var propertyType = property.PropertyType;


                MethodInfo build = this.GetType()
                    .GetMethod("BuildLambda")
                    .MakeGenericMethod(type, propertyType, typeof(T));

                var o = build.Invoke(this, new object[]{ obj });

                var whenAnyValue = typeof(WhenAnyMixin)
                    .GetMethods()
                    .Single(x => x.Name == "WhenAnyValue" && x.GetParameters().Length == 2)
                    .MakeGenericMethod(type, propertyType);

                var o2 = (IObservable<T>)whenAnyValue.Invoke(null, new object[]{ target, o });

                if(obj.SkipInitial)
                    subscription = createSubscriptionForObservable<T>(obj, o2.Skip(1));
                else
                    subscription = createSubscriptionForObservable<T>(obj, o2);
            }
            else if (candidate as IObservable<EventPattern<T>> != null)
                subscription = createSubscriptionForEvent<T>(obj, candidate as IObservable<EventPattern<T>>);

            else if (candidate as IObservable<T> != null)
                subscription = createSubscriptionForObservable<T>(obj, candidate as IObservable<T>);

            else if(typeof(T).Equals(typeof(Unit)))
                subscription = createSubscriptionForUnit(obj as EventManagerEvent<Unit>, candidate as IObservable<object>);

            return subscription;
        }

        protected IDisposable createSubscriptionForEvent<T>(EventManagerEvent<T> obj, IObservable<EventPattern<T>> observable)
        {
            var subscription = observable.ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => obj.Action((T)x.EventArgs));
            return subscription;
        }

        protected IDisposable createSubscriptionForObservable<T>(EventManagerEvent<T> obj, IObservable<T> observable)
        {
            var subscription = observable.ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => obj.Action((T)x));
            return subscription;
        }

        protected IDisposable createSubscriptionForUnit(EventManagerEvent<Unit> obj, IObservable<object> observable)
        {
            var subscription = observable.ObserveOn(RxApp.MainThreadScheduler).Subscribe(x => obj.Action(Unit.Default));
            return subscription;
        }

        public void UndelegateEvent<T>(EventManagerEvent<T> obj)
        {
            foreach(var sub in obj.Subscriptions)
                sub.Dispose();

            obj.Subscriptions.Clear();
        }

        public void RegisterEvent<T>(EventManagerEvent<T> evt)
        {
            List<object> list;
            Type type = typeof(T);

            registration.TryGetValue(type, out list);

            if (list == null)
            {
                list = new List<object>();
                registration[type] = list;
            }

            // does this list need to be locked?
            // if we are in the middle of delegating events
            // and we register a new event this might
            // cause an issue.
            list.Add(evt as object);

            if (_eventsDelegated)
                DelegateEvent<T>(evt);
        }

        public void UnregisterEvent<T>(EventManagerEvent<T> evt)
        {
            List<object> list;
            Type type = typeof(T);

            foreach (var sub in evt.Subscriptions)
                sub.Dispose();

            evt.Subscriptions.Clear();

            registration.TryGetValue(type, out list);

            // nothign to unregister
            if (list == null)
                return;

            // does this list need to be locked?
            // if we are in the middle of delegating events
            // and we remove an event this might
            // cause an issue.
            list.Remove(evt);
        }

        /// <summary>
        /// skipInitialValue only applies to ReactiveObject targets
        /// </summary>
        public void ListenTo(object target, string eventName, Action<Unit> action, bool skipInitialValue = true)
        {
            ListenTo<Unit>(target, eventName, action, skipInitialValue);
        }

        /// <summary>
        /// skipInitialValue only applies to ReactiveObject targets
        /// </summary>
        public void ListenTo<T>(object target, string eventName, Action<T> action, bool skipInitialValue = true)
        {
            var evt = new EventManagerEvent<T> {
                Target = new WeakReference(target),
                EventName = eventName,
                Action = action,
                SkipInitial = skipInitialValue,
                Once = false,
            };

            RegisterEvent<T>(evt);
        }

        /// <summary>
        /// skipInitialValue only applies to ReactiveObject targets
        /// </summary>
        public void ListenToOnce(object target, string eventName, Action<Unit> action, bool skipInitialValue = true)
        {
            ListenToOnce<Unit>(target, eventName, action, skipInitialValue);
        }

        /// <summary>
        /// skipInitialValue only applies to ReactiveObject targets
        /// </summary>
        public void ListenToOnce<T>(object target, string eventName, Action<T> action, bool skipInitialValue = true)
        {
            var evt = new EventManagerEvent<T> {
                Target = new WeakReference(target),
                EventName = eventName,
                Action = action,
                SkipInitial = skipInitialValue,
                Once = true,
            };

            RegisterEvent<T>(evt);
        }

        public void Dispose()
        {
            UndelegateEvents();
            registration.Clear();
        }
    }
}