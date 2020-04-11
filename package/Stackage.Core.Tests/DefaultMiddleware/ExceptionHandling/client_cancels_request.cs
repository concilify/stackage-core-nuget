using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.Abstractions.Metrics;

namespace Stackage.Core.Tests.DefaultMiddleware.ExceptionHandling
{
   public class client_cancels_request : middleware_scenario
   {
      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200)))
         {
            try
            {
               await TestService.GetAsync("/get", cancellationToken: cts.Token);
            }
            catch (OperationCanceledException)
            {
               // Expecting this so do nothing
            }
         }
      }

      protected override void Configure(IApplicationBuilder app)
      {
         base.Configure(app);

         app.UseMiddleware<StubResponseMiddleware>(new StubResponseOptions {Latency = TimeSpan.FromSeconds(10)});
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

         // Time is usually c120ms less than specified in from CancellationTokenSource
         Assert.That(metric.Value, Is.InRange(50, 250));
      }
   }
}
