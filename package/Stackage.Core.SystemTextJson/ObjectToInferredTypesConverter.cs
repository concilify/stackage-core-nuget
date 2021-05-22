using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Stackage.Core.SystemTextJson
{
   public class ObjectToInferredTypesConverter : JsonConverter<object>
   {
      public override object Read(
         ref Utf8JsonReader reader,
         Type typeToConvert,
         JsonSerializerOptions options)
      {
         object? result = reader.TokenType switch
         {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number when reader.TryGetInt64(out var value) => value,
            JsonTokenType.Number => reader.GetDouble(),
            JsonTokenType.String when reader.TryGetDateTime(out var value) => value,
            JsonTokenType.String => reader.GetString(),
            _ => JsonDocument.ParseValue(ref reader).RootElement.Clone()
         };

         if (result == null) throw new InvalidOperationException();

         return result;
      }

      public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
      {
         JsonSerializer.Serialize(writer, value, value.GetType(), options);
      }
   }
}
