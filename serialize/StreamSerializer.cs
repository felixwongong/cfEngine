using System.IO;
using Newtonsoft.Json;

namespace cfEngine.Serialize
{
    public abstract class StreamSerializer
    {
        public interface ISerializeParam
        {
        }

        public interface IDeserializeParam
        {
        }

        public abstract void Serialize(object obj, Stream stream, ISerializeParam param = null);
        public abstract object Deserialize(Stream stream, IDeserializeParam param = null);
        public abstract T DeserializeAs<T>(Stream stream, IDeserializeParam param = null);
    }

    public class JsonStreamSerializer : StreamSerializer
    {
        public override void Serialize(object obj, Stream stream, ISerializeParam param = null)
        {
            var serializer = JsonSerializer.Create();
            using var streamWriter = new StreamWriter(stream);
            using var jsonWriter = new JsonTextWriter(streamWriter);

            serializer.Serialize(jsonWriter, obj);
        }

        public override object Deserialize(Stream stream, IDeserializeParam param = null)
        {
            using var streamReader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(streamReader);

            var serializer = JsonSerializer.Create();

            return serializer.Deserialize(jsonReader);
        }

        public override T DeserializeAs<T>(Stream stream, IDeserializeParam param = null) where T : default
        {
            using var streamReader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(streamReader);

            var serializer = JsonSerializer.Create();

            return serializer.Deserialize<T>(jsonReader);
        }
    }
}