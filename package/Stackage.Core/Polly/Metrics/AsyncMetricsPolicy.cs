using System;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Abstractions.Polly.Metrics;

namespace Stackage.Core.Polly.Metrics
{
   public class AsyncMetricsPolicy : AsyncPolicy, IMetricsPolicy, IsPolicy
   {
      private readonly string _name;
      private readonly IMetricSink _metricSink;
      private readonly Func<Context, Task>? _onSuccessAsync;
      private readonly Func<Context, Exception, Task>? _onExceptionAsync;

      public AsyncMetricsPolicy(
         string name,
         IMetricSink metricSink,
         Func<Context, Task>? onSuccessAsync,
         Func<Context, Exception, Task>? onExceptionAsync)
      {
         _name = name ?? throw new ArgumentNullException(nameof(name));
         _metricSink = metricSink ?? throw new ArgumentNullException(nameof(metricSink));
         _onSuccessAsync = onSuccessAsync;
         _onExceptionAsync = onExceptionAsync;
      }

      protected override Task<TResult> ImplementationAsync<TResult>(
         Func<Context, CancellationToken, Task<TResult>> action,
         Context context,
         CancellationToken cancellationToken,
         bool continueOnCapturedContext)
      {
         async Task OnSuccessAsync(Context c, TResult _) => await Invoke.NullableAsync(_onSuccessAsync, c);

         return AsyncMetricsEngine.ImplementationAsync(
            action, context, cancellationToken, _name, _metricSink, OnSuccessAsync, _onExceptionAsync, continueOnCapturedContext);
      }
   }

   public class AsyncMetricsPolicy<TResult> : AsyncPolicy<TResult>, IMetricsPolicy<TResult>, IsPolicy
   {
      private readonly string _name;
      private readonly IMetricSink _metricSink;
      private readonly Func<Context, TResult, Task>? _onSuccessAsync;
      private readonly Func<Context, Exception, Task>? _onExceptionAsync;

      public AsyncMetricsPolicy(
         string name,
         IMetricSink metricSink,
         Func<Context, TResult, Task>? onSuccessAsync,
         Func<Context, Exception, Task>? onExceptionAsync)
      {
         _name = name ?? throw new ArgumentNullException(nameof(name));
         _metricSink = metricSink ?? throw new ArgumentNullException(nameof(metricSink));
         _onSuccessAsync = onSuccessAsync;
         _onExceptionAsync = onExceptionAsync;
      }

      protected override Task<TResult> ImplementationAsync(
         Func<Context, CancellationToken, Task<TResult>> action,
         Context context,
         CancellationToken cancellationToken,
         bool continueOnCapturedContext)
      {
         return AsyncMetricsEngine.ImplementationAsync(
            action, context, cancellationToken, _name, _metricSink, _onSuccessAsync, _onExceptionAsync, continueOnCapturedContext);
      }
   }
}
