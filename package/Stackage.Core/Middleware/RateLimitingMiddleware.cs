using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Polly;
using Stackage.Core.Abstractions.Polly;
using Stackage.Core.Abstractions.Polly.RateLimit;
using Stackage.Core.Extensions;
using Stackage.Core.Middleware.Options;
using Stackage.Core.Polly.RateLimit;

namespace Stackage.Core.Middleware
{
   // ReSharper disable once ClassNeverInstantiated.Global
   public class RateLimitingMiddleware
   {
      private readonly RequestDelegate _next;
      private readonly Func<HttpContext, Task> _invokeDelegateAsync;

      public RateLimitingMiddleware(
         RequestDelegate next,
         IOptions<RateLimitingOptions> options,
         IPolicyFactory policyFactory)
      {
         if (options == null) throw new ArgumentNullException(nameof(options));
         if (policyFactory == null) throw new ArgumentNullException(nameof(policyFactory));

         _next = next ?? throw new ArgumentNullException(nameof(next));

         var rateLimitingOptions = options.Value;

         if (rateLimitingOptions.Enabled)
         {
            var rateLimiter = CreateRateLimiter(rateLimitingOptions);
            var rateLimitPolicy = policyFactory.CreateAsyncRateLimitPolicy(rateLimiter);

            _invokeDelegateAsync = (context) => InvokeWithRateLimiting(context, rateLimitPolicy);
         }
         else
         {
            _invokeDelegateAsync = (context) => _next(context);
         }
      }

      private static RateLimiter CreateRateLimiter(RateLimitingOptions options)
      {
         var rateLimiter = new RateLimiter(
            options.RequestsPerPeriod,
            TimeSpan.FromSeconds(options.PeriodSeconds),
            options.BurstSize,
            TimeSpan.FromMilliseconds(options.MaxWaitMs));
         return rateLimiter;
      }

      public async Task Invoke(HttpContext context)
      {
         await _invokeDelegateAsync(context);
      }

      private async Task InvokeWithRateLimiting(HttpContext context, IAsyncPolicy rateLimitPolicy)
      {
         try
         {
            await rateLimitPolicy.ExecuteAsync(() => _next(context));
         }
         catch (RateLimitRejectionException)
         {
            await context.Response.WriteJsonAsync((HttpStatusCode) 429, new {message = "Too Many Requests"});
         }
      }
   }
}
