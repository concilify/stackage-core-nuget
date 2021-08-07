using System;
using System.Diagnostics;

namespace Stackage.Core.Polly.Metrics
{
   public class TimerFactory : ITimerFactory
   {
      public ITimer CreateAndStart()
      {
         return new Timer(Stopwatch.StartNew());
      }

      private class Timer : ITimer
      {
         private readonly Stopwatch _stopwatch;

         public Timer(Stopwatch stopwatch)
         {
            _stopwatch = stopwatch;
         }

         public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

         public void Stop() => _stopwatch.Stop();
      }
   }
}
