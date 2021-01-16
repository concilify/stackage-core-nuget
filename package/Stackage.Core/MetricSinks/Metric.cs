using System.Collections.Generic;

namespace Stackage.Core.MetricSinks
{
   public class Metric
   {
      public string Name { get; set; }

      public string Type { get; set; }

      public string Description { get; set; }

      public string[] Labels { get; set; }

      public double[] Buckets { get; set; }

      public IList<Sanitiser> Sanitisers { get; set; } = new List<Sanitiser>();
   }
}
