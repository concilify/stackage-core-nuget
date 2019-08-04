using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Stackage.Core.Abstractions;
using Stackage.Core.Extensions;

namespace Stackage.Core.Middleware
{
   // ReSharper disable once ClassNeverInstantiated.Global
   public class ExceptionHandlingMiddleware
   {
      private readonly RequestDelegate _next;
      private readonly IGuidGenerator _guidGenerator;
      private readonly ILogger<ExceptionHandlingMiddleware> _logger;

      public ExceptionHandlingMiddleware(
         RequestDelegate next,
         IGuidGenerator guidGenerator,
         ILogger<ExceptionHandlingMiddleware> logger)
      {
         _next = next;
         _guidGenerator = guidGenerator;
         _logger = logger;
      }

      public async Task Invoke(HttpContext context)
      {
         try
         {
            await _next(context);
         }
         catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
         {
            await context.Response.WriteJsonAsync((HttpStatusCode) 499, new {message = "Client Closed Request"});
         }
         catch (Exception e)
         {
            var token = _guidGenerator.Generate().Substring(0, 8).ToUpper();

            _logger.LogError(e, "An unexpected exception has occurred (token={token})", token);

            await context.Response.WriteJsonAsync(HttpStatusCode.InternalServerError, new {message = "Internal Server Error", token});
         }
      }
   }
}
