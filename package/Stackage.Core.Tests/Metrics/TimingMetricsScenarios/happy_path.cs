using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Metrics;

namespace Stackage.Core.Tests.Metrics.TimingMetricsScenarios
{
   public class happy_path
   {
      private StubMetricSink _metricSink;

      [OneTimeSetUp]
      public async Task setup_once_before_all_tests()
      {
         _metricSink = new StubMetricSink();

         var timingBlockFactory = new TimingBlockFactory(_metricSink);
         var timingBlock = timingBlockFactory.Create("foo");

         await timingBlock.ExecuteAsync(() => Task.Delay(100));
      }

      [Test]
      public void should_write_two_metrics()
      {
         Assert.That(_metricSink.Metrics.Count, Is.EqualTo(2));
      }

      [Test]
      public void should_write_start_metric()
      {
         var metric = (Counter) _metricSink.Metrics.First();

         Assert.That(metric.Name, Is.EqualTo("foo_start"));
      }

      [Test]
      public void should_write_end_metric()
      {
         var metric = (Gauge) _metricSink.Metrics.Last();

         Assert.That(metric.Name, Is.EqualTo("foo_end"));
         Assert.That(metric.Value, Is.InRange(50, 200));
      }
   }
}
