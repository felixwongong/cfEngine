using System.Text.Json;

namespace cfEngine.Util
{
    public static class JsonElementExtension
    {
        public static object ToObject(this JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.String:
                    return jsonElement.GetString();
                case JsonValueKind.True or JsonValueKind.False:
                    return jsonElement.GetBoolean();
                case JsonValueKind.Number:
                    return jsonElement.GetDouble();
                default:
                    return null;
            }
        }
    }
}