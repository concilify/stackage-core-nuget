using System;
using System.Net;

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

      public HttpStatusCode? StatusCode { get; set; }

      public string Content { get; set; }

      public Exception ThrowException { get; set; }

      public TimeSpan? Latency { get; set; }
   }
}
