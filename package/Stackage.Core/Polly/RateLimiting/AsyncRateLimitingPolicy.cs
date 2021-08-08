using System;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Stackage.Core.Abstractions.Polly.RateLimiting;
using Stackage.Core.Abstractions.RateLimiting;

namespace Stackage.Core.Polly.RateLimiting
{
   public class AsyncRateLimitingPolicy : AsyncPolicy, IRateLimitingPolicy, IsPolicy
   {
      private readonly IRateLimiter _rateLimiter;
      private readonly Func<Context, Exception, Task>? _onRejectionAsync;

      public AsyncRateLimitingPolicy(
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
         return AsyncRateLimitingEngine.ImplementationAsync(action, context, cancellationToken, _rateLimiter, _onRejectionAsync, continueOnCapturedContext);
      }
   }

   public class AsyncRateLimitingPolicy<TResult> : AsyncPolicy<TResult>, IRateLimitingPolicy<TResult>, IsPolicy
   {
      private readonly IRateLimiter _rateLimiter;
      private readonly Func<Context, Exception, Task>? _onRejectionAsync;

      public AsyncRateLimitingPolicy(
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
         return AsyncRateLimitingEngine.ImplementationAsync(action, context, cancellationToken, _rateLimiter, _onRejectionAsync, continueOnCapturedContext);
      }
   }
}
