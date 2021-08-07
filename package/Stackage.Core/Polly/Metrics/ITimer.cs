namespace Stackage.Core.Polly.Metrics
{
   // TODO: Move to abstractions
   public interface ITimer
   {
      long ElapsedMilliseconds { get; }

      void Stop();
   }
}
