using System;
using WatchKit;
using Foundation;

namespace BUILT.iOS.WatchKit
{
	public class CountUpController
	{
		private WKInterfaceTimer interfaceTimer;
		private float secondsPassed = 0;
		private DateTime? startTime;
		private Boolean isPlaying = false;

		private float timerDelta {
			get {
				var now = DateTime.Now;
				var delta = now.Subtract(Convert.ToDateTime(startTime));
				float deltaSeconds = (float)delta.Seconds + (float)delta.Milliseconds / 1000;
				return deltaSeconds;
			}
		}

		public float SecondsPassed {
			get {
				if (startTime == null)
				{
					return secondsPassed;
				}
				else
				{
					return secondsPassed + timerDelta;
				}
			}
		}

		public CountUpController (WKInterfaceTimer _timer)
		{
			interfaceTimer = _timer;

		}

		public void Start()
		{
			if (isPlaying)
				return;
			updateTimerUI();
			interfaceTimer.Start();
			startTime = DateTime.Now;
			isPlaying = true;
		}

		public void Stop()
		{
			if (!isPlaying)
				return;
			secondsPassed += timerDelta;
			isPlaying = false;
			interfaceTimer.Stop();
			startTime = null;
			updateTimerUI();
		}

		public void Reset()
		{
			isPlaying = false;
			interfaceTimer.Stop();
			startTime = null;
			secondsPassed = (float)0;
			updateTimerUI();
		}

		private void updateTimerUI()
		{
			var now = new NSDate ();
			var time = now.AddSeconds(-SecondsPassed);
			interfaceTimer.SetDate(time);
		}

	}
}

