using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Stackage.Core.Abstractions;

namespace Stackage.Core.Extensions
{
   public static class HttpResponseExtensions
   {
      public static async Task WriteTextAsync(this HttpResponse response, HttpStatusCode statusCode, string content)
      {
         response.StatusCode = (int) statusCode;
         response.ContentType = "text/plain";

         await response.WriteAsync(content, Encoding.UTF8);
      }

      public static async Task WriteJsonAsync(this HttpResponse response, HttpStatusCode statusCode, object content, IJsonSerialiser jsonSerialiser)
      {
         response.StatusCode = (int) statusCode;
         response.ContentType = "application/json";

         await response.WriteAsync(jsonSerialiser.Serialise(content), Encoding.UTF8);
      }

      public static async Task WriteServiceUnavailableAsync(this HttpResponse response)
      {
         response.Headers["Retry-After"] = "30";

         await response.WriteTextAsync(HttpStatusCode.ServiceUnavailable, "Service Unavailable");
      }

      public static void AddNoCacheHeaders(this HttpResponse response)
      {
         var headers = response.Headers;

         // Similar to: https://github.com/aspnet/Security/blob/7b6c9cf0eeb149f2142dedd55a17430e7831ea99/src/Microsoft.AspNetCore.Authentication.Cookies/CookieAuthenticationHandler.cs#L377-L379
         headers[HeaderNames.CacheControl] = "no-store, no-cache";
         headers[HeaderNames.Pragma] = "no-cache";
         headers[HeaderNames.Expires] = "Thu, 01 Jan 1970 00:00:00 GMT";
      }
   }
}
