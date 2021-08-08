using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Metrics;

namespace Stackage.Core.Tests.DefaultMiddleware.Prometheus
{
   public class unmatched_label : middleware_scenario
   {
      private HttpResponseMessage _response;
      private string _content;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         using (var server = TestService.CreateServer())
         {
            await TestService.GetAsync(server, "/unmatched");

            await Task.Delay(100);

            _response = await TestService.GetAsync(server, "/metrics");
            _content = await _response.Content.ReadAsStringAsync();
         }
      }

      protected override void ConfigureConfiguration(IConfigurationBuilder configurationBuilder)
      {
         base.ConfigureConfiguration(configurationBuilder);

         configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
         {
            {"STACKAGE:PROMETHEUS:METRICS:0:NAME", "http_request_start"},
            {"STACKAGE:PROMETHEUS:METRICS:0:TYPE", "Counter"},
            {"STACKAGE:PROMETHEUS:METRICS:0:DESCRIPTION", "HTTP Server Requests (Count)"},
            {"STACKAGE:PROMETHEUS:METRICS:0:LABELS:0", "method"},
            {"STACKAGE:PROMETHEUS:METRICS:0:LABELS:1", "path"},
            {"STACKAGE:PROMETHEUS:METRICS:0:SANITISERS:0:LABEL", "path"},
            {"STACKAGE:PROMETHEUS:METRICS:0:SANITISERS:0:LITERAL", "/replace-this"},
            {"STACKAGE:PROMETHEUS:METRICS:0:SANITISERS:0:VALUE", "with-this"},
            {"STACKAGE:PROMETHEUS:METRICS:0:SANITISERS:1:LABEL", "path"},
            {"STACKAGE:PROMETHEUS:METRICS:0:SANITISERS:1:LITERAL", "/metrics"},
            {"STACKAGE:PROMETHEUS:METRICS:0:SANITISERS:1:VALUE", "/metrics"},
            {"STACKAGE:PROMETHEUS:SANITISERFALLBACK", "sanitiser-fallback"},
         });
      }

      protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
      {
         base.ConfigureServices(services, configuration);

         services.AddSingleton<IMetricSink>(sp => sp.GetRequiredService<PrometheusMetricSink>());
      }

      protected override void Configure(IApplicationBuilder app)
      {
         base.Configure(app);

         app.UseMiddleware<StubResponseMiddleware>(new StubResponseOptions(HttpStatusCode.OK, "content"));
      }

      [Test]
      public void should_return_status_code_200()
      {
         _response.StatusCode.ShouldBe(HttpStatusCode.OK);
      }

      [Test]
      public void should_return_start_metric()
      {
         var lines = _content.Split('\n');

         lines.ShouldContain("# HELP http_request_start HTTP Server Requests (Count)");
         lines.ShouldContain("# TYPE http_request_start counter");
         lines.ShouldContain("http_request_start{method=\"GET\",path=\"sanitiser-fallback\"} 1");
      }

      [Test]
      public void should_return_content_plain_text()
      {
         _response.Content.Headers.ContentType.MediaType.ShouldBe("text/plain");
      }
   }
}
