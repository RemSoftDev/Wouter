using FoodJournal.Logging;
using FoodJournal.WinPhone.Common.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FoodJournal.Resources;


namespace FoodJournal.Values
{

    public enum StandardProperty
    {
        none,

        Amount,
        Weight,

        Calories,
        Protein,    // g
        TotalFat,   // g
        Carbs,      // g
        Fiber,      // g
        Sugar,      // g
        SatFat,     // g
        MonoFat,    // g
        PolyFat,    // g
        Calcium,    // mg
        Iron,       // mg
        Potassium,  // mg
        Sodium,     // mg
        Cholesterol,// mg
        VitaminC,   // mg 
        VitaminD    // µg
    }

    public class Property
    {

        private const int FIRSTCUSTOMID = 50;

        private static Dictionary<string, Property> propertyIndex;

#if DEBUG
        public static void Reset() { propertyIndex = null; }// used for screenshots
#endif

        private static Dictionary<string, Property> PropertyIndex
        {
            get
            {
                if (propertyIndex == null)
                {
                    propertyIndex = new Dictionary<String, Property>();
                    int i;
                    for (i = 0; i <= (int)StandardProperty.VitaminD; i++)
                    {
                        Property value = new Property((StandardProperty)i);
                        propertyIndex.Add(value.ID, value);
                    }
                    //i = 0;
                    //foreach (string cstm in App.CustomProperties)
                    //{
                    //    Property value = Property.Parse(cstm, i);
                    //    propertyIndex.Add(value.ID, value);
                    //    i++;
                    //}
                }
                return propertyIndex;
            }
        }

        public static Property GetProperty(string ID)
        {
            return PropertyIndex[ID];
        }

        public static IEnumerable<Property> All()
        {
            for (var i = StandardProperty.Weight; i <= StandardProperty.VitaminD; i++)
                yield return i;
        }

        // weight, volume, [nutrition properties], time
        // translated

        public string ID;

        //private StandardProperty standard;

        private string LowerText;   // sodium
        private String Text;        // Sodium
        private String Unit;        // mg

        // fulltext  = lowertext (unit) or lowertext (if there is no unit, like with calories)
        // fullcapit = text (unit)      or text
        // extension = unit lowertext   or lowertext

        public Property(string Text, string Unit, int customID) { this.Text = Text; this.LowerText = Text.ToLower(); this.Unit = Unit; ID = GetID(customID); }
        public Property(StandardProperty property)
        {
            //this.standard = property;
            if (property == StandardProperty.none)
                Text = AppResources.None;
            else
                Text = Strings.FromEnum(property);
            LowerText = Text.ToLower();
            if (property >= StandardProperty.Protein) Unit = AppResources.Unit_Gram.ToLower();
            if (property >= StandardProperty.Calcium) Unit = "mg";
            if (property >= StandardProperty.VitaminD) Unit = "µg";
            ID = GetID(property);
        }

        private static string GetID(int customID) { return (FIRSTCUSTOMID + customID).ToString().PadLeft(2, '0'); }
        private static string GetID(StandardProperty property) { return ((int)property).ToString().PadLeft(2, '0'); }

        public string FormatValue(string value)
        {
            return string.Format("{0} {1}", value, Extension);
        }

        public string TextOnly
        {
            get
            {
                return string.Format("{0}", Text);
            }
        }

        public string FullText
        {
            get
            {
                if (null != Unit && Unit.Length > 0)
                    return string.Format("{0} ({1})", Text, Unit);
                return string.Format("{0}", Text);
            }
        }

        public string FullCapitalizedText
        {
            get
            {
                if (null != Unit && Unit.Length > 0)
                    return string.Format("{0} ({1})", Text, Unit);
                return string.Format("{0}", Text);
            }
        }

        public string Extension
        {
            get
            {
                if (null != Unit && Unit.Length > 0)
                    return string.Format("{1} {0}", LowerText, Unit);
                return string.Format("{0}", LowerText);
            }
        }

        public static bool operator ==(Property val1, Property val2) { if (Object.ReferenceEquals(val1, val2)) return true; return val1.ID == val2.ID; }
        public static bool operator !=(Property val1, Property val2) { if (Object.ReferenceEquals(val1, val2)) return false; return val1.ID != val2.ID; }
        public override bool Equals(object obj) { return this == (Property)obj; }
        public override int GetHashCode() { return ID.GetHashCode(); }

        public static implicit operator Property(StandardProperty value)
        {
            return PropertyIndex[GetID(value)];
        }

        private static Property Parse(string text, int customId)
        {

            string unit = "";

            // see if text starts with a single value
            int pos = text.IndexOf("(");
            if (pos > 0)
            {
                unit = text.Substring(pos + 1).Replace(")", "");
                text = text.Substring(0, pos).Trim();
            }

            return new Property(text, unit, customId);
        }

        public override string ToString()
        {
            return FullText;
        }


        /// <summary>
        /// Need to add logic to calclate color here,
        /// problem with AndroidUI.GetPropertyColor() is that it need index
        /// </summary>
        /// <value>The color.</value>
//        public Color Color
//        {
//            get;
//            set;
//        }
        //private Color GetColor(StandardProperty property)
        //{
        //    Color _color;
        //    switch (property)
        //    {
        //        case StandardProperty.Calories: _color = Color.FromArgb(255, 241, 162, 22); break;// F1A216
        //        case StandardProperty.SatFat:
        //        case StandardProperty.Fiber:
        //            //case "fruit": 
        //            _color = Color.FromArgb(255, 255, 0, 64); break;// FF0040
        //        case StandardProperty.Protein:
        //            _color = Color.FromArgb(255, 133, 38, 162); break;// 8526A2
        //        //case "vegetable": _color = Color.FromArgb(255, 122, 206, 48); break;// 7ACE30
        //        //case "grains": _color = Color.FromArgb(255, 241, 162, 22); break;// F1A216
        //        //case "dairy":
        //        //case "sodium": _color = Color.FromArgb(255, 72, 178, 178); break;
        //        case StandardProperty.Sodium:
        //            _color = Color.FromArgb(255, 38, 133, 162); break;// 2685A2
        //        default:
        //            _color = Color.FromArgb(255, 255, 0, 64); break;
        //    }
        //    return _color;
        //}

    }
}



