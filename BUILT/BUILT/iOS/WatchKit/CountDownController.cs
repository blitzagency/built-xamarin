#if __IOS__
using System;
using System.Collections.Generic;
using Foundation;
using WatchKit;
using UIKit;

namespace BUILT.iOS.WatchKit
{

	public class Marker : EventArgs
	{
		public float percent;
		public float seconds;
		public string type;
	}

	public class CountDownController
	{
		
		private WKInterfaceTimer interfaceTimer;
		private NSTimer timer;
		private DateTime? startTime;
		private float secondsPassed = 0;
		private float duration;
		private Boolean isPlaying = false;
		private List<Marker> markerList = new List<Marker> ();
		private List<NSTimer> timerList;

		public event EventHandler CompleteEvent;
		public event EventHandler MarkerEvent;

		public Boolean IsPlaying { get { return isPlaying; } }

		public float TimeRemaining {
			get {
				if (startTime == null)
				{
					return duration - secondsPassed;
				}
				else
				{
					var now = DateTime.Now;
					var delta = now.Subtract(Convert.ToDateTime(startTime));
					float deltaSeconds = (float)delta.Seconds + (float)delta.Milliseconds / 1000;
					return duration - secondsPassed - deltaSeconds;
				}
			}
		}

		public CountDownController (WKInterfaceTimer _timer, float _duration = 0)
		{
			interfaceTimer = _timer;
			duration = _duration;
			updateTimerUI();
		}

		public void Start()
		{
			interfaceTimer.Start();
			startTime = DateTime.Now;
			isPlaying = true;
			timer = NSTimer.CreateScheduledTimer(TimeRemaining, delegate {
				triggerDoneEvent();
			});
			wantsResetAllMarkers();
			updateTimerUI();
		}

		public void Stop()
		{
			secondsPassed = duration - TimeRemaining;
			startTime = null;
			interfaceTimer.Stop();
			isPlaying = false;
			if (timer != null)
			{
				timer.Invalidate();
				timer = null;
			}
			resetMarkerTimers();
			updateTimerUI();
		}

		public void Reset()
		{
			secondsPassed = 0;
			startTime = null;
			interfaceTimer.Stop();
			isPlaying = false;
			if (timer != null)
			{
				timer.Invalidate();
				timer = null;
			}
			resetMarkerTimers();
			updateTimerUI();
		}

		private void updateTimerUI()
		{
			var now = new NSDate ();
			var seconds = TimeRemaining;
			if (!isPlaying && TimeRemaining == duration)
			{
				seconds = TimeRemaining + 1;
			}
			var time = now.AddSeconds(seconds);
			interfaceTimer.SetDate(time);
		}

		private void triggerDoneEvent()
		{
			EventHandler handler = CompleteEvent;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);  
			}
		}

		public void AddMarkerBySeconds(float seconds)
		{
			var marker = new Marker ();
			marker.percent = (float)seconds / (float)duration;
			marker.type = "seconds";
			marker.seconds = seconds;
			addMarker(marker);
		}

		public void AddMarkerByPercent(float percent)
		{
			var marker = new Marker ();
			marker.percent = percent;
			marker.type = "percent";
			addMarker(marker);
		}

		private void addMarker(Marker marker)
		{
			markerList.Add(marker);
			wantsResetAllMarkers();
		}

		private void wantsResetAllMarkers()
		{
			// clear all old timers
			resetMarkerTimers();
			if (!isPlaying)
				return;
			// create all timers
			foreach (var marker in markerList)
			{
				var seconds = marker.percent * duration;
				var timeElapsed = duration - TimeRemaining;
				var updatedSeconds = seconds - timeElapsed;
				if (updatedSeconds < 0)
				{
					continue;
				}
				var markerTimer = NSTimer.CreateScheduledTimer(updatedSeconds, delegate {
					triggerMarkerEvent(marker);
				});
				timerList.Add(markerTimer);
			}
		}

		private void triggerMarkerEvent(Marker marker)
		{
			EventHandler handler = MarkerEvent;
			if (handler != null)
			{
				handler(this, marker);
			}
		}

		private void resetMarkerTimers()
		{
			if (timerList != null)
			{
				foreach (var timer in timerList)
				{
					timer.Invalidate();
				}
			}
			timerList = new List<NSTimer> ();
		}

	}
}

#endif