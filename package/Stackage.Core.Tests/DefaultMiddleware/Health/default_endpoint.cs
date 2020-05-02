using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.Abstractions.Metrics;

namespace Stackage.Core.Tests.DefaultMiddleware.Health
{
   public class default_endpoint : health_scenario
   {
      private HttpResponseMessage _response;
      private string _content;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _response = await TestService.GetAsync("/health");
         _content = await _response.Content.ReadAsStringAsync();
      }

      [Test]
      public void should_return_status_code_200()
      {
         _response.StatusCode.ShouldBe(HttpStatusCode.OK);
      }

      [Test]
      public void should_return_content()
      {
         var response = JObject.Parse(_content);

         var expectedResponse = new JObject
         {
            ["status"] = "Healthy"
         };

         response.Should().ContainSubtree(expectedResponse);
      }

      [Test]
      public void should_return_content_type_json()
      {
         _response.Content.Headers.ContentType.MediaType.ShouldBe("application/json");
      }

      [Test]
      public void should_write_start_metric_with_path()
      {
         var metric = (Counter) MetricSink.Metrics.First();

         Assert.That(metric.Name, Is.EqualTo("http_request_start"));
         Assert.That(metric.Dimensions["path"], Is.EqualTo("/health"));
      }

      [Test]
      public void should_write_end_metric_with_path_and_status_200()
      {
         var metric = (Gauge) MetricSink.Metrics.Last();

         Assert.That(metric.Name, Is.EqualTo("http_request_end"));
         Assert.That(metric.Dimensions["path"], Is.EqualTo("/health"));
         Assert.That(metric.Dimensions["statusCode"], Is.EqualTo(200));
      }
   }
}
