using System.IO;
using System.Text;
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

        public abstract Stream Serialize(object obj, ISerializeParam param = null);
        public abstract object Deserialize(Stream stream, IDeserializeParam param = null);
        public abstract T DeserializeAs<T>(Stream stream, IDeserializeParam param = null);
    }

    public class JsonStreamSerializer : StreamSerializer
    {
        public override Stream Serialize(object obj, ISerializeParam param = null)
        {
            var stream = new MemoryStream();
            var serializer = new JsonSerializer();
            using var streamWriter = new StreamWriter(stream, Encoding.Default, 1024, true);
            using var jsonWriter = new JsonTextWriter(streamWriter);
            serializer.Serialize(jsonWriter, obj);
            
            jsonWriter.Flush();
            stream.Position = 0;
            
            return stream;
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