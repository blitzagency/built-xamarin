using System;
using System.NMath;
using System.Linq;
using System.Collections.Generic;
using ReactiveUI;

namespace BUILT.Shared.Controllers
{
    public class RangeMarkerEventArgs : EventArgs
    {
        public List<nfloat> Markers { get; set; }
    }

    public class RangePositionEventArgs : EventArgs
    {
        public nfloat FromPosition {get; set;}
        public nfloat ToPosition {get; set;}
    }

    public class RangeController: ReactiveObject
    {
        nfloat _range = 0;
        nfloat _position = 0;
        nfloat _prevPosition = 0;
        List<nfloat> _markers;

        public nfloat Min {get; set;}
        public nfloat Max {get; set;}
        public nfloat Steps { get; set; }

        public event EventHandler<RangeMarkerEventArgs> MarkersReached;
        public event EventHandler<RangePositionEventArgs> PositionChanged;

        public nfloat Position { 
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

        public nfloat Value{
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

        public RangeController (nfloat min, nfloat max)
        {
            _markers = new List<nfloat>();
            Min = min;
            Max = max;
            computeRange();
        }

        public nfloat ValueForPosition(nfloat position)
        {
            var result = NMath.Round(_range * NormalizedPosition(position));
            return result;
        }

        public nfloat PositionForValue(nfloat value)
        {
            var position = value / _range;
            return NormalizedPosition(position);
        }

        void computeRange()
        {
            _range = NMath.Abs(Max - Min);
        }
            
        public nfloat NormalizedPosition(nfloat value)
        {
            var result = RoundPosition(BoundValue(Position, min: 0, max: 1));
            return result;
        }

        public nfloat RoundPosition(nfloat position)
        {
            var multiplier = (nfloat)10000.0;
            return NMath.Round(position * multiplier) / multiplier;
        }

        public nfloat BoundValue(nfloat value, nfloat min, nfloat max)
        {
            var result = value;

            result = result > max ? max : result;
            result = result < min ? min : result;

            return result;
        }

        #region Events
        protected virtual void OnMarkersReached(List<nfloat> markers)
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

        protected virtual void OnPositionChanged(nfloat fromPosition, nfloat toPosition)
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
        public List<nfloat> CheckMarkers(nfloat prevPosition, nfloat position)
        {
            var reached = new List<nfloat> ();

            // when incrementing we will omit the first index (0.0)
            // but include the last index (1.0)
            Action<nfloat> incremental = delegate (nfloat marker) {
                var inbetween = (marker > _prevPosition && marker <= Position);

                if (inbetween)
                    reached.Add(marker);
            };

            Action<nfloat> decremental = delegate(nfloat marker) {
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

        public List<nfloat> AddMarkersForSteps(nfloat steps) 
        {
            var markers = CreateMarkersForSteps(steps);
            AddMarkerPositions(markers.ToArray());

            return markers;
        }
        
        public List<nfloat> CreateMarkersForSteps(nfloat steps)
        {
            var stepDelta = (nfloat)(1.0 / (steps - 1.0));

            // trim it up.
            var max = (int)steps;

            var markers = Enumerable.Range(0, max).Select(x => x * stepDelta);
            return markers.ToList();
        }

        public void AddMarkerPositions(params nfloat[] positions)
        {

            var set = new HashSet<nfloat>(_markers);
           
            foreach(var each in positions)
                set.Add(each);

            _markers = set.ToList();
            _markers.Sort();

        }
        #endregion
    }
}

