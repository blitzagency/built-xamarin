#if __IOS__
using System;
using System.Linq;
using System.NMath;
using Foundation;
using UIKit;
using ObjCRuntime;
using CoreGraphics;
using BUILT.Shared.Controllers;
using BUILT.iOS.Gestures;

namespace BUILT.iOS.Controls
{
    public abstract class SliderController: NSObject
    {
        public UIView HandleView {get; set;}
        public UIView TrackView {get; set;}
        public RangeController RangeController {get; protected set;}
        public TouchGesture TouchGesture {get; protected set;}

        protected CGPoint _touchBeganPoint = CGPoint.Empty;
        protected CGPoint _lastHandleOrigin = CGPoint.Empty;
        protected nfloat _steps = 0;

        public abstract void UpdateHandlePosition(nfloat position);

        public nfloat Value{
            get{

                if(_steps > 0){
                    return StepForPosition(RangeController.Position);
                }

                return RangeController.Position;
            }

            set{
                RangeController.Position = value;
                UpdateHandlePosition(value);
            }
        }
       
        protected SliderController(UIView handleView, UIView trackView, nfloat steps)
            : this(handleView, trackView)
        {
            _steps = steps;
            RangeController.AddMarkersForSteps(steps);
        }

        protected SliderController(UIView handleView, UIView trackView)
        {
            HandleView = handleView;
            TrackView = trackView;
            RangeController = new RangeController();
            TouchGesture = new TouchGesture(this, new Selector("processHandleTouches:"));
            HandleView.AddGestureRecognizer(TouchGesture);
        }

        [Export("processHandleTouches:")]
        protected virtual void processHandleTouches(TouchGesture gesture)
        {
            switch (gesture.State)
            {
                case UIGestureRecognizerState.Began:
                    var touch = gesture.Touches.ToArray<UITouch>()[0];
                    _touchBeganPoint = touch.LocationInView(TrackView);
                    TouchesBegan(gesture.Touches, gesture.Evt);
                    break;
                
                case UIGestureRecognizerState.Changed:
                    TouchesMoved(gesture.Touches, gesture.Evt);
                    break;
                
                case UIGestureRecognizerState.Ended:
                    _lastHandleOrigin = HandleView.Frame.Location;
                    TouchesEnded(gesture.Touches, gesture.Evt);
                    break;

                case UIGestureRecognizerState.Cancelled:
                    TouchesCancelled(gesture.Touches, gesture.Evt);
                    break;

                default:
                    break;
            }
        }

        public nfloat StepForPosition(nfloat position)
        {
            return NMath.Round((_steps - ((nfloat)1.0))  * position);
        }
            
        public nfloat PositionForStep(nfloat value) {
            var steps = (_steps - 1);

            // isNaN check handles 0/0 case
            var position = value / steps;

            return position;
        }

        public virtual void TouchesBegan(NSSet touches, UIEvent evt){
        }

        public virtual void TouchesEnded(NSSet touches, UIEvent evt) {
        }

        public virtual void TouchesMoved(NSSet touches, UIEvent evt) {
        }

        public virtual void TouchesCancelled(NSSet touches, UIEvent evt) {
        }
    }
}
#endif