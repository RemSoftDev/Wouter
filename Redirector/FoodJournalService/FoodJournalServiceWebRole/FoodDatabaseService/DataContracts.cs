using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace FoodJournalServiceWebRole.FoodDatabaseService
{

    [DataContract]
    public class QueryResponse
    {
        [DataMember]
        public int ServerTime;

        [DataMember]
        public FoodItem[] Results;

    }

    [DataContract]
    public class FoodItem
    {

        // Search Results:
        // FoodID	Culture	Name	LongName	Meal	Pos
        // (common names)

        // Datapoints:
        // SourceID PivotAmount	
        // Energ_Kcal	Protein_(g)	Lipid_Tot_(g)	Carbohydrt_(g)	Fiber_TD_(g)	Sugar_Tot_(g)	Calcium_(mg)	Iron_(mg)	Potassium_(mg)	Sodium_(mg)	Vit_D_microg	Vit_C_(mg)	FA_Sat_(g)	FA_Mono_(g)	FA_Poly_(g)	Cholestrl_(mg)	
        // AllWeights

        [DataMember]
        public string FoodId;
        [DataMember]
        public string Source;
        [DataMember]
        public string Culture;
        [DataMember]
        public string Name;

        //public float QualityRating;  // lower for translated items, lower for item with fewer datapoints, lower with fewer weights

        [DataMember(EmitDefaultValue = false)]
        public string SourceCulture;
        [DataMember(EmitDefaultValue = false)]
        public string Description;
        [DataMember(EmitDefaultValue = false)]
        public string LongName;
        [DataMember(EmitDefaultValue = false)]
        public string AlternativeNames;
        [DataMember(EmitDefaultValue = false)]
        public string Brand;
        [DataMember(EmitDefaultValue = false)]
        public string Restaurant;
        [DataMember(EmitDefaultValue = false)]
        public string Comment;
        [DataMember(EmitDefaultValue = false)]
        public string[] Pictures;

        // DataPoints
        [DataMember(EmitDefaultValue = false)]
        public string Meal;
        [DataMember(EmitDefaultValue = false)]
        public string[] Categories;

        [DataMember]
        public string SourceID;
        [DataMember]
        public string PivotAmount;
        [DataMember]
        public string Nutrition;
        [DataMember]
        public string AllWeights;

    }

}