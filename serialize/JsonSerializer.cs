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
            IncludeFields = true,
            IgnoreReadOnlyFields = false,
            IgnoreReadOnlyProperties = false
        };
        
        private static JsonSerializer _instance;
        public static JsonSerializer Instance => _instance ??= new JsonSerializer();

        public override byte[] Serialize(object obj, ISerializeParam param = null)
        {
            return JSON.SerializeToUtf8Bytes(obj, OPTIONS);
        }

        public override async Task<byte[]> SerializeAsync(object obj, ISerializeParam param = null, CancellationToken token = default)
        {
            using var ms = new MemoryStream();
            await JSON.SerializeAsync(ms, obj, OPTIONS,cancellationToken:token).ConfigureAwait(false);

            var loadedByte = new byte[ms.Length];
            await ms.WriteAsync(loadedByte, 0, loadedByte.Length, token).ConfigureAwait(false);
            return loadedByte;
        }

        public override object Deserialize(byte[] byteLoaded, IDeserializeParam param = null)
        {
            return JSON.Deserialize<object>(byteLoaded, OPTIONS);
        }

        public override async Task<object> DeserializeAsync(byte[] byteLoaded, IDeserializeParam deserializeParam = null,
            CancellationToken token = default)
        {
            using var ms = new MemoryStream(byteLoaded, false);
            var result = await JSON.DeserializeAsync<object>(ms, OPTIONS, cancellationToken: token).ConfigureAwait(false);
            return result;
        }

        public override T DeserializeAs<T>(byte[] byteLoaded, IDeserializeParam param = null) where T : default
        {
            return JSON.Deserialize<T>(byteLoaded, OPTIONS);
        }

        public override async Task<T> DeserializeAsAsync<T>(byte[] byteLoaded, IDeserializeParam deserializeParam = null,
            CancellationToken token = default)
        {
             using var ms = new MemoryStream(byteLoaded, false);
            var result = await JSON.DeserializeAsync<T>(ms, OPTIONS, cancellationToken: token).ConfigureAwait(false);
            return result;           
        }
    }
}