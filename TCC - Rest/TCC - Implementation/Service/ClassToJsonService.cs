using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TCC___Implementation.Service
{
    public class ClassToJsonService
    {
        public void JsonSerialize(object data, string filePath)
        {
            JsonSerializer jsonSerializer = new JsonSerializer();
            if (File.Exists(filePath)) File.Delete(filePath);
            StreamWriter sw = new StreamWriter(filePath);
            JsonWriter jsonWriter = new JsonTextWriter(sw);
            jsonSerializer.Serialize(jsonWriter, data);
            jsonWriter.Close();
            sw.Close();
        }

        public object JsonDeserialize(Type dataType, string filePath)
        {
            JObject obj = null;
            JsonSerializer jsonSerializer = new JsonSerializer();
            if(File.Exists(filePath))
            {
                StreamReader sr = new StreamReader(filePath);
                JsonReader jsonReader = new JsonTextReader(sr);
                obj = jsonSerializer.Deserialize(jsonReader) as JObject;
                jsonReader.Close();
                sr.Close();
            }
            return obj.ToObject(dataType);
        }
    }
}
