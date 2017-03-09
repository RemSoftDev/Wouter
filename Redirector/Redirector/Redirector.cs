using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace Redirector
{

    [DataContract]
    public class RedirectorSettings
    {
        [DataMember]
        public RedirectorSetting[] Settings;
    }

    [DataContract]
    public class RedirectorSetting
    {

        [DataMember]
        public string Mask;

        [DataMember]
        public string URL;

    }

    public class Settings
    {

        public static RedirectorSetting[] settings;
        public static DateTime LastLoaded = DateTime.MinValue;

        public static void Load(string xml)
        {

#if false 
            RedirectorSettings s = new RedirectorSettings();
            s.Settings = new RedirectorSetting[] { new RedirectorSetting() { Mask = "Platform.W", URL = "http://windowsphone.com/s?appid=2f44a06e-3d7c-4e11-b74d-9135949a1889" },
                                                   new RedirectorSetting() { Mask = "Platform.A", URL = "https://play.google.com/store/apps/details?id=app.dailybits.foodjournal" },
                                                   new RedirectorSetting() { Mask = ".", URL = "http://windowsphone.com/s?appid=2f44a06e-3d7c-4e11-b74d-9135949a1889" }
                                                  };
            xml = Serialize(s);
            System.Diagnostics.Debug.WriteLine(xml);
#endif

            try
            {
                settings = ((RedirectorSettings)Deserialize(xml, typeof(RedirectorSettings))).Settings;

                LastLoaded = DateTime.Now;

            }
            catch { }

        }

        public static string GetSettingsRedirect(string info, string redirect)
        {

            try
            {
                if (settings != null)
                {
                    foreach (var set in settings)
                        if (Regex.IsMatch(info, set.Mask))
                        {
                            redirect = set.URL;
                            break;
                        }
                }
            }
            catch { }

            return redirect;

        }

        //TODO: deal with UTF16?
        private static string Serialize(object obj)
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
        private static object Deserialize(string xml, Type toType)
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