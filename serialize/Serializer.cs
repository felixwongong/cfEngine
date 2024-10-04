using System.Threading;
using System.Threading.Tasks;

namespace cfEngine.Serialize
{
    public abstract class Serializer
    {
        public interface ISerializeParam
        {
        }

        public interface IDeserializeParam
        {
        }

        public abstract byte[] Serialize(object obj, ISerializeParam param = null);
        public abstract object Deserialize(byte[] byteLoaded, IDeserializeParam param = null);
        public abstract Task<object> DeserializeAsync(byte[] byteLoaded, CancellationToken token, IDeserializeParam param = null);
        public abstract T DeserializeAs<T>(byte[] byteLoaded, IDeserializeParam param = null);
        public abstract Task<T> DeserializeAsAsync<T>(byte[] byteLoaded, CancellationToken token, IDeserializeParam param = null);
    }
}