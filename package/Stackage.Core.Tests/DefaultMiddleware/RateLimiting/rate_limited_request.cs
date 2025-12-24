using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.Abstractions.Metrics;

namespace Stackage.Core.Tests.DefaultMiddleware.RateLimiting
{
   public class rate_limited_request : middleware_scenario
   {
      private HttpResponseMessage _fooResponse;
      private HttpResponseMessage _barResponse;
      private string _fooContent;
      private string _barContent;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         using (var server = TestService.CreateServer())
         {
            var foo = TestService.GetAsync(server, "/foo");
            await Task.Delay(200);
            var bar = TestService.GetAsync(server, "/bar");

            await Task.WhenAll(foo, bar);

            _fooResponse = foo.Result;
            _barResponse = bar.Result;

            _fooContent = await _fooResponse.Content.ReadAsStringAsync();
            _barContent = await _barResponse.Content.ReadAsStringAsync();
         }
      }

      protected override void ConfigureConfiguration(IConfigurationBuilder configurationBuilder)
      {
         base.ConfigureConfiguration(configurationBuilder);

         configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
         {
            {"STACKAGE:RATELIMITING:ENABLED", "true"},
            {"STACKAGE:RATELIMITING:REQUESTSPERPERIOD", "1"},
            {"STACKAGE:RATELIMITING:PERIODSECONDS", "1"},
            {"STACKAGE:RATELIMITING:BURSTSIZE", "1"},
            {"STACKAGE:RATELIMITING:MAXWAITMS", "10"}
         });
      }

      protected override void Configure(IApplicationBuilder app)
      {
         base.Configure(app);

         app.UseMiddleware<StubResponseMiddleware>(new StubResponseOptions(HttpStatusCode.OK, "content") {Latency = TimeSpan.FromMilliseconds(50)});
      }

      [Test]
      public void foo_request_should_return_200()
      {
         _fooResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
      }

      [Test]
      public void bar_request_should_return_429()
      {
         _barResponse.StatusCode.ShouldBe((HttpStatusCode) 429);
      }

      [Test]
      public void foo_request_should_return_content()
      {
         _fooContent.ShouldBe("content");
      }

      [Test]
      public void bar_request_should_return_json_error_content()
      {
         _barContent.ShouldBe("{\"message\":\"Too Many Requests\"}");
      }

      [Test]
      public void should_return_content_type_json()
      {
         _barResponse.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
      }

      [Test]
      public void should_not_log_a_message()
      {
         Logger.Entries.Count.ShouldBe(0);
      }

      [Test]
      public void should_write_four_metrics()
      {
         Assert.That(MetricSink.Metrics.Count, Is.EqualTo(4));
      }

      [Test]
      public void should_write_foo_start_metric()
      {
         MetricSink.Metrics.Count(x => x.Name == "http_request_start" && (string) x.Dimensions["path"] == "/foo").ShouldBe(1);
      }

      [Test]
      public void should_write_bar_start_metric()
      {
         MetricSink.Metrics.Count(x => x.Name == "http_request_start" && (string) x.Dimensions["path"] == "/bar").ShouldBe(1);
      }

      [Test]
      public void should_write_foo_end_metric()
      {
         var metric = (Gauge) MetricSink.Metrics.Single(x => x.Name == "http_request_end" && (string) x.Dimensions["path"] == "/foo");

         Assert.That(metric.Dimensions["method"], Is.EqualTo("GET"));
         Assert.That(metric.Dimensions["statusCode"], Is.EqualTo(200));
      }

      [Test]
      public void should_write_bar_end_metric()
      {
         var metric = (Gauge) MetricSink.Metrics.Single(x => x.Name == "http_request_end" && (string) x.Dimensions["path"] == "/bar");

         Assert.That(metric.Dimensions["method"], Is.EqualTo("GET"));
         Assert.That(metric.Dimensions["statusCode"], Is.EqualTo(429));
      }
   }
}
