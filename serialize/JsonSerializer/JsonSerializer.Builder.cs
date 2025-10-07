using System.Text.Json;
using System.Text.Json.Serialization;

namespace cfEngine.Serialize
{
    public partial class JsonSerializer
    {
        public struct SerializerOptions
        {
            public bool IncludeFields;
            public bool IncludeReadOnlyProperties;
        }

        public partial class Builder
        {
            private static JsonSerializerOptions emptyOption = new JsonSerializerOptions();
            private JsonSerializerOptions _options;

            public Builder WithOptions(SerializerOptions options)
            {
                _options = new JsonSerializerOptions()
                {
                    IncludeFields = options.IncludeFields,
                    IgnoreReadOnlyFields = !options.IncludeReadOnlyProperties,
                };
                return this;
            }

            public Builder AddConverter(JsonConverter converter)
            {
                _options ??= new JsonSerializerOptions();
                _options.Converters.Add(converter);
                return this;
            }

            public JsonSerializer Build()
            {
                var option = _options ?? emptyOption;
                return new JsonSerializer(option);
            }
        }
    }
}