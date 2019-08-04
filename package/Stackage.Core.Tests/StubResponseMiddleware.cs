using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Stackage.Core.Tests
{
   public class StubResponseMiddleware
   {
      private readonly StubResponseOptions _options;

      public StubResponseMiddleware(
         RequestDelegate _,
         StubResponseOptions options)
      {
         _options = options ?? throw new ArgumentNullException(nameof(options));
      }

      public async Task Invoke(HttpContext context)
      {
         if (_options.Latency != null)
         {
            await Task.Delay(_options.Latency.Value, context.RequestAborted);
         }

         if (_options.ThrowException != null)
         {
            throw _options.ThrowException;
         }

         if (_options.StatusCode != null)
         {
            context.Response.StatusCode = (int) _options.StatusCode;
         }

         if (_options.Content != null)
         {
            await context.Response.WriteAsync(_options.Content, Encoding.UTF8);
         }
      }
   }
}
