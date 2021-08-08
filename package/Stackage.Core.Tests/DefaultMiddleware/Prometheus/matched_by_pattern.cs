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
   public class matched_by_pattern : middleware_scenario
   {
      private HttpResponseMessage _response;
      private string _content;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         using (var server = TestService.CreateServer())
         {
            await TestService.GetAsync(server, "/tenant-a/bar/get");
            await TestService.GetAsync(server, "/tenant-b/bar/get");

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
            {"STACKAGE:PROMETHEUS:METRICS:0:SANITISERS:0:PATTERN", @"^\/.*?\/bar\/get$"},
            {"STACKAGE:PROMETHEUS:METRICS:0:SANITISERS:0:VALUE", "/{tenant}/bar/get"},
            {"STACKAGE:PROMETHEUS:METRICS:1:NAME", "http_request_end"},
            {"STACKAGE:PROMETHEUS:METRICS:1:TYPE", "Histogram"},
            {"STACKAGE:PROMETHEUS:METRICS:1:DESCRIPTION", "HTTP Server Requests (Duration ms)"},
            {"STACKAGE:PROMETHEUS:METRICS:1:LABELS:0", "method"},
            {"STACKAGE:PROMETHEUS:METRICS:1:LABELS:1", "path"},
            {"STACKAGE:PROMETHEUS:METRICS:1:LABELS:2", "statusCode"},
            {"STACKAGE:PROMETHEUS:METRICS:1:SANITISERS:0:LABEL", "path"},
            {"STACKAGE:PROMETHEUS:METRICS:1:SANITISERS:0:PATTERN", @"^\/.*?\/bar\/get$"},
            {"STACKAGE:PROMETHEUS:METRICS:1:SANITISERS:0:VALUE", "/{tenant}/bar/get"},
            {"STACKAGE:PROMETHEUS:METRICS:1:BUCKETS:0", "10000"},
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
         lines.ShouldContain("http_request_start{method=\"GET\",path=\"/{tenant}/bar/get\"} 2");
      }

      [Test]
      public void should_return_end_metric()
      {
         var lines = _content.Split('\n');

         lines.ShouldContain("# HELP http_request_end HTTP Server Requests (Duration ms)");
         lines.ShouldContain("# TYPE http_request_end histogram");
         lines.ShouldContain("http_request_end_bucket{method=\"GET\",path=\"/{tenant}/bar/get\",statusCode=\"200\",le=\"10000\"} 2");
      }

      [Test]
      public void should_return_content_plain_text()
      {
         _response.Content.Headers.ContentType.MediaType.ShouldBe("text/plain");
      }
   }
}
