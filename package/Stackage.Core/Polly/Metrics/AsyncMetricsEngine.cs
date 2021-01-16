using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Stackage.Core.Abstractions.Metrics;

namespace Stackage.Core.Polly.Metrics
{
   public static class AsyncMetricsEngine
   {
      public static async Task<TResult> ImplementationAsync<TResult>(
         Func<Context, CancellationToken, Task<TResult>> action,
         Context context,
         CancellationToken cancellationToken,
         string name,
         IMetricSink metricSink,
         Func<Context, TResult, Task> onSuccessAsync,
         Func<Context, Exception, Task> onExceptionAsync,
         bool continueOnCapturedContext)
      {
         cancellationToken.ThrowIfCancellationRequested();

         await metricSink.PushAsync(new Counter
         {
            Name = $"{name}_start",
            Dimensions = context.ToDictionary(c => c.Key, c => c.Value)
         });

         var stopwatch = Stopwatch.StartNew();

         try
         {
            var result = await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);

            stopwatch.Stop();
            await Invoke.NullableAsync(onSuccessAsync, context, result);

            return result;
         }
         catch (Exception e)
         {
            stopwatch.Stop();
            await Invoke.NullableAsync(onExceptionAsync, context, e);

            throw;
         }
         finally
         {
            await metricSink.PushAsync(new Gauge
            {
               Name = $"{name}_end",
               Dimensions = context.ToDictionary(c => c.Key, c => c.Value),
               Value = stopwatch.ElapsedMilliseconds
            });
         }
      }
   }
}
