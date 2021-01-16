using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.Abstractions.Metrics;

namespace Stackage.Core.Tests.DefaultMiddleware.Metrics
{
   public class get_request : middleware_scenario
   {
      private HttpResponseMessage _response;
      private string _content;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _response = await TestService.GetAsync("/get");
         _content = await _response.Content.ReadAsStringAsync();
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
      public void should_return_content()
      {
         _content.ShouldBe("content");
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
         Assert.That(metric.Dimensions["statusCode"], Is.EqualTo(200));
      }
   }
}
