using System.Collections.Generic;

namespace Stackage.Core.MetricSinks
{
   public class PrometheusOptions
   {
      public int BufferCapacity { get; set; } = 100;

      public string SanitiserFallback { get; set; } = "*";

      public double[] Buckets { get; set; }

      public IList<Metric> Metrics { get; set; } = new List<Metric>();
   }
}
