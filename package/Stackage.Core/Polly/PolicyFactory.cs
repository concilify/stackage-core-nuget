using System;
using System.Threading.Tasks;
using Polly;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Abstractions.Polly;
using Stackage.Core.Abstractions.Polly.RateLimit;
using Stackage.Core.Polly.Metrics;
using Stackage.Core.Polly.RateLimit;

namespace Stackage.Core.Polly
{
   public class PolicyFactory : IPolicyFactory
   {
      private readonly ITimerFactory _timerFactory;

      public PolicyFactory(ITimerFactory timerFactory)
      {
         _timerFactory = timerFactory;
      }

      public IAsyncPolicy CreateAsyncRateLimitPolicy(
         IRateLimiter rateLimiter,
         Func<Context, Exception, Task>? onRejectionAsync = null)
      {
         return new AsyncRateLimitPolicy(rateLimiter, onRejectionAsync);
      }

      public IAsyncPolicy<TResult> CreateAsyncRateLimitPolicy<TResult>(
         IRateLimiter rateLimiter,
         Func<Context, Exception, Task>? onRejectionAsync = null)
      {
         return new AsyncRateLimitPolicy<TResult>(rateLimiter, onRejectionAsync);
      }

      // TODO: Remove IMetricSink from abstractions package
      public IAsyncPolicy CreateAsyncMetricsPolicy(
         string name,
         IMetricSink metricSink,
         Func<Context, Task>? onSuccessAsync = null,
         Func<Context, Exception, Task>? onExceptionAsync = null)
      {
         return new AsyncMetricsPolicy(name, metricSink, _timerFactory, onSuccessAsync, onExceptionAsync);
      }

      // TODO: Remove IMetricSink from abstractions package
      public IAsyncPolicy<TResult> CreateAsyncMetricsPolicy<TResult>(
         string name,
         IMetricSink metricSink,
         Func<Context, TResult, Task>? onSuccessAsync = null,
         Func<Context, Exception, Task>? onExceptionAsync = null)
      {
         return new AsyncMetricsPolicy<TResult>(name, metricSink, _timerFactory, onSuccessAsync, onExceptionAsync);
      }
   }
}
