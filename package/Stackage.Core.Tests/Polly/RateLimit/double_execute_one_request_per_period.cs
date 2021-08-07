using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Core.Polly;
using Stackage.Core.Polly.RateLimit;

namespace Stackage.Core.Tests.Polly.RateLimit
{
   public class double_execute_one_request_per_period
   {
      private int _executeCallCount;
      private long _durationMs;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         var rateLimiter = new RateLimiter(1, TimeSpan.FromMilliseconds(50), 1, TimeSpan.FromMinutes(1));
         var policyFactory = new PolicyFactory(new StubTimerFactory());
         var rateLimitPolicy = policyFactory.CreateAsyncRateLimitPolicy(rateLimiter);

         var stopwatch = Stopwatch.StartNew();

         for (var i = 0; i < 2; i++)
         {
            await rateLimitPolicy.ExecuteAsync(() =>
            {
               _executeCallCount++;

               return Task.CompletedTask;
            });
         }

         _durationMs = stopwatch.ElapsedMilliseconds;
      }

      [Test]
      public void should_have_executed_twice()
      {
         Assert.That(_executeCallCount, Is.EqualTo(2));
      }

      [Test]
      public void should_wait_for_limit_period_for_second_execute()
      {
         Assert.That(_durationMs, Is.InRange(40, 80));
      }
   }
}
