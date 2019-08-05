using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Stackage.Core.Extensions;

namespace Stackage.Core.Middleware
{
   // ReSharper disable once ClassNeverInstantiated.Global
   public class PingMiddleware
   {
      private readonly RequestDelegate _next;

      public PingMiddleware(RequestDelegate next)
      {
         _next = next ?? throw new ArgumentNullException(nameof(next));
      }

      public async Task Invoke(HttpContext context)
      {
         if (context.Request.Path.StartsWithSegments("/ping", out var remainder) && !remainder.HasValue)
         {
            await context.Response.WriteTextAsync((HttpStatusCode) 200, "Healthy");
         }
         else
         {
            await _next(context);
         }
      }
   }
}
