using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Abstractions.Polly.Timing;

namespace Stackage.Core.Polly.Timing
{
   public class AsyncTimingPolicy : AsyncPolicy, ITimingPolicy, IsPolicy
   {
      private readonly string _name;
      private readonly IMetricSink _metricSink;
      private readonly IDictionary<string, object> _policyDimensions;
      private readonly Func<Context, Task> _onSuccessAsync;
      private readonly Func<Context, Exception, Task> _onExceptionAsync;

      public AsyncTimingPolicy(
         string name,
         IMetricSink metricSink,
         IDictionary<string, object> policyDimensions,
         Func<Context, Task> onSuccessAsync,
         Func<Context, Exception, Task> onExceptionAsync)
      {
         _name = name ?? throw new ArgumentNullException(nameof(name));
         _metricSink = metricSink ?? throw new ArgumentNullException(nameof(metricSink));
         _policyDimensions = policyDimensions;
         _onSuccessAsync = onSuccessAsync;
         _onExceptionAsync = onExceptionAsync;
      }

      protected override Task<TResult> ImplementationAsync<TResult>(
         Func<Context, CancellationToken, Task<TResult>> action,
         Context context,
         CancellationToken cancellationToken,
         bool continueOnCapturedContext)
      {
         async Task OnSuccessAsync(Context c, TResult _) => await For.OptionalInvokeAsync(_onSuccessAsync, c);

         return AsyncTimingEngine.ImplementationAsync(action, context, cancellationToken, _name, _metricSink, _policyDimensions, OnSuccessAsync,
            _onExceptionAsync, continueOnCapturedContext);
      }
   }

   public class AsyncTimingPolicy<TResult> : AsyncPolicy<TResult>, ITimingPolicy<TResult>, IsPolicy
   {
      private readonly string _name;
      private readonly IMetricSink _metricSink;
      private readonly IDictionary<string, object> _policyDimensions;
      private readonly Func<Context, TResult, Task> _onSuccessAsync;
      private readonly Func<Context, Exception, Task> _onExceptionAsync;

      public AsyncTimingPolicy(
         string name,
         IMetricSink metricSink,
         IDictionary<string, object> policyDimensions,
         Func<Context, TResult, Task> onSuccessAsync,
         Func<Context, Exception, Task> onExceptionAsync)
      {
         _name = name ?? throw new ArgumentNullException(nameof(name));
         _metricSink = metricSink ?? throw new ArgumentNullException(nameof(metricSink));
         _policyDimensions = policyDimensions;
         _onSuccessAsync = onSuccessAsync;
         _onExceptionAsync = onExceptionAsync;
      }

      protected override Task<TResult> ImplementationAsync(
         Func<Context, CancellationToken, Task<TResult>> action,
         Context context,
         CancellationToken cancellationToken,
         bool continueOnCapturedContext)
      {
         return AsyncTimingEngine.ImplementationAsync(action, context, cancellationToken, _name, _metricSink, _policyDimensions, _onSuccessAsync,
            _onExceptionAsync, continueOnCapturedContext);
      }
   }
}
