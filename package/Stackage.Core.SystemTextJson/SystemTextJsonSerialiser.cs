using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Stackage.Core.Abstractions;

namespace Stackage.Core.SystemTextJson
{
   public class SystemTextJsonSerialiser : IJsonSerialiser
   {
      private readonly JsonSerializerOptions _options;

      public SystemTextJsonSerialiser(IEnumerable<JsonConverter> converters)
      {
         _options = new JsonSerializerOptions
         {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreNullValues = true
         };

         _options.Converters.Add(new ObjectToInferredTypesConverter());

         foreach (var converter in converters)
         {
            _options.Converters.Add(converter);
         }
      }

      public string Serialise<TValue>(TValue value)
      {
         var json = JsonSerializer.Serialize(value, _options);
         return json;
      }

      public T Deserialise<T>(string json)
      {
         var value = JsonSerializer.Deserialize<T>(json, _options);

         if (value == null) throw new InvalidOperationException();

         return value;
      }

      public async Task<T> DeserialiseAsync<T>(Stream jsonStream)
      {
         var value = await JsonSerializer.DeserializeAsync<T>(jsonStream, _options);

         if (value == null) throw new InvalidOperationException();

         return value;
      }
   }
}
