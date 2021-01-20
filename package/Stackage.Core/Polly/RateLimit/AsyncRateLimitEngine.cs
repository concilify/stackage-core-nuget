using System;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Stackage.Core.Abstractions.Polly.RateLimit;

namespace Stackage.Core.Polly.RateLimit
{
   public static class AsyncRateLimitEngine
   {
      public static async Task<TResult> ImplementationAsync<TResult>(
         Func<Context, CancellationToken, Task<TResult>> action,
         Context context,
         CancellationToken cancellationToken,
         IRateLimiter rateLimiter,
         Func<Context, Exception, Task>? onRejectionAsync,
         bool continueOnCapturedContext)
      {
         cancellationToken.ThrowIfCancellationRequested();

         try
         {
            await rateLimiter.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext);
         }
         catch (RateLimitRejectionException e)
         {
            await Invoke.NullableAsync(onRejectionAsync, context, e).ConfigureAwait(continueOnCapturedContext);

            throw;
         }

         return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
      }
   }
}
