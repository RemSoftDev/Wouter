using FoodJournal.Model;
using FoodJournal.Model.Data;
using FoodJournal.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoodJournal.Values
{

    public interface IPropertyTarget
    {
        string NutritionDB { get; set; }
    }

    #region class PropertyComparer

    public class PropertyComparer : IEqualityComparer<Property>
    {

        public bool Equals(Property x, Property y)
        {
            return x == y;
        }

        public int GetHashCode(Property obj)
        {
            return obj.GetHashCode();
        }
    }

    #endregion

    public class PropertyDictionary
    {

        private FoodItem item = null;
        private IPropertyTarget data = null;
        private Dictionary<Property, Single> values = new Dictionary<Property, float>(new PropertyComparer());

        public PropertyDictionary(string value) { Read(value); }
        public PropertyDictionary(FoodItem item, IPropertyTarget data) { this.item = item; this.data = data; Read(data.NutritionDB); }

        public Single this[Property property]
        {
            get
            {
                if (!values.ContainsKey(property)) return 0;
                return values[property];
            }
            set
            {
                if (Single.IsNaN(value))
                {
                    if (!values.ContainsKey(property)) return;
                    values.Remove(property);
                }
                else
                    values[property] = value;
                if (item != null)
                {
                    data.NutritionDB = ToString();
                    item.OnPropertyChanged("Nutrition");
                }
            }
        }

        private void Read(string value)
        {
            if (value == null || value.Length == 0) return;
            var x = value.Split(new char[] { '|' });
            for (int i = 0; i < x.Length; i++)
                if (x[i].Length > 2)
                    values[Property.GetProperty(x[i].Substring(0, 2))] = Floats.ParseStorage(x[i].Substring(2));
        }

        public void DivideAllValues(Single div)
        {
            for (int i = 0; i < values.Count; i++)
                values[values.ElementAt(i).Key] = values.ElementAt(i).Value / div;
        }

        public override string ToString()
        {
            var s = new StringBuilder();
            var b = false;
            foreach (var x in values)
            {
                if (b) { s.Append("|"); } else { b = true; }
                s.Append(x.Key.ID);
                s.Append(Floats.ToStorageString(x.Value));
            }
            return s.ToString();
        }

    }

}
