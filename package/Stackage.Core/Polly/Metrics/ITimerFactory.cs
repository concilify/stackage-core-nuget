namespace Stackage.Core.Polly.Metrics
{
   // TODO: Move to abstractions
   public interface ITimerFactory
   {
      ITimer CreateAndStart();
   }
}
