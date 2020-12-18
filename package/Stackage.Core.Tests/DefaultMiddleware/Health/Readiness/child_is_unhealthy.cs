using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Extensions;

namespace Stackage.Core.Tests.DefaultMiddleware.Health.Readiness
{
   public class child_is_unhealthy : health_scenario
   {
      private HttpResponseMessage _response;
      private string _content;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _response = await TestService.GetAsync("/health/readiness");
         _content = await _response.Content.ReadAsStringAsync();
      }

      protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
      {
         base.ConfigureServices(services, configuration);

         services.AddHealthCheck("critical-unhealthy", new StubHealthCheck {CheckHealthResponse = new HealthCheckResult(HealthStatus.Unhealthy)});
      }

      [Test]
      public void should_return_status_code_503()
      {
         _response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
      }

      [Test]
      public void should_return_content_unhealthy()
      {
         _content.ShouldBe("Unhealthy");
      }

      [Test]
      public void should_return_content_type_text_plain()
      {
         _response.Content.Headers.ContentType.MediaType.ShouldBe("text/plain");
      }

      [Test]
      public void should_write_end_metric_with_status_503()
      {
         var metric = (Gauge) MetricSink.Metrics.Last();

         Assert.That(metric.Dimensions["statusCode"], Is.EqualTo(503));
      }
   }
}
