using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JSON = System.Text.Json.JsonSerializer;

namespace cfEngine.Serialize
{
    public class JsonSerializer: Serializer
    {
        private JsonSerializerOptions OPTIONS = new()
        {
            IgnoreReadOnlyProperties = false
        };
        
        private static JsonSerializer _instance;
        public static JsonSerializer Instance => _instance ??= new JsonSerializer();

        public override string Serialize(object obj, ISerializer.ISerializeParam param = null)
        {
            return JSON.Serialize(obj, OPTIONS);
        }

        public override async Task<string> SerializeAsync(object obj, ISerializer.ISerializeParam param = null, CancellationToken token = default)
        {
            using var ms = new MemoryStream();
            await JSON.SerializeAsync(ms, obj, OPTIONS, token).ConfigureAwait(false);

            ms.Position = 0;

            using var reader = new StreamReader(ms);
            var jsonString = await reader.ReadToEndAsync().ConfigureAwait(false);
            
            return jsonString;
        }

        public override object Deserialize(byte[] byteLoaded, ISerializer.IDeserializeParam param = null)
        {
            return JSON.Deserialize<object>(byteLoaded, OPTIONS);
        }

        public override async Task<object> DeserializeAsync(byte[] byteLoaded, ISerializer.IDeserializeParam deserializeParam = null,
            CancellationToken token = default)
        {
            using var ms = new MemoryStream(byteLoaded, false);
            var result = await JSON.DeserializeAsync<object>(ms, OPTIONS, cancellationToken: token).ConfigureAwait(false);
            return result;
        }

        public override T DeserializeAs<T>(byte[] byteLoaded, ISerializer.IDeserializeParam param = null) where T : default
        {
            return JSON.Deserialize<T>(byteLoaded, OPTIONS);
        }

        public override async Task<T> DeserializeAsAsync<T>(byte[] byteLoaded, ISerializer.IDeserializeParam deserializeParam = null,
            CancellationToken token = default)
        {
             using var ms = new MemoryStream(byteLoaded, false);
            var result = await JSON.DeserializeAsync<T>(ms, OPTIONS, cancellationToken: token).ConfigureAwait(false);
            return result;           
        }
    }
}