using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Polly;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Polly;

namespace Stackage.Core.Tests.Polly.Metrics
{
   public class happy_path_double_execute
   {
      private StubMetricSink _metricSink;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         Task OnSuccessAsync(Context context)
         {
            context.Add("success-key", "success-value");
            return Task.CompletedTask;
         }

         _metricSink = new StubMetricSink();

         var policyFactory = new PolicyFactory();
         var metricsPolicy = policyFactory.CreateAsyncMetricsPolicy("foo", _metricSink, onSuccessAsync: OnSuccessAsync);

         await metricsPolicy.ExecuteAsync((context) => Task.Delay(5), new Dictionary<string, object> {{"execute-key", "execute-value-1"}});
         await metricsPolicy.ExecuteAsync((context) => Task.Delay(5), new Dictionary<string, object> {{"execute-key", "execute-value-2"}});
      }

      [Test]
      public void should_write_four_metrics()
      {
         Assert.That(_metricSink.Metrics.Count, Is.EqualTo(4));
      }

      [Test]
      public void should_write_first_start_metric()
      {
         var metric = (Counter) _metricSink.Metrics.First(x => x.Name == "foo_start");

         Assert.That(metric.Name, Is.EqualTo("foo_start"));
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"execute-key"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {"execute-value-1"}));
      }

      [Test]
      public void should_write_first_end_metric()
      {
         var metric = (Gauge) _metricSink.Metrics.First(x => x.Name == "foo_end");

         Assert.That(metric.Name, Is.EqualTo("foo_end"));
         Assert.That(metric.Value, Is.GreaterThan(0));
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"execute-key", "success-key"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {"execute-value-1", "success-value"}));
      }

      [Test]
      public void should_write_last_start_metric()
      {
         var metric = (Counter) _metricSink.Metrics.Last(x => x.Name == "foo_start");

         Assert.That(metric.Name, Is.EqualTo("foo_start"));
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"execute-key"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {"execute-value-2"}));
      }

      [Test]
      public void should_write_last_end_metric()
      {
         var metric = (Gauge) _metricSink.Metrics.Last(x => x.Name == "foo_end");

         Assert.That(metric.Name, Is.EqualTo("foo_end"));
         Assert.That(metric.Value, Is.GreaterThan(0));
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"execute-key", "success-key"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {"execute-value-2", "success-value"}));
      }
   }
}
