using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WooCommerce.Http.SourceInstallation.Structures
{


  public class StringOrArrayConverter : JsonConverter<string?>
  {
    public override string? ReadJson(JsonReader reader, Type objectType, string? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
      switch (reader.TokenType)
      {
        case JsonToken.StartArray:
          var array = JArray.Load(reader);
          return array.First?.ToString();

        case JsonToken.StartObject:
          var obj = JObject.Load(reader);
          // Get first value in the object, or join all values if multiple
          return obj.Properties().FirstOrDefault()?.Value?.ToString();

        case JsonToken.String:
          return reader.Value?.ToString();

        case JsonToken.Null:
          return null;

        default:
          throw new JsonSerializationException($"Unexpected token {reader.TokenType}");
      }
    }

    public override void WriteJson(JsonWriter writer, string? value, JsonSerializer serializer)
    {
      writer.WriteValue(value);
    }
  }


}
