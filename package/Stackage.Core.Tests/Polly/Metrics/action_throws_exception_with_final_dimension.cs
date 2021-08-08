using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Polly;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Polly;

namespace Stackage.Core.Tests.Polly.Metrics
{
   public class action_throws_exception_with_final_dimension
   {
      private const int TimerDurationMs = 17;

      private Exception _exceptionToThrow;
      private Exception _exceptionThrown;
      private StubMetricSink _metricSink;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         Task OnExceptionAsync(Context context, Exception exception)
         {
            context.Add("final-key", exception.GetType().FullName);
            return Task.CompletedTask;
         }

         _exceptionToThrow = new Exception();
         _metricSink = new StubMetricSink();

         var policyFactory = new PolicyFactory(_metricSink, new StubTimerFactory(TimerDurationMs));
         var metricsPolicy = policyFactory.CreateAsyncMetricsPolicy("bar", onExceptionAsync: OnExceptionAsync);

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
         Assert.That(metric.Dimensions.Count, Is.EqualTo(0));
      }

      [Test]
      public void should_write_end_metric()
      {
         var metric = (Gauge) _metricSink.Metrics.Last();

         Assert.That(metric.Name, Is.EqualTo("bar_end"));
         Assert.That(metric.Value, Is.EqualTo(TimerDurationMs));
         Assert.That(metric.Dimensions.Keys, Is.EquivalentTo(new[] {"final-key"}));
         Assert.That(metric.Dimensions.Values, Is.EquivalentTo(new[] {"System.Exception"}));
      }
   }
}
