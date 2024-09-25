using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

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
        public abstract T DeserializeAs<T>(byte[] byteLoaded, IDeserializeParam param = null);
    }

    public class JsonSerializer : Serializer
    {
        private static JsonSerializer _instance;
        public static JsonSerializer Instance => _instance ??= new JsonSerializer();
        
        public override byte[] Serialize(object obj, ISerializeParam param = null)
        {
            var stream = new MemoryStream();
            var serializer = new Newtonsoft.Json.JsonSerializer();
            using var streamWriter = new StreamWriter(stream, Encoding.Default, 1024, true);
            using var jsonWriter = new JsonTextWriter(streamWriter);
            serializer.Serialize(jsonWriter, obj);
            
            jsonWriter.Flush();
            stream.Position = 0;

            var loadedByte = new byte[stream.Length];
            var byteLoadedCount = stream.Read(loadedByte);
            if (byteLoadedCount < loadedByte.Length)
            {
                throw new Exception("Byte load from stream less than input");
            }
            
            return loadedByte;
        }

        public override object Deserialize(byte[] byteLoaded, IDeserializeParam param = null)
        {
            using var memoryStream = new MemoryStream(byteLoaded);
            using var streamReader = new StreamReader(memoryStream, Encoding.Default, true, 1024, true);
            
            using var jsonReader = new JsonTextReader(streamReader);

            var serializer = Newtonsoft.Json.JsonSerializer.Create();

            return serializer.Deserialize(jsonReader);
        }

        public override T DeserializeAs<T>(byte[] byteLoaded, IDeserializeParam param = null) where T : default
        {
            using var memoryStream = new MemoryStream(byteLoaded);
            using var streamReader = new StreamReader(memoryStream, Encoding.Default, true, 1024, true);
            
            using var jsonReader = new JsonTextReader(streamReader);
            
            var serializer = Newtonsoft.Json.JsonSerializer.Create();
            
            return serializer.Deserialize<T>(jsonReader);
        }
    }
}