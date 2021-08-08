using System;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Stackage.Core.Abstractions.Polly.RateLimiting;
using Stackage.Core.Abstractions.RateLimiting;

namespace Stackage.Core.Polly.RateLimiting
{
   public static class AsyncRateLimitingEngine
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
         catch (RateLimitExceededException e)
         {
            await Invoke.NullableAsync(onRejectionAsync, context, e).ConfigureAwait(continueOnCapturedContext);

            throw new RateLimitRejectionException("Request failed due to rate limit", e);
         }

         return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
      }
   }
}
