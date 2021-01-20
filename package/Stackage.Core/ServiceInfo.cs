using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Stackage.Core.Abstractions;

namespace Stackage.Core
{
   public class ServiceInfo : IServiceInfo
   {
      private const string Unknown = "(Unknown)";

      private readonly IHttpContextAccessor _httpContextAccessor;

      public ServiceInfo(IHttpContextAccessor httpContextAccessor)
      {
         _httpContextAccessor = httpContextAccessor;
      }

      public string Service { get; } = Assembly.GetEntryAssembly()?.GetName().Name ?? Unknown;

      public string Version { get; } = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? Unknown;

      public string Host { get; } = Environment.MachineName;

      public string BaseAddress
      {
         get
         {
            var request = _httpContextAccessor.HttpContext?.Request;

            return request != null ? $"{request.Scheme}://{request.Host}{request.PathBase}" : Unknown;
         }
      }
   }
}
