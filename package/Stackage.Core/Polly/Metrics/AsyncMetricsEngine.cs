using System;
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
         ITimerFactory timerFactory,
         Func<Context, TResult, Task>? onSuccessAsync,
         Func<Context, Exception, Task>? onExceptionAsync,
         bool continueOnCapturedContext)
      {
         cancellationToken.ThrowIfCancellationRequested();

         await metricSink.PushAsync(new Counter($"{name}_start")
         {
            Dimensions = context.ToDictionary(c => c.Key, c => c.Value)
         });

         var timer = timerFactory.CreateAndStart();

         try
         {
            var result = await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);

            timer.Stop();
            await Invoke.NullableAsync(onSuccessAsync, context, result);

            return result;
         }
         catch (Exception e)
         {
            timer.Stop();
            await Invoke.NullableAsync(onExceptionAsync, context, e);

            throw;
         }
         finally
         {
            await metricSink.PushAsync(new Gauge($"{name}_end")
            {
               Dimensions = context.ToDictionary(c => c.Key, c => c.Value),
               Value = timer.ElapsedMilliseconds
            });
         }
      }
   }
}
