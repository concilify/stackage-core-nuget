using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Stackage.Core.Abstractions.Metrics;

namespace Stackage.Core.Polly.Timing
{
   public static class AsyncTimingEngine
   {
      public static async Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action,
         Context context,
         CancellationToken cancellationToken,
         string name,
         IMetricSink metricSink,
         IDictionary<string, object> policyDimensions,
         Func<Context, TResult, Task> onSuccessAsync,
         Func<Context, Exception, Task> onExceptionAsync,
         bool continueOnCapturedContext)
      {
         cancellationToken.ThrowIfCancellationRequested();

         await metricSink.PushAsync(new Counter
         {
            Name = $"{name}_start",
            Dimensions = MergeDimensions(policyDimensions, context)
         });

         var stopwatch = Stopwatch.StartNew();

         try
         {
            var result = await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);

            stopwatch.Stop();
            await For.OptionalInvokeAsync(onSuccessAsync, context, result);

            return result;
         }
         catch (Exception e)
         {
            stopwatch.Stop();
            await For.OptionalInvokeAsync(onExceptionAsync, context, e);

            throw;
         }
         finally
         {
            await metricSink.PushAsync(new Gauge
            {
               Name = $"{name}_end",
               Dimensions = MergeDimensions(policyDimensions, context),
               Value = stopwatch.ElapsedMilliseconds
            });
         }
      }

      private static IDictionary<string, object> MergeDimensions(IDictionary<string, object> policyDimensions, Context context)
      {
         if (policyDimensions == null)
         {
            return context.ToDictionary(x => x.Key, x => x.Value);
         }

         return policyDimensions
            .Concat(context)
            .ToDictionary(x => x.Key, x => x.Value);
      }
   }
}
