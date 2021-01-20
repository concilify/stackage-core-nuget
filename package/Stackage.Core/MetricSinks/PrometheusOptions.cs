using System;

namespace Stackage.Core.MetricSinks
{
   public class PrometheusOptions
   {
      public int BufferCapacity { get; set; } = 100;

      public string SanitiserFallback { get; set; } = "*";

      public double[] Buckets { get; set; } = Array.Empty<double>();

      public Metric[] Metrics { get; set; } = Array.Empty<Metric>();
   }
}
