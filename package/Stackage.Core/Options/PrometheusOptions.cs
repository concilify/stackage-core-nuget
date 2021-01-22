using System;

namespace Stackage.Core.Options
{
   public class PrometheusOptions
   {
      public string MetricsEndpoint { get; set; } = "/metrics";

      public int BufferCapacity { get; set; } = 10000;

      public string SanitiserFallback { get; set; } = "*";

      public double[] Buckets { get; set; } = Array.Empty<double>();

      public Metric[] Metrics { get; set; } = Array.Empty<Metric>();

      public class Metric
      {
         public string Name { get; set; } = string.Empty;

         public string Type { get; set; } = string.Empty;

         public string? Description { get; set; }

         public string[] Labels { get; set; } = Array.Empty<string>();

         public double[]? Buckets { get; set; }

         public Sanitiser[]? Sanitisers { get; set; }
      }

      public class Sanitiser
      {
         public string Label { get; set; } = string.Empty;

         public string? Literal { get; set; }

         public string? Pattern { get; set; }

         public string Value { get; set; } = string.Empty;
      }
   }
}
