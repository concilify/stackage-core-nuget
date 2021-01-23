namespace Stackage.Core.Options
{
   public class HealthOptions
   {
      public string Endpoint { get; set; } = "/health";

      public string LivenessEndpoint { get; set; } = "/health/liveness";

      public string ReadinessEndpoint { get; set; } = "/health/readiness";
   }
}
