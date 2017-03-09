using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Xml;

namespace FoodJournalServiceWebRole.FoodJournalService
{
    public static class Util
    {
        //TODO: deal with UTF16?
        public static string Serialize(object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                DataContractSerializer serializer = new DataContractSerializer(obj.GetType());

                var settings = new XmlWriterSettings { Indent = true };
                using (var w = XmlWriter.Create(memoryStream, settings))
                    serializer.WriteObject(w, obj);

                //serializer.WriteObject(memoryStream, obj);
                memoryStream.Position = 0;

                return new StreamReader(memoryStream).ReadToEnd();
            }
        }

        //TODO: deal with UTF16?
        public static object Deserialize(string xml, Type toType)
        {
            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(memoryStream, new XmlDictionaryReaderQuotas());
                DataContractSerializer serializer = new DataContractSerializer(toType);
                return serializer.ReadObject(reader);
            }
        }

    }
}