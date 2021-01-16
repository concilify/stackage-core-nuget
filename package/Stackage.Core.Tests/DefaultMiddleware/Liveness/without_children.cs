using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace Stackage.Core.Tests.DefaultMiddleware.Liveness
{
   public class without_children : middleware_scenario
   {
      private HttpResponseMessage _response;
      private string _content;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _response = await TestService.GetAsync("/health/liveness");
         _content = await _response.Content.ReadAsStringAsync();
      }

      [Test]
      public void should_return_status_code_200()
      {
         _response.StatusCode.ShouldBe(HttpStatusCode.OK);
      }

      [Test]
      public void should_return_content_healthy()
      {
         _content.ShouldBe("Healthy");
      }

      [Test]
      public void should_return_content_type_text_plain()
      {
         _response.Content.Headers.ContentType.MediaType.ShouldBe("text/plain");
      }

      [Test]
      public void should_disable_caching()
      {
         _response.Headers.Pragma.ToString().ShouldBe("no-cache");
         _response.Headers.CacheControl.ToString().ShouldBe("no-store, no-cache");
      }
   }
}
