using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.Abstractions.Metrics;

namespace Stackage.Core.Tests.DefaultMiddleware.ExceptionHandling
{
   // TODO: These don't run on their own, but do with all the others
   public class client_cancels_request : middleware_scenario
   {
      private HttpResponseMessage _response;
      private string _content;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50)))
         {
            _response = await TestService.GetAsync("/get", cancellationToken: cts.Token);
            _content = await _response.Content.ReadAsStringAsync();
         }
      }

      protected override void Configure(IApplicationBuilder app)
      {
         base.Configure(app);

         app.UseMiddleware<StubResponseMiddleware>(new StubResponseOptions {Latency = TimeSpan.FromSeconds(10)});
      }

      [Test]
      public void should_return_status_code_499()
      {
         _response.StatusCode.ShouldBe((HttpStatusCode) 499);
      }

      [Test]
      public void should_return_json_content_with_token()
      {
         _content.ShouldBe("{\"message\":\"Client Closed Request\"}");
      }

      [Test]
      public void should_return_content_type_json()
      {
         _response.Content.Headers.ContentType.MediaType.ShouldBe("application/json");
      }

      [Test]
      public void should_not_log_a_message()
      {
         Logger.Entries.Count.ShouldBe(0);
      }

      [Test]
      public void should_write_two_metrics()
      {
         Assert.That(MetricSink.Metrics.Count, Is.EqualTo(2));
      }

      [Test]
      public void should_write_start_metric()
      {
         var metric = (Counter) MetricSink.Metrics.First();

         Assert.That(metric.Name, Is.EqualTo("http_request_start"));
         Assert.That(metric.Dimensions["method"], Is.EqualTo("GET"));
         Assert.That(metric.Dimensions["path"], Is.EqualTo("/get"));
      }

      [Test]
      public void should_write_end_metric()
      {
         var metric = (Gauge) MetricSink.Metrics.Last();

         Assert.That(metric.Name, Is.EqualTo("http_request_end"));
         Assert.That(metric.Value, Is.GreaterThanOrEqualTo(0));
         Assert.That(metric.Dimensions["method"], Is.EqualTo("GET"));
         Assert.That(metric.Dimensions["path"], Is.EqualTo("/get"));
         Assert.That(metric.Dimensions["statusCode"], Is.EqualTo(499));
      }

      [Test]
      public void should_write_end_metric_with_duration_similar_to_timeout()
      {
         var metric = (Gauge) MetricSink.Metrics.Last();

         Assert.That(metric.Value, Is.InRange(40, 100));
      }
   }
}
