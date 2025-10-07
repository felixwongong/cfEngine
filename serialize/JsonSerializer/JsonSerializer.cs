using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JSON = System.Text.Json.JsonSerializer;

namespace cfEngine.Serialize
{
    public partial class JsonSerializer: Serializer
    {
        private readonly JsonSerializerOptions _options;

        private JsonSerializer(JsonSerializerOptions options) : base()
        {
            _options = options;
        }
        
        public override string Serialize(object obj, ISerializer.ISerializeParam? param = null)
        {
            return JSON.Serialize(obj, _options);
        }

        public override async Task<string> SerializeAsync(object obj, ISerializer.ISerializeParam? param = null, CancellationToken token = default)
        {
            using var ms = new MemoryStream();
            await JSON.SerializeAsync(ms, obj, _options, token).ConfigureAwait(false);

            ms.Position = 0;

            using var reader = new StreamReader(ms);
            var jsonString = await reader.ReadToEndAsync().ConfigureAwait(false);
            
            return jsonString;
        }

        public override object Deserialize(byte[] byteLoaded, ISerializer.IDeserializeParam? param = null)
        {
            return JSON.Deserialize<object>(byteLoaded, _options);
        }

        public override async Task<object> DeserializeAsync(byte[] byteLoaded, ISerializer.IDeserializeParam? deserializeParam = null,
            CancellationToken token = default)
        {
            using var ms = new MemoryStream(byteLoaded, false);
            var result = await JSON.DeserializeAsync<object>(ms, _options, cancellationToken: token).ConfigureAwait(false);
            return result;
        }

        public override T DeserializeAs<T>(byte[] byteLoaded, ISerializer.IDeserializeParam? param = null) where T : default
        {
            return JSON.Deserialize<T>(byteLoaded, _options);
        }

        public override async Task<T> DeserializeAsAsync<T>(byte[] byteLoaded, ISerializer.IDeserializeParam? deserializeParam = null,
            CancellationToken token = default)
        {
             using var ms = new MemoryStream(byteLoaded, false);
            var result = await JSON.DeserializeAsync<T>(ms, _options, cancellationToken: token).ConfigureAwait(false);
            return result;           
        }
    }
}