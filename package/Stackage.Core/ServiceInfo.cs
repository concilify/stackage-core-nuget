using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Stackage.Core.Abstractions;

namespace Stackage.Core
{
   public class ServiceInfo : IServiceInfo
   {
      private readonly IHttpContextAccessor _httpContextAccessor;

      public ServiceInfo(IHttpContextAccessor httpContextAccessor)
      {
         _httpContextAccessor = httpContextAccessor;
      }

      public string Service { get; } = Assembly.GetEntryAssembly().GetName().Name;

      public string Version { get; } = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

      public string Host { get; } = Environment.MachineName;

      public string BaseAddress
      {
         get
         {
            var request = _httpContextAccessor.HttpContext.Request;

            return $"{request.Scheme}://{request.Host}{request.PathBase}";
         }
      }
   }
}
