using System;
using System.Threading;
using System.Threading.Tasks;
using Stackage.Core.Abstractions.Polly.RateLimit;

namespace Stackage.Core.Polly.RateLimit
{
   public class RateLimiter : IRateLimiter, IDisposable
   {
      private readonly SemaphoreSlim _tokenBucket;
      private readonly int _newTokensPerTimerCallback;
      private readonly Timer _timer;
      private readonly int _burstSize;
      private readonly TimeSpan _maxWait;

      public RateLimiter(
         int requestsPerPeriod,
         TimeSpan period,
         int burstSize,
         TimeSpan maxWait)
      {
         var periodMs = (long) period.TotalMilliseconds;
         var gcd = GreatestCommonDivisor(requestsPerPeriod, periodMs);

         _newTokensPerTimerCallback = (int) (requestsPerPeriod / gcd);
         var timerPeriodMs = (int) (periodMs / gcd);

         _burstSize = burstSize;
         _maxWait = maxWait;
         _tokenBucket = new SemaphoreSlim(_burstSize);

         _timer = new Timer(TimerCallback, null, timerPeriodMs, timerPeriodMs);
      }

      private void TimerCallback(object state)
      {
         var maxNewTokens = _burstSize - _tokenBucket.CurrentCount;
         var newTokens = Math.Min(maxNewTokens, _newTokensPerTimerCallback);

         if (newTokens > 0)
         {
            _tokenBucket.Release(newTokens);
         }
      }

      public async Task WaitAsync(CancellationToken cancellationToken)
      {
         var allowed = await _tokenBucket.WaitAsync(_maxWait, cancellationToken);

         if (!allowed)
         {
            throw new RateLimitRejectionException();
         }
      }

      private static long GreatestCommonDivisor(long value1, long value2)
      {
         while (value1 != 0 && value2 != 0)
         {
            if (value1 > value2)
               value1 -= value2;
            else
               value2 -= value1;
         }

         return Math.Max(value1, value2);
      }

      public void Dispose()
      {
         _timer?.Dispose();
         _tokenBucket?.Dispose();
      }
   }
}
