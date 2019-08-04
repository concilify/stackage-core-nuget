using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polly;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Abstractions.Polly;
using Stackage.Core.Abstractions.Polly.RateLimit;
using Stackage.Core.Polly.RateLimit;
using Stackage.Core.Polly.Timing;

namespace Stackage.Core.Polly
{
   public class PolicyFactory : IPolicyFactory
   {
      public IAsyncPolicy CreateAsyncRateLimitPolicy(
         IRateLimiter rateLimiter,
         Func<Context, Exception, Task> onRejectionAsync = null)
      {
         return new AsyncRateLimitPolicy(rateLimiter, onRejectionAsync);
      }

      public IAsyncPolicy<TResult> CreateAsyncRateLimitPolicy<TResult>(
         IRateLimiter rateLimiter,
         Func<Context, Exception, Task> onRejectionAsync = null)
      {
         return new AsyncRateLimitPolicy<TResult>(rateLimiter, onRejectionAsync);
      }

      public IAsyncPolicy CreateAsyncTimingPolicy(
         string name,
         IMetricSink metricSink,
         IDictionary<string, object> policyDimensions = null,
         Func<Context, Task> onSuccessAsync = null,
         Func<Context, Exception, Task> onExceptionAsync = null)
      {
         return new AsyncTimingPolicy(name, metricSink, policyDimensions, onSuccessAsync, onExceptionAsync);
      }

      public IAsyncPolicy<TResult> CreateAsyncTimingPolicy<TResult>(
         string name,
         IMetricSink metricSink,
         IDictionary<string, object> policyDimensions = null,
         Func<Context, TResult, Task> onSuccessAsync = null,
         Func<Context, Exception, Task> onExceptionAsync = null)
      {
         return new AsyncTimingPolicy<TResult>(name, metricSink, policyDimensions, onSuccessAsync, onExceptionAsync);
      }
   }
}
