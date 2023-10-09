using Reason.Sdk;

namespace Reason.Serialization;

using Microsoft.OpenApi.Readers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ReasonCommandJsonConverter : JsonConverter
{
	
	public override bool CanWrite => false;
	
	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		throw new NotSupportedException();
	}
	
	public override object ReadJson(
		JsonReader reader, 
		Type objectType, 
		object? existingValue, 
		JsonSerializer serializer)
	{
		JObject? obj = serializer.Deserialize<JObject>(reader);
		if (obj == null)
		{
			throw new JsonException("Expected object");
		}
		var type = (obj["Type"] ?? throw new JsonException("Missing or invalid type for ReasonCommand")).Value<string>();

		Command api = type switch
		{
			"builtin" => new Command(),
			"http" => new HttpCommand(),
			_ => throw new NotSupportedException("Unknown command type " + type)
		};

		serializer.Populate(obj.CreateReader(), api);
		return obj;
	}
	
	public override bool CanConvert(Type objectType)
	{
		return typeof(Command).IsAssignableFrom(objectType);
	}
}
