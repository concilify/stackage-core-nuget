using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Polly;

namespace Stackage.Core.Tests.Polly.Metrics
{
   public class happy_path_with_return
   {
      private const int TimerDurationMs = 41;

      private StubMetricSink _metricSink;
      private int _result;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _metricSink = new StubMetricSink();

         var policyFactory = new PolicyFactory(_metricSink, new StubTimerFactory(TimerDurationMs));
         var metricsPolicy = policyFactory.CreateAsyncMetricsPolicy("bar");

         _result = await metricsPolicy.ExecuteAsync(async () =>
         {
            await Task.Yield();

            return 23;
         });
      }

      [Test]
      public void should_return_result()
      {
         Assert.That(_result, Is.EqualTo(23));
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

         Assert.That(metric.Name, Is.EqualTo("bar_start"));
      }

      [Test]
      public void should_write_end_metric()
      {
         var metric = (Gauge) _metricSink.Metrics.Last();

         Assert.That(metric.Name, Is.EqualTo("bar_end"));
         Assert.That(metric.Value, Is.EqualTo(TimerDurationMs));
      }
   }
}
