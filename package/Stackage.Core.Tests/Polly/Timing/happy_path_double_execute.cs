using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Polly;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Polly;

namespace Stackage.Core.Tests.Polly.Timing
{
   public class happy_path_double_execute
   {
      private StubMetricSink _metricSink;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         Task OnSuccessAsync(Context context)
         {
            context.Add("final-key", "final-value");
            return Task.CompletedTask;
         }

         _metricSink = new StubMetricSink();

         var policyFactory = new PolicyFactory();
         var timingPolicy = policyFactory.CreateAsyncTimingPolicy("foo", _metricSink, new Dictionary<string, object> {{"initial-key", "initial-value"}},
            onSuccessAsync: OnSuccessAsync);

         await timingPolicy.ExecuteAsync((context) => Task.Delay(5), new Dictionary<string, object> {{"execute-key", "execute-value-1"}});
         await timingPolicy.ExecuteAsync((context) => Task.Delay(5), new Dictionary<string, object> {{"execute-key", "execute-value-2"}});
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
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"initial-key", "execute-key"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {"initial-value", "execute-value-1"}));
      }

      [Test]
      public void should_write_first_end_metric()
      {
         var metric = (Gauge) _metricSink.Metrics.First(x => x.Name == "foo_end");

         Assert.That(metric.Name, Is.EqualTo("foo_end"));
         Assert.That(metric.Value, Is.GreaterThan(0));
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"initial-key", "execute-key", "final-key"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {"initial-value", "execute-value-1", "final-value"}));
      }

      [Test]
      public void should_write_last_start_metric()
      {
         var metric = (Counter) _metricSink.Metrics.Last(x => x.Name == "foo_start");

         Assert.That(metric.Name, Is.EqualTo("foo_start"));
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"initial-key", "execute-key"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {"initial-value", "execute-value-2"}));
      }

      [Test]
      public void should_write_last_end_metric()
      {
         var metric = (Gauge) _metricSink.Metrics.Last(x => x.Name == "foo_end");

         Assert.That(metric.Name, Is.EqualTo("foo_end"));
         Assert.That(metric.Value, Is.GreaterThan(0));
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"initial-key", "execute-key", "final-key"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {"initial-value", "execute-value-2", "final-value"}));
      }
   }
}
