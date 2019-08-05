using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Polly;
using Stackage.Core.Abstractions.Polly;
using Stackage.Core.Abstractions.Polly.RateLimit;
using Stackage.Core.Extensions;
using Stackage.Core.Polly.RateLimit;

namespace Stackage.Core.Middleware
{
   // ReSharper disable once ClassNeverInstantiated.Global
   public class RateLimitingMiddleware
   {
      private readonly RequestDelegate _next;
      private readonly Func<HttpContext, Task> _implementation;

      public RateLimitingMiddleware(
         RequestDelegate next,
         IPolicyFactory policyFactory,
         IConfiguration configuration)
      {
         if (policyFactory == null) throw new ArgumentNullException(nameof(policyFactory));
         if (configuration == null) throw new ArgumentNullException(nameof(configuration));

         _next = next ?? throw new ArgumentNullException(nameof(next));

         if (configuration.GetSection("RATELIMITING").Exists())
         {
            var rateLimiter = CreateRateLimiter(configuration);
            var rateLimitPolicy = policyFactory.CreateAsyncRateLimitPolicy(rateLimiter);

            _implementation = (context) => InvokeWithRateLimiting(context, rateLimitPolicy);
         }
         else
         {
            _implementation = InvokeWithoutRateLimiting;
         }
      }

      private static RateLimiter CreateRateLimiter(IConfiguration configuration)
      {
         var rlc = new RateLimitConfiguration();
         configuration.Bind("RATELIMITING", rlc);

         var rateLimiter = new RateLimiter(
            rlc.RequestsPerPeriod,
            TimeSpan.FromSeconds(rlc.PeriodSeconds),
            rlc.BurstSize,
            TimeSpan.FromMilliseconds(rlc.MaxWaitMs));
         return rateLimiter;
      }

      public async Task Invoke(HttpContext context)
      {
         await _implementation(context);
      }

      private async Task InvokeWithoutRateLimiting(HttpContext context)
      {
         await _next(context);
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

      private class RateLimitConfiguration
      {
         public int RequestsPerPeriod { get; set; }
         public double PeriodSeconds { get; set; }
         public int BurstSize { get; set; }
         public int MaxWaitMs { get; set; } = 3000;
      }
   }
}
