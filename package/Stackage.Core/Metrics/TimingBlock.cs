using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stackage.Core.Abstractions.Metrics;

namespace Stackage.Core.Metrics
{
   public class TimingBlock : ITimingBlock
   {
      private readonly string _name;
      private readonly IMetricSink _metricSink;

      public TimingBlock(string name, IMetricSink metricSink)
      {
         _name = name;
         _metricSink = metricSink;
      }

      public IDictionary<string, object> Dimensions { get; } = new Dictionary<string, object>();

      public async Task ExecuteAsync(Func<Task> action)
      {
         if (action == null) throw new ArgumentNullException(nameof(action));

         async Task<bool> VoidAction()
         {
            await action();
            return true;
         }

         await ExecuteAsync(VoidAction);
      }

      public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
      {
         if (action == null) throw new ArgumentNullException(nameof(action));

         await _metricSink.PushAsync(new Counter
         {
            Name = $"{_name}_start",
            Dimensions = Dimensions.ToDictionary(c => c.Key, c => c.Value)
         });

         var stopwatch = Stopwatch.StartNew();

         try
         {
            return await action();
         }
         catch (Exception e)
         {
            Dimensions.Add("exception", e.GetType().FullName);

            throw;
         }
         finally
         {
            stopwatch.Stop();

            await _metricSink.PushAsync(new Gauge
            {
               Name = $"{_name}_end",
               Dimensions = Dimensions.ToDictionary(c => c.Key, c => c.Value),
               Value = stopwatch.ElapsedMilliseconds
            });
         }
      }
   }
}
