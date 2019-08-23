using System;
using System.Reflection;
using Stackage.Core.Abstractions;

namespace Stackage.Core
{
   public class ServiceInfo : IServiceInfo
   {
      public string Service { get; } = Assembly.GetEntryAssembly().GetName().Name;

      public string Version { get; } = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

      public string Host { get; } = Environment.MachineName;
   }
}
