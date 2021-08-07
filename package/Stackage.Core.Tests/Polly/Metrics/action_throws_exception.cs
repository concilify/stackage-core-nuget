using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Polly;

namespace Stackage.Core.Tests.Polly.Metrics
{
   public class action_throws_exception
   {
      private const int TimerDurationMs = 11;

      private Exception _exceptionToThrow;
      private Exception _exceptionThrown;
      private StubMetricSink _metricSink;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _exceptionToThrow = new Exception();
         _metricSink = new StubMetricSink();

         var policyFactory = new PolicyFactory(new StubTimerFactory(TimerDurationMs));
         var metricsPolicy = policyFactory.CreateAsyncMetricsPolicy("bar", _metricSink);

         try
         {
            await metricsPolicy.ExecuteAsync(async () =>
            {
               await Task.Yield();

               throw _exceptionToThrow;
            });
         }
         catch (Exception e)
         {
            _exceptionThrown = e;
         }
      }

      [Test]
      public void should_throw_exception()
      {
         Assert.That(_exceptionThrown, Is.SameAs(_exceptionToThrow));
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
         Assert.That(metric.Dimensions.Count, Is.EqualTo(0));
      }
   }
}
