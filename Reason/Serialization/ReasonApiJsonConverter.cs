using Reason.Sdk;

namespace Reason.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ReasonApiJsonConverter : JsonConverter
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
		if (obj == null) { throw new JsonException("Expected object"); }

		var type = (obj["Type"] ?? throw new JsonException("Missing or invalid type for ReasonApi")).Value<string>();

		ReasonApi api;
		switch (type)
		{
			case "builtin":
				api = new BuiltinReasonApi();
				break;

			case "openapi":
				api = new OpenApiReasonApi();
				break;

			default:
				throw new NotSupportedException("Unknown api type " + type);
		}

		serializer.Populate(obj.CreateReader(), api);
		return obj;
	}

	public override bool CanConvert(Type objectType)
	{
		return typeof(ReasonApi).IsAssignableFrom(objectType);
	}
}
