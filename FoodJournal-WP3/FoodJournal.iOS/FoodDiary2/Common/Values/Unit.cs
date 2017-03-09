using FoodJournal.Parsing;
using FoodJournal.WinPhone.Common.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoodJournal.Values
{
    public enum UnitKind
    {
        StandardVolume,
        MetricVolume,
        StandardWeight,
        MetricWeight
    }

    public class Unit
    {

        public UnitKind UnitKind;
        public string Key;
        public string LocalizedText;
        public string LocalizedMulti;
        public string Short;
        public string Other;
        public Single BaseUnits;

        private Unit() { }

        private void fix()
        {
            LocalizedText = Strings.FromEnum("Unit", Key).ToLower();
            LocalizedMulti = Strings.FromEnum("Units", Key).ToLower();
            //if (LocalizedMulti == null) LocalizedMulti = StringsExtension.GetPlural(LocalizedText);
        }

        #region Standard Units

        private static Unit gram;
        public static Unit Gram { get { if (gram == null) gram = Parse("g"); return gram; } }

        private static List<Unit> standardUnits;
        public static List<Unit> StandardUnits
        {
            get
            {
                if (standardUnits == null)
                {
                    standardUnits = new List<Unit>();

                    standardUnits.Add(new Unit() { UnitKind = UnitKind.MetricWeight, Key = "Gram", Short = "g", Other = "gr", BaseUnits = 1 /* g */ });
                    standardUnits.Add(new Unit() { UnitKind = UnitKind.MetricWeight, Key = "Kilogram", Short = "kg", BaseUnits = 1000 /* g */ });
                    standardUnits.Add(new Unit() { UnitKind = UnitKind.StandardWeight, Key = "Pound", Short = "lb", BaseUnits = 16 /* oz */ });
                    standardUnits.Add(new Unit() { UnitKind = UnitKind.StandardWeight, Key = "Ounce", Short = "oz", BaseUnits = 1 /* oz */ });

                    standardUnits.Add(new Unit() { UnitKind = UnitKind.StandardVolume, Key =  "Gallon", BaseUnits = 3 * 16 * 4 * 4 /* tsp */ });
                    standardUnits.Add(new Unit() { UnitKind = UnitKind.StandardVolume, Key =  "Quart", BaseUnits = 3 * 16 * 4 /* tsp */ });
                    standardUnits.Add(new Unit() { UnitKind = UnitKind.StandardVolume, Key =  "Pint", Short = "pint", BaseUnits = 3 * 16 * 2 /* tsp */ });
                    standardUnits.Add(new Unit() { UnitKind = UnitKind.StandardVolume, Key =  "Cup", Short = "cup", BaseUnits = 3 * 16  /* tsp */ });
                    standardUnits.Add(new Unit() { UnitKind = UnitKind.StandardVolume, Key =  "FluidOunce", Short = "fl oz", BaseUnits = 3 * 2  /* tsp */ });
                    standardUnits.Add(new Unit() { UnitKind = UnitKind.StandardVolume, Key =  "Tablespoon", Short = "tbsp", BaseUnits = 3 /* tsp */ });
                    standardUnits.Add(new Unit() { UnitKind = UnitKind.StandardVolume, Key =  "Teaspoon", Short = "tsp", BaseUnits = 1 /* tsp */ });

                    standardUnits.Add(new Unit() { UnitKind = UnitKind.MetricVolume, Key =  "Liter", Short = "l", BaseUnits = 1000 /* ml */ });
                    standardUnits.Add(new Unit() { UnitKind = UnitKind.MetricVolume, Key =  "Deciliter", Short = "dl", BaseUnits = 100 /* ml */ });
                    standardUnits.Add(new Unit() { UnitKind = UnitKind.MetricVolume, Key =  "Centiliter", Short = "cl", BaseUnits = 10 /* ml */ });
                    standardUnits.Add(new Unit() { UnitKind = UnitKind.MetricVolume, Key =  "Milliliter", Short = "ml", BaseUnits = 1 /* ml */ });

                    foreach (var u in standardUnits) u.fix();

                }
                return standardUnits;
            }
        }

        #endregion

        public static Single operator /(Unit unit1, Unit unit2)
        {
            if (unit1.UnitKind == unit2.UnitKind) return unit1.BaseUnits / unit2.BaseUnits;

            if (unit1.IsVolume && unit2.IsVolume)
            {
                if (unit1.UnitKind == UnitKind.MetricVolume)
                    return (Single)(unit1.BaseUnits / unit2.BaseUnits / MetricVolumeBaseOverStandardVolumeBase);
                else
                    return (Single)(unit1.BaseUnits / unit2.BaseUnits * MetricVolumeBaseOverStandardVolumeBase);
            }

            if (unit1.IsWeight && unit2.IsWeight)
            {
                if (unit1.UnitKind == UnitKind.MetricWeight)
                    return (Single)(unit1.BaseUnits / unit2.BaseUnits / MetricWeightBaseOverStandardWeightBase);
                else
                    return (Single)(unit1.BaseUnits / unit2.BaseUnits * MetricWeightBaseOverStandardWeightBase);
            }

            return 0; // cannot convert these units
        }

        private const double MetricVolumeBaseOverStandardVolumeBase = 4.92892; // ml / tsp
        private const double MetricWeightBaseOverStandardWeightBase = 28.3495; // g / oz

        /// <summary>
        /// Parse a string to return the Standard Unit it represents
        /// Supports Text, Multi and Short version of the unit
        /// </summary>
        /// <returns>null if no standard unit is found</returns>
        public static Unit Parse(string text)
        {
            if (text == null) return null;

            foreach (Unit option in StandardUnits)
            {
                if (string.Compare(text, option.LocalizedText, StringComparison.InvariantCultureIgnoreCase) == 0) return option;
                if (string.Compare(text, option.LocalizedMulti, StringComparison.InvariantCultureIgnoreCase) == 0) return option;
                if (string.Compare(text, option.Short, StringComparison.InvariantCultureIgnoreCase) == 0) return option;
                if (string.Compare(text, option.Other, StringComparison.InvariantCultureIgnoreCase) == 0) return option;
            }
            return null;
        }

        public string ToString(Single value, bool round)
        {

            // TODO: perhaps this should implement Uncide CLDR rules for other cultures
            if (round)
            {
                if (value > 0 && value <= 1)
                    return string.Format("{0} {1}", Floats.ToUIString(value), LocalizedText);
                return string.Format("{0} {1}", Floats.ToUIString(value), LocalizedMulti);
            }
            else
            {
                if (value > 0 && value <= 1)
                    return string.Format("{0} {1}", Floats.ToStorageString(value), LocalizedText);
                return string.Format("{0} {1}", Floats.ToStorageString(value), LocalizedMulti);
            }
        }

        public bool IsVolume { get { return (UnitKind == Values.UnitKind.MetricVolume || UnitKind == Values.UnitKind.StandardVolume); } }
        public bool IsWeight { get { return (UnitKind == Values.UnitKind.MetricWeight || UnitKind == Values.UnitKind.StandardWeight); } }
        
        public override string ToString()
        {
            return LocalizedText;
        }

    }

}
