using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Polly;
using Stackage.Core.RateLimiting;

namespace Stackage.Core.Tests.Polly.RateLimit
{
   public class limit_one_every_20_ms
   {
      private int _executeCallCount;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         var rateLimiter = new RateLimiter(1, TimeSpan.FromMilliseconds(20), 1, TimeSpan.FromMinutes(1));
         var policyFactory = new PolicyFactory(A.Fake<IMetricSink>(), A.Fake<ITimerFactory>());
         var rateLimitPolicy = policyFactory.CreateAsyncRateLimitingPolicy(rateLimiter);

         var executeCallCount = 0;
         for (var i = 0; i < 100; i++)
         {
            var _ = rateLimitPolicy.ExecuteAsync(() =>
            {
               Interlocked.Increment(ref executeCallCount);

               return Task.CompletedTask;
            });
         }

         await Task.Delay(200);
         _executeCallCount = executeCallCount;
      }


      [Test]
      public void should_have_executed_about_10_times()
      {
         Assert.That(_executeCallCount, Is.InRange(9, 11));
      }
   }
}
