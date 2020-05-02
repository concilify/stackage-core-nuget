using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Stackage.Core.Extensions;
using Stackage.Core.Middleware.Options;

namespace Stackage.Core.Middleware
{
   // ReSharper disable once ClassNeverInstantiated.Global
   public class PingMiddleware
   {
      private readonly RequestDelegate _next;
      private readonly string _pingEndpoint;

      public PingMiddleware(RequestDelegate next, IOptions<PingOptions> options)
      {
         if (options == null) throw new ArgumentNullException(nameof(options));

         _next = next ?? throw new ArgumentNullException(nameof(next));

         _pingEndpoint = options.Value.Endpoint;
      }

      public async Task Invoke(HttpContext context)
      {
         if (!context.Request.Path.StartsWithSegments(_pingEndpoint, out var remainder) || remainder.HasValue)
         {
            await _next(context);
            return;
         }

         await context.Response.WriteTextAsync((HttpStatusCode) 200, "Healthy");
      }
   }
}
