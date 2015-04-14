#if __IOS__

using System;
using Foundation;
using UIKit;
using ObjCRuntime;


namespace BUILT.iOS.Gestures
{
    public class TouchGesture: UIGestureRecognizer{

        public UIEvent Evt { get; set;}
        public NSSet Touches { get; set;}

        public TouchGesture(NSObject target, Selector sel): base(target, sel)
        {
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            Evt = evt;
            Touches = touches.Copy() as NSSet;
            State = UIGestureRecognizerState.Began;
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            Evt = evt;
            Touches = touches.Copy() as NSSet;
            State = UIGestureRecognizerState.Changed;
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            Evt = evt;
            Touches = touches.Copy() as NSSet;
            State = UIGestureRecognizerState.Ended;
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            Evt = evt;
            Touches = touches.Copy() as NSSet;
            State = UIGestureRecognizerState.Cancelled;
        }

        public override void Reset()
        {
            Evt = null;
            Touches = null;
        }
    }
}


#endif