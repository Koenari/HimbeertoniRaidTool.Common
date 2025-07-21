using HimbeertoniRaidTool.Common.Security;
using Newtonsoft.Json.Linq;

namespace HimbeertoniRaidTool.Common.Services;

public class CloneService
{
    private static CloneService Instance { get; } = new();

    private readonly JsonSerializer _serializer;
    private readonly JsonSerializerSettings _settings = new()
    {

    };
    private readonly ReferenceConverter _referenceConverter = new();

    private CloneService()
    {
        _settings.Converters.Add(_referenceConverter);
        _serializer = JsonSerializer.Create(_settings);
        _serializer.Converters.Add(_referenceConverter);
    }

    private TData InternalClone<TData>(TData data)
    {
        using var writer = new JTokenWriter();
        _serializer.Serialize(writer, data, typeof(TData));
        using var reader = new JTokenReader(writer.Token!);
        return _serializer.Deserialize<TData>(reader)!;
    }

    public static TData Clone<TData>(TData data) => Instance.InternalClone(data);

    private class ReferenceConverter : JsonConverter<IReference>
    {
        private readonly Dictionary<HrtId, IReference> _references = new();
        public override void WriteJson(JsonWriter writer, IReference? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }
            _references.TryAdd(value.Id, value);
            serializer.Serialize(writer, value.Id, value.GetType());
        }

        public override IReference? ReadJson(JsonReader reader, Type objectType, IReference? existingValue,
                                             bool hasExistingValue,
                                             JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var id = JObject.Load(reader).ToObject<HrtId>();
            if (id is null) return null;
            return _references.GetValueOrDefault(id, null!);
        }
    }

}