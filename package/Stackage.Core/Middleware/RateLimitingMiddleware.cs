using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Stackage.Core.Abstractions;
using Stackage.Core.Abstractions.Polly;
using Stackage.Core.Abstractions.Polly.RateLimit;
using Stackage.Core.Extensions;
using Stackage.Core.Options;
using Stackage.Core.Polly.RateLimit;

namespace Stackage.Core.Middleware
{
   // ReSharper disable once ClassNeverInstantiated.Global
   public class RateLimitingMiddleware
   {
      private readonly RequestDelegate _next;
      private readonly RateLimiter? _rateLimiter;

      public RateLimitingMiddleware(
         RequestDelegate next,
         IOptions<RateLimitingOptions> options)
      {
         if (options == null) throw new ArgumentNullException(nameof(options));

         _next = next ?? throw new ArgumentNullException(nameof(next));

         var rateLimitingOptions = options.Value;

         _rateLimiter = rateLimitingOptions.Enabled ? CreateRateLimiter(rateLimitingOptions) : null;
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

      public async Task Invoke(
         HttpContext context,
         IPolicyFactory policyFactory,
         IJsonSerialiser jsonSerialiser)
      {
         if (_rateLimiter != null)
         {
            await InvokeWithRateLimitingAsync(context, policyFactory, jsonSerialiser);
         }
         else
         {
            await _next(context);
         }
      }

      private async Task InvokeWithRateLimitingAsync(HttpContext context, IPolicyFactory policyFactory, IJsonSerialiser jsonSerialiser)
      {
         var rateLimitPolicy = policyFactory.CreateAsyncRateLimitPolicy(_rateLimiter!);

         try
         {
            await rateLimitPolicy.ExecuteAsync(() => _next(context));
         }
         catch (RateLimitRejectionException)
         {
            await context.Response.WriteJsonAsync((HttpStatusCode) 429, new {message = "Too Many Requests"}, jsonSerialiser);
         }
      }
   }
}
