using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Polly;

namespace Stackage.Core.Tests.Polly.Timing
{
   public class happy_path_with_initial_dimensions
   {
      private StubMetricSink _metricSink;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _metricSink = new StubMetricSink();

         var policyFactory = new PolicyFactory();
         var timingPolicy = policyFactory.CreateAsyncTimingPolicy("foo", _metricSink, new Dictionary<string, object> {{"initial-key", "initial-value"}});

         await timingPolicy.ExecuteAsync(() => Task.Delay(100));
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
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"initial-key"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {"initial-value"}));
      }

      [Test]
      public void should_write_end_metric()
      {
         var metric = (Gauge) _metricSink.Metrics.Last();

         Assert.That(metric.Name, Is.EqualTo("foo_end"));
         Assert.That(metric.Value, Is.InRange(50, 200));
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"initial-key"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {"initial-value"}));
      }
   }
}