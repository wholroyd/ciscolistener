using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace CiscoListener.Helpers
{
    public static class Serializer
    {
        public static void SaveXml<T>(string path, T config)
        {
            var x = new XmlSerializer(typeof(T));
            using (var w = XmlWriter.Create(path, new XmlWriterSettings {Indent = true}))
            {
                x.Serialize(w, config);
            }
        }

        public static void LoadXml<T>(string path, out T config)
        {
            var x = new XmlSerializer(typeof(T));
            using (var r = XmlReader.Create(path))
            {
                config = (T)x.Deserialize(r);
            }
        }

        public static void SaveJson<T>(string path, T config)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(config));
        }

        public static void LoadJson<T>(string path, out T config)
        {
            config = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        }
    }
}