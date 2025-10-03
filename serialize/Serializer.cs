using System.Threading;
using System.Threading.Tasks;

namespace cfEngine.Serialize
{
    public interface ISerializer
    {
        public interface ISerializeParam
        {
        }

        public interface IDeserializeParam
        {
        }

        public string Serialize(object obj, ISerializeParam? param = null);
        public Task<string> SerializeAsync(object obj, ISerializeParam? param = null, CancellationToken token = default);
        public object Deserialize(byte[] byteLoaded, IDeserializeParam? param = null);
        public Task<object> DeserializeAsync(byte[] byteLoaded, IDeserializeParam? param = null, CancellationToken token = default);
        public T DeserializeAs<T>(byte[] byteLoaded, IDeserializeParam? param = null);
        public Task<T> DeserializeAsAsync<T>(byte[] byteLoaded, IDeserializeParam? param = null, CancellationToken token = default);
    }
    
    public abstract class Serializer: ISerializer
    {
        public abstract string Serialize(object obj, ISerializer.ISerializeParam? param = null);
        public abstract Task<string> SerializeAsync(object obj, ISerializer.ISerializeParam? param = null, CancellationToken token = default);
        public abstract object Deserialize(byte[] byteLoaded, ISerializer.IDeserializeParam? param = null);
        public abstract Task<object> DeserializeAsync(byte[] byteLoaded, ISerializer.IDeserializeParam? param = null, CancellationToken token = default);
        public abstract T DeserializeAs<T>(byte[] byteLoaded, ISerializer.IDeserializeParam? param = null);
        public abstract Task<T> DeserializeAsAsync<T>(byte[] byteLoaded, ISerializer.IDeserializeParam? param = null, CancellationToken token = default);
    }
}