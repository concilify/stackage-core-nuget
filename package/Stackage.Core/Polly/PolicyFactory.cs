using System;
using System.Threading.Tasks;
using Polly;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Abstractions.Polly;
using Stackage.Core.Abstractions.RateLimiting;
using Stackage.Core.Polly.Metrics;
using Stackage.Core.Polly.RateLimiting;

namespace Stackage.Core.Polly
{
   public class PolicyFactory : IPolicyFactory
   {
      private readonly IMetricSink _metricSink;
      private readonly ITimerFactory _timerFactory;

      public PolicyFactory(
         IMetricSink metricSink,
         ITimerFactory timerFactory)
      {
         _metricSink = metricSink;
         _timerFactory = timerFactory;
      }

      public IAsyncPolicy CreateAsyncRateLimitingPolicy(
         IRateLimiter rateLimiter,
         Func<Context, Exception, Task>? onRejectionAsync = null)
      {
         return new AsyncRateLimitingPolicy(rateLimiter, onRejectionAsync);
      }

      public IAsyncPolicy<TResult> CreateAsyncRateLimitingPolicy<TResult>(
         IRateLimiter rateLimiter,
         Func<Context, Exception, Task>? onRejectionAsync = null)
      {
         return new AsyncRateLimitingPolicy<TResult>(rateLimiter, onRejectionAsync);
      }

      public IAsyncPolicy CreateAsyncMetricsPolicy(
         string name,
         Func<Context, Task>? onSuccessAsync = null,
         Func<Context, Exception, Task>? onExceptionAsync = null)
      {
         return new AsyncMetricsPolicy(name, _metricSink, _timerFactory, onSuccessAsync, onExceptionAsync);
      }

      public IAsyncPolicy<TResult> CreateAsyncMetricsPolicy<TResult>(
         string name,
         Func<Context, TResult, Task>? onSuccessAsync = null,
         Func<Context, Exception, Task>? onExceptionAsync = null)
      {
         return new AsyncMetricsPolicy<TResult>(name, _metricSink, _timerFactory, onSuccessAsync, onExceptionAsync);
      }
   }
}
