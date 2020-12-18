using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.Abstractions.Metrics;

namespace Stackage.Core.Tests.DefaultMiddleware.Health.Readiness
{
   public class without_children : health_scenario
   {
      private HttpResponseMessage _response;
      private string _content;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _response = await TestService.GetAsync("/health/readiness");
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

      [Test]
      public void should_write_end_metric_with_status_200()
      {
         var metric = (Gauge) MetricSink.Metrics.Last();

         Assert.That(metric.Dimensions["statusCode"], Is.EqualTo(200));
      }
   }
}
