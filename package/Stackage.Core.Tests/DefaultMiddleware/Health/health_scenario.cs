using System.Linq;
using NUnit.Framework;
using Shouldly;
using Stackage.Core.Abstractions.Metrics;

namespace Stackage.Core.Tests.DefaultMiddleware.Health
{
   public abstract class health_scenario : middleware_scenario
   {
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
         Assert.That(metric.Dimensions["path"], Is.EqualTo("/health"));
      }

      [Test]
      public void should_write_end_metric()
      {
         var metric = (Gauge) MetricSink.Metrics.Last();

         Assert.That(metric.Name, Is.EqualTo("http_request_end"));
         Assert.That(metric.Value, Is.GreaterThanOrEqualTo(0));
         Assert.That(metric.Dimensions["method"], Is.EqualTo("GET"));
         Assert.That(metric.Dimensions["path"], Is.EqualTo("/health"));
      }

      [Test]
      public void should_not_log_a_message()
      {
         Logger.Entries.Count.ShouldBe(0);
      }
   }
}
