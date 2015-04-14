#if __IOS__
using System;
using System.Linq;
using UIKit;
using BUILT.Shared.Controllers;

namespace BUILT.iOS.Controls
{
    public class HorizontalSliderController: SliderController, IDisposable
    {
        
        public HorizontalSliderController(UIView handleView, UIView trackView, nfloat steps):
            base(handleView, trackView, steps)
        {
            RangeController.MarkersReached += OnRangeMarkersReached;
        }

        public HorizontalSliderController(UIView handleView, UIView trackView):
            base(handleView, trackView)
        {
            
        }

        protected override void Dispose(bool disposing)
        {
        }

        public void OnRangeMarkersReached(object source, RangeMarkerEventArgs args)
        {
            var position = args.Markers.Last<nfloat>();
            RangeController.Position = position;
            UpdateHandlePosition(position);
        }

        public override void UpdateHandlePosition(nfloat position)
        {
            var value = RangeController.NormalizedPosition(position);
            var totalWidth = (TrackView.Bounds.Width - HandleView.Bounds.Width);
            var origin = HandleView.Frame.Location;
            origin.X = (totalWidth * value);
        }

        public override void TouchesMoved(Foundation.NSSet touches, UIEvent evt)
        {
            var touch = touches.ToArray<UITouch>()[0];
            var touchLocation = touch.LocationInView(TrackView);

            var delta = touchLocation.X - _touchBeganPoint.X;
                delta = _lastHandleOrigin.X + delta;

            var totalWidth = (TrackView.Bounds.Width - HandleView.Bounds.Width);
            var position = (delta / totalWidth);

                if (_steps == 0){
                RangeController.Position = position;
                UpdateHandlePosition(position);
            } else {

                var reached = RangeController.CheckMarkers(RangeController.Position, position: position);

                if (reached.Count > 0)
                    RangeController.Position = reached.Last<nfloat>();
            }
        }
    }
}
#endif