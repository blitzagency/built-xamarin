using System;
using System.Linq;
using System.Collections.Generic;


namespace BUILT.Shared.Controllers
{
    public class RangeMarkerEventArgs : EventArgs
    {
        public List<double> Markers { get; set; }
    }

    public class RangePositionEventArgs : EventArgs
    {
        public double FromPosition {get; set;}
        public double ToPosition {get; set;}
    }

    public class RangeController
    {
        double _range = 0;
        double _position = 0;
        double _prevPosition = 0;
        List<double> _markers;

        public double Min {get; set;}
        public double Max {get; set;}
        public double Steps { get; set; }

        public event EventHandler<RangeMarkerEventArgs> MarkersReached;
        public event EventHandler<RangePositionEventArgs> PositionChanged;

        public double Position { 
            get{
                return _position;
            }

            set{ 
                var normalizedValue = NormalizedPosition(value);

                if (normalizedValue.Equals(_position) == false){
                    _prevPosition = _position;
                    _position = value;
                    var markers = CheckMarkers(_prevPosition, _position);

                    OnPositionChanged(_prevPosition, _position);

                    if (markers.Count > 0)
                        OnMarkersReached(markers);
                }
            } 
        }

        public double Value{
            get{
                return ValueForPosition(Position);
            }

            set{
                var convertedValue = PositionForValue(value);
                Position = convertedValue;
            }
        }

        public RangeController ():
            this(min: 0, max: 1)
        {
        }

        public RangeController (double min, double max)
        {
            _markers = new List<double>();
            Min = min;
            Max = max;
            computeRange();
        }

        public double ValueForPosition(double position)
        {
            var result = Math.Round(_range * NormalizedPosition(position));
            return result;
        }

        public double PositionForValue(double value)
        {
            var position = value / _range;
            return NormalizedPosition(position);
        }

        void computeRange()
        {
            _range = Math.Abs(Max - Min);
        }
            
        public double NormalizedPosition(double value)
        {
            var result = RoundPosition(BoundValue(Position, min: 0, max: 1));
            return result;
        }

        public double RoundPosition(double position)
        {
            return Math.Round(Position * 10000.0) / 10000.0;
        }

        public double BoundValue(double value, double min, double max)
        {
            var result = value;

            result = result > max ? max : result;
            result = result < min ? min : result;

            return result;
        }

        #region Events
        protected virtual void OnMarkersReached(List<double> markers)
        {
            var eventHandler = MarkersReached;

            if(eventHandler != null)
            {
                var evt = new RangeMarkerEventArgs {
                    Markers = markers
                };

                eventHandler(this, evt);
            }
        }

        protected virtual void OnPositionChanged(double fromPosition, double toPosition)
        {
            var eventHandler = PositionChanged;

            if(eventHandler != null)
            {
                var evt = new RangePositionEventArgs {
                    FromPosition = fromPosition,
                    ToPosition = toPosition
                };

                eventHandler(this, evt);
            }
        }
        #endregion

        #region Markers
        public List<double> CheckMarkers(double prevPosition, double position)
        {
            var reached = new List<double> ();

            // when incrementing we will omit the first index (0.0)
            // but include the last index (1.0)
            Action<double> incremental = delegate (double marker) {
                var inbetween = (marker > _prevPosition && marker <= Position);

                if (inbetween)
                    reached.Add(marker);
            };

            Action<double> decremental = delegate(double marker) {
                var inbetween = (marker < _prevPosition && marker >= Position);

                if (inbetween)
                    reached.Add(marker);
            };

            var direction = (prevPosition < position) ? "incremental" : "decremental";
            var action  = direction == "incremental" ? incremental : decremental;

            foreach(var each in _markers)
                action(each);

            return reached;
        }

        public List<double> AddMarkersForSteps(double steps) 
        {
            var markers = CreateMarkersForSteps(steps);
            AddMarkerPositions(markers.ToArray());

            return markers;
        }
        
        public List<double> CreateMarkersForSteps(double steps)
        {
            var stepDelta = 1.0 / (steps - 1.0);

            // trim it up.
            var max = (int)steps;

            var markers = Enumerable.Range(0, max).Select(x => x * stepDelta);
            return markers.ToList();
        }

        public void AddMarkerPositions(params double[] positions)
        {

            var set = new HashSet<double>(_markers);
           
            foreach(var each in positions)
                set.Add(each);

            _markers = set.ToList();
            _markers.Sort();

        }
        #endregion
    }
}

