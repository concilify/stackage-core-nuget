using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Polly;

namespace Stackage.Core.Tests.Polly.Metrics
{
   public class happy_path_with_execute_dimensions
   {
      private const int TimerDurationMs = 31;

      private StubMetricSink _metricSink;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _metricSink = new StubMetricSink();

         var policyFactory = new PolicyFactory(new StubTimerFactory(TimerDurationMs));
         var metricsPolicy = policyFactory.CreateAsyncMetricsPolicy("foo", _metricSink);

         await metricsPolicy.ExecuteAsync(async _ => await Task.Yield(), new Dictionary<string, object> {{"execute-key", "execute-value"}});
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
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"execute-key"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {"execute-value"}));
      }

      [Test]
      public void should_write_end_metric()
      {
         var metric = (Gauge) _metricSink.Metrics.Last();

         Assert.That(metric.Name, Is.EqualTo("foo_end"));
         Assert.That(metric.Value, Is.EqualTo(TimerDurationMs));
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"execute-key"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {"execute-value"}));
      }
   }
}
