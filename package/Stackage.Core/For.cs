using System;
using System.Threading.Tasks;

namespace Stackage.Core
{
   public static class For
   {
      public static async Task OptionalInvokeAsync<T>(Func<T, Task> nullableActionAsync, T arg)
      {
         if (nullableActionAsync != null)
         {
            await nullableActionAsync(arg);
         }
      }

      public static async Task OptionalInvokeAsync<T1, T2>(Func<T1, T2, Task> nullableActionAsync, T1 arg1, T2 arg2)
      {
         if (nullableActionAsync != null)
         {
            await nullableActionAsync(arg1, arg2);
         }
      }
   }
}
