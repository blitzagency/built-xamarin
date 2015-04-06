#if __IOS__

using System;
using Foundation;
using UIKit;


namespace BUILT.iOS.Gestures
{
    public class TouchGesture: UIGestureRecognizer{

        public UIEvent evt { get; set;}
        public NSSet touches { get; set;}

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            this.evt = evt;
            this.touches = touches.Copy() as NSSet;
            State = UIGestureRecognizerState.Began;
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            this.evt = evt;
            this.touches = touches.Copy() as NSSet;
            State = UIGestureRecognizerState.Changed;
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            this.evt = evt;
            this.touches = touches.Copy() as NSSet;
            State = UIGestureRecognizerState.Ended;
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            this.evt = evt;
            this.touches = touches.Copy() as NSSet;
            State = UIGestureRecognizerState.Cancelled;
        }

        public override void Reset()
        {
            evt = null;
            touches = null;
        }
    }
}


#endif