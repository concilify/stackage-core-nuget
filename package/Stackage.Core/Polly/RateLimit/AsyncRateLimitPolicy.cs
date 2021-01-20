using System;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Stackage.Core.Abstractions.Polly.RateLimit;

namespace Stackage.Core.Polly.RateLimit
{
   public class AsyncRateLimitPolicy : AsyncPolicy, IRateLimitPolicy, IsPolicy
   {
      private readonly IRateLimiter _rateLimiter;
      private readonly Func<Context, Exception, Task>? _onRejectionAsync;

      public AsyncRateLimitPolicy(
         IRateLimiter rateLimiter,
         Func<Context, Exception, Task>? onRejectionAsync)
      {
         _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
         _onRejectionAsync = onRejectionAsync;
      }

      protected override Task<TResult> ImplementationAsync<TResult>(
         Func<Context, CancellationToken, Task<TResult>> action,
         Context context,
         CancellationToken cancellationToken,
         bool continueOnCapturedContext)
      {
         return AsyncRateLimitEngine.ImplementationAsync(action, context, cancellationToken, _rateLimiter, _onRejectionAsync, continueOnCapturedContext);
      }
   }

   public class AsyncRateLimitPolicy<TResult> : AsyncPolicy<TResult>, IRateLimitPolicy<TResult>, IsPolicy
   {
      private readonly IRateLimiter _rateLimiter;
      private readonly Func<Context, Exception, Task>? _onRejectionAsync;

      public AsyncRateLimitPolicy(
         IRateLimiter rateLimiter,
         Func<Context, Exception, Task>? onRejectionAsync)
      {
         _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
         _onRejectionAsync = onRejectionAsync;
      }

      protected override Task<TResult> ImplementationAsync(
         Func<Context, CancellationToken, Task<TResult>> action,
         Context context,
         CancellationToken cancellationToken,
         bool continueOnCapturedContext)
      {
         return AsyncRateLimitEngine.ImplementationAsync(action, context, cancellationToken, _rateLimiter, _onRejectionAsync, continueOnCapturedContext);
      }
   }
}
