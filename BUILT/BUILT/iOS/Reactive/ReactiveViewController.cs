#if __IOS__
using System;
using System.Reactive;
using System.Reactive.Subjects;
using System.ComponentModel;
using ReactiveUI;
using UIKit;
using BUILT.Reactive;
using Foundation;


namespace BUILT.iOS.Reactive
{
    [Register("ReactiveViewController")]
    public class ReactiveViewController: ReactiveUI.ReactiveViewController, IEventManager
    {
        protected EventManager _eventManager = new EventManager();

        Subject<UIViewController> _willAppear = new Subject<UIViewController>();
        public IObservable<UIViewController> WillAppear {get { return _willAppear;}}

        Subject<UIViewController> _didAppear = new Subject<UIViewController>();
        public IObservable<UIViewController> DidAppear {get { return _didAppear;}}

        Subject<UIViewController> _willDisappear = new Subject<UIViewController>();
        public IObservable<UIViewController> WillDisappear {get { return _willDisappear;}}

        Subject<UIViewController> _didDisappear = new Subject<UIViewController>();
        public IObservable<UIViewController> DidDisappear {get { return _didDisappear;}}

        public ReactiveViewController(IntPtr handle): base(handle)
        {
        
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // delegate events before the 1st event 
            // we allow subscriptions too.
            _eventManager.DelegateEvents();
            _willAppear.OnNext(this);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            _didAppear.OnNext(this);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            _willDisappear.OnNext(this);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _didDisappear.OnNext(this);

            // undelegate events after the last event 
            // we allow subscriptions too.
            _eventManager.UndelegateEvents();
        }

        #region IEventManager implementation

        public void ListenTo<T>(object target, string eventName, Action<T> action, bool skipInitialValue=true, bool register=true)
        {
            _eventManager.ListenTo<T>(target, eventName, action, skipInitialValue, register);
        }

        public void ListenTo(object target, string eventName, Action<Unit> action, bool skipInitialValue=true, bool register=true)
        {
            _eventManager.ListenTo(target, eventName, action, skipInitialValue, register);
        }

        public void ListenToOnce<T>(object target, string eventName, Action<T> action, bool skipInitialValue=true, bool register=true)
        {
            _eventManager.ListenToOnce<T>(target, eventName, action, skipInitialValue, register);
        }

        public void ListenToOnce(object target, string eventName, Action<Unit> action, bool skipInitialValue=true, bool register=true)
        {
            _eventManager.ListenToOnce(target, eventName, action, skipInitialValue, register);
        }

        #endregion
    }
}
#endif