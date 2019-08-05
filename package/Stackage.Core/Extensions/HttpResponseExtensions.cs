using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

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

      public static async Task WriteJsonAsync(this HttpResponse response, HttpStatusCode statusCode, object content)
      {
         response.StatusCode = (int) statusCode;
         response.ContentType = "application/json";

         await response.WriteAsync(JsonConvert.SerializeObject(content), Encoding.UTF8);
      }
   }
}
