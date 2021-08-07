using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Extensions;
using Stackage.Core.Polly.Metrics;

namespace Stackage.Core.Tests.DefaultMiddleware.Health
{
   public class children_with_latency : health_scenario
   {
      private const int MiddlewareDurationMs = 19;
      private const int OverallTimerDurationMs = 23;
      private const int Task1TimerDurationMs = 29;
      private const int Task2TimerDurationMs = 31;

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

         services.AddSingleton<ITimerFactory>(new StubTimerFactory(MiddlewareDurationMs, OverallTimerDurationMs, Task1TimerDurationMs, Task2TimerDurationMs));

         services.AddHealthCheck("quick",
            new StubHealthCheck {CheckHealthResponse = new HealthCheckResult(HealthStatus.Healthy)});
         services.AddHealthCheck("slower",
            new StubHealthCheck {CheckHealthResponse = new HealthCheckResult(HealthStatus.Healthy)});
      }

      [Test]
      public void should_return_content()
      {
         var response = JObject.Parse(_content);

         response["durationMs"].Value<int>().ShouldBe(OverallTimerDurationMs);
         response["dependencies"][0]["durationMs"].Value<int>().ShouldBe(Task1TimerDurationMs);
         response["dependencies"][1]["durationMs"].Value<int>().ShouldBe(Task2TimerDurationMs);
      }

      [Test]
      public void should_write_end_metric_with_duration_similar_to_timeout()
      {
         var metric = (Gauge) MetricSink.Metrics.Last();

         Assert.That(metric.Value, Is.EqualTo(MiddlewareDurationMs));
      }
   }
}
