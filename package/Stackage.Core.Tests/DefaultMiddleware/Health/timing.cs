using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.Extensions;

namespace Stackage.Core.Tests.DefaultMiddleware.Health
{
   public class timing : middleware_scenario
   {
      private HttpResponseMessage _response;
      private string _content;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _response = await TestService.GetAsync("/health");
         _content = await _response.Content.ReadAsStringAsync();
      }

      protected override void ConfigureServices(IServiceCollection services)
      {
         base.ConfigureServices(services);

         services.AddHealthCheck("quick",
            new StubHealthCheck {CheckHealthResponse = new HealthCheckResult(HealthStatus.Healthy), Latency = TimeSpan.FromMilliseconds(10)});
         services.AddHealthCheck("slower",
            new StubHealthCheck {CheckHealthResponse = new HealthCheckResult(HealthStatus.Healthy), Latency = TimeSpan.FromMilliseconds(100)});
      }

      [Test]
      public void should_return_content()
      {
         var response = JObject.Parse(_content);

         response["durationMs"].Value<int>().ShouldBeInRange(50, 150);
         response["dependencies"][0]["durationMs"].Value<int>().ShouldBeInRange(0, 30);
         response["dependencies"][1]["durationMs"].Value<int>().ShouldBeInRange(50, 150);
      }
   }
}
