using System;

namespace Stackage.Core.StartupTasks
{
   public class StartupException : Exception
   {
      public StartupException(string message)
         : base(message)
      {
      }

      public StartupException(string message, Exception innerException)
         : base(message, innerException)
      {
      }
   }
}
