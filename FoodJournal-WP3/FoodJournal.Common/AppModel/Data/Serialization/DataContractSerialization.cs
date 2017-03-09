using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FoodJournal.WinPhone.Common.AppModel.Data.Serialization
{
    public static class DataContractSerialization
    {
        public static string Serialize(object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                DataContractSerializer serializer = new DataContractSerializer(obj.GetType());

                var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
                using (var w = XmlWriter.Create(memoryStream, settings))
                    serializer.WriteObject(w, obj);
                memoryStream.Position = 0;
                return new StreamReader(memoryStream).ReadToEnd();
            }
        }

        public static object Deserialize(string xml, Type toType)
        {
            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(memoryStream, new XmlDictionaryReaderQuotas());
                DataContractSerializer serializer = new DataContractSerializer(toType);
                return serializer.ReadObject(reader);
            }
        }

        public static bool IsObjectTreeIdentical(object a, object b)
        {
			#if DEBUG
			var aa = Serialize(a);
			var bb = Serialize(b);
			#endif

            return Serialize(a) == Serialize(b);
        }
    }
}