namespace Stackage.Core.Middleware.Options
{
   public class RateLimitingOptions
   {
      public bool Enabled { get; set; }

      public int RequestsPerPeriod { get; set; }

      public double PeriodSeconds { get; set; }

      public int BurstSize { get; set; }

      public int MaxWaitMs { get; set; } = 3000;
   }
}
