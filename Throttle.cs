using System;
using System.Collections.Generic;
using System.Threading;

namespace HD
{
  public class Throttle
  {
    DateTime lastRun;
    readonly TimeSpan minTimeBetweenRuns;

    public Throttle(
      TimeSpan minTimeBetweenRuns)
    {
      lastRun = DateTime.MinValue;
      this.minTimeBetweenRuns = minTimeBetweenRuns;
    }

    public void SleepIfNeeded()
    {
      TimeSpan timeToSleep = minTimeBetweenRuns - (DateTime.Now - lastRun);
      if (timeToSleep.Ticks > 0)
      {
        Thread.Sleep(timeToSleep);
      }
      lastRun = DateTime.Now;
    }
  }
}
