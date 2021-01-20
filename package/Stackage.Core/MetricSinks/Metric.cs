using System;

namespace Stackage.Core.MetricSinks
{
   public class Metric
   {
      public string? Name { get; set; }

      public string? Type { get; set; }

      public string? Description { get; set; }

      public string[] Labels { get; set; } = Array.Empty<string>();

      public double[] Buckets { get; set; } = Array.Empty<double>();

      public Sanitiser[] Sanitisers { get; set; } = Array.Empty<Sanitiser>();
   }
}
