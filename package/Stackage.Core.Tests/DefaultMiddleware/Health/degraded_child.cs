using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Extensions;

namespace Stackage.Core.Tests.DefaultMiddleware.Health
{
   public class degraded_child : health_scenario
   {
      private HttpResponseMessage _response;
      private string _content;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _response = await TestService.GetAsync("/health");
         _content = await _response.Content.ReadAsStringAsync();
      }

      protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
      {
         base.ConfigureServices(services, configuration);

         services.AddHealthCheck("critical-degraded", new StubHealthCheck {CheckHealthResponse = new HealthCheckResult(HealthStatus.Degraded)});
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
            ["status"] = "Degraded",
            ["dependencies"] = new JArray
            {
               new JObject
               {
                  ["name"] = "critical-degraded",
                  ["status"] = "Degraded"
               }
            }
         };

         response.Should().ContainSubtree(expectedResponse);
      }

      [Test]
      public void should_return_content_type_json()
      {
         _response.Content.Headers.ContentType.MediaType.ShouldBe("application/json");
      }

      [Test]
      public void should_write_end_metric_with_status_200()
      {
         var metric = (Gauge) MetricSink.Metrics.Last();

         Assert.That(metric.Dimensions["statusCode"], Is.EqualTo(200));
      }
   }
}
