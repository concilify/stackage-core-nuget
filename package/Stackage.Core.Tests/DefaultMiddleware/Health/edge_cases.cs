using System;
using System.Collections.Generic;
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
using Stackage.Core.Extensions;

namespace Stackage.Core.Tests.DefaultMiddleware.Health
{
   public class edge_cases : health_scenario
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

         services.AddHealthCheck("with-description",
            new StubHealthCheck {CheckHealthResponse = new HealthCheckResult(HealthStatus.Healthy, description: "Some description")});
         services.AddHealthCheck("null-description",
            new StubHealthCheck {CheckHealthResponse = new HealthCheckResult(HealthStatus.Healthy, description: null)});
         services.AddHealthCheck("empty-description",
            new StubHealthCheck {CheckHealthResponse = new HealthCheckResult(HealthStatus.Healthy, description: string.Empty)});
         services.AddHealthCheck("with-exception",
            new StubHealthCheck {CheckHealthResponse = new HealthCheckResult(HealthStatus.Healthy, exception: new InvalidOperationException())});
         services.AddHealthCheck("empty-data",
            new StubHealthCheck {CheckHealthResponse = new HealthCheckResult(HealthStatus.Healthy, data: new Dictionary<string, object>())});
         services.AddHealthCheck("with-data",
            new StubHealthCheck
               {CheckHealthResponse = new HealthCheckResult(HealthStatus.Healthy, data: new Dictionary<string, object> {{"string", "value"}, {"int", 23}})});
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
            ["status"] = "Healthy",
            ["dependencies"] = new JArray
            {
               new JObject
               {
                  ["name"] = "with-description",
                  ["status"] = "Healthy",
                  ["description"] = "Some description"
               },
               new JObject
               {
                  ["name"] = "null-description",
                  ["status"] = "Healthy"
               },
               new JObject
               {
                  ["name"] = "empty-description",
                  ["status"] = "Healthy"
               },
               new JObject
               {
                  ["name"] = "with-exception",
                  ["status"] = "Healthy",
                  ["exception"] = "System.InvalidOperationException"
               },
               new JObject
               {
                  ["name"] = "empty-data",
                  ["status"] = "Healthy"
               },
               new JObject
               {
                  ["name"] = "with-data",
                  ["status"] = "Healthy",
                  ["data"] = new JObject
                  {
                     ["string"] = "value",
                     ["int"] = 23
                  }
               }
            }
         };

         response.Should().ContainSubtree(expectedResponse);
      }
   }
}
