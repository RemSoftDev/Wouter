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
    public class Serializer
    {
        public string Name;
        public string Key;
        private Dictionary<String, String> values = new Dictionary<String, String>();
        private List<Serializer> contents = new List<Serializer>();
        public Serializer(string Name, string Key) { this.Name = Name; this.Key = Key; }
        public string Read(string Field) { if (values.ContainsKey(Field)) return values[Field]; return null;}
        public void Read(string Field, ref DateTime value) { if (values.ContainsKey(Field)) value = DateTime.Parse(values[Field]); }
        public void Read(string Field, ref Single value) { if (values.ContainsKey(Field)) value = Single.Parse(values[Field]); }
        public void WriteDate(string Field, DateTime value) { values[Field] = value.ToString("yyyy/MM/dd"); }
        public Serializer Write(string Field, string value) { values[Field] = value; return this; }
        public Serializer Add(string Name, string Value) { var child = new Serializer(Name, Value); contents.Add(child); return child; }

        public IEnumerable<Serializer> Select(string Name)
        {
            foreach (Serializer child in contents)
                if (child.Name == Name)
                    yield return child;
        }

        public string GetXML()
        {
            var s = new StringBuilder();
            var x = new XmlWriterSettings();
            x.Indent = true;
            x.OmitXmlDeclaration = true;
            var w = XmlWriter.Create(s, x);
            WriteXMLContents(w);
            w.Flush();
            x.Clone();
            return s.ToString();
        }

        private void WriteXMLContents(XmlWriter w)
        {
            w.WriteStartElement(Name);
            w.WriteAttributeString("Key", Key);
            foreach (var e in values)
				if (e.Value != null)
	                w.WriteAttributeString(e.Key, e.Value);
            foreach (var c in contents)
                c.WriteXMLContents(w);
            w.WriteEndElement();
        }

        public static Serializer FromXML(string Xml)
        {
            var r = XmlReader.Create(new System.IO.StringReader(Xml));
            return ReadOne(r);
        }

        private static Serializer ReadOne(XmlReader r)
        {
            if (r.NodeType != XmlNodeType.Element)
                if (r.MoveToContent() != XmlNodeType.Element)
                    return null;
            Serializer result = new Serializer(r.Name, r.GetAttribute("Key"));
            bool IsEmpty = r.IsEmptyElement;
            while (r.MoveToNextAttribute())
                if (r.Name != "Key")
                    result.values.Add(r.Name, r.Value);
            if (!IsEmpty)
                while (r.Read())
                    if (r.NodeType == XmlNodeType.Element)
                    {
                        Serializer child = ReadOne(r);
                        if (child != null)
                            result.contents.Add(child);
                    }
                    else if (r.NodeType == XmlNodeType.EndElement)
                        break;
            return result;
        }
    }
}