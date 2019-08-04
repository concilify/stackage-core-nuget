using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Polly;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Polly;

namespace Stackage.Core.Tests.Polly.Timing
{
   public class happy_path_with_final_dimensions
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
         var timingPolicy = policyFactory.CreateAsyncTimingPolicy("foo", _metricSink, onSuccessAsync: OnSuccessAsync);

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
         Assert.That(metric.Dimensions.Keys, Is.Empty);
         Assert.That(metric.Dimensions.Values, Is.Empty);
      }

      [Test]
      public void should_write_end_metric()
      {
         var metric = (Gauge) _metricSink.Metrics.Last();

         Assert.That(metric.Name, Is.EqualTo("foo_end"));
         Assert.That(metric.Value, Is.InRange(50, 200));
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"final-key"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {"final-value"}));
      }
   }
}
