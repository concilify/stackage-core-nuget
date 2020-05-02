using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Stackage.Core.Tests
{
   public class StubResponseOptions
   {
      public StubResponseOptions()
      {
      }

      public StubResponseOptions(HttpStatusCode statusCode, string content)
      {
         StatusCode = statusCode;
         Content = content;
      }

      public Func<HttpContext, Task> Handler { get; set; }

      public HttpStatusCode? StatusCode { get; set; }

      public string Content { get; set; }

      public Exception ThrowException { get; set; }

      public TimeSpan? Latency { get; set; }
   }
}
