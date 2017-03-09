#if WINDOWS_PHONE
using FoodJournal.Logging;
using FoodJournal.Messages;
using FoodJournal.Model;
using FoodJournal.Parsing;
using FoodJournal.Search;
using FoodJournal.Values;
using FoodJournal.ViewModels;
#if WINDOWS_PHONE
using Newtonsoft.Json.Linq;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FoodJournal.ViewModels
{

    public abstract class SearchWorkerService : SearchWorker
    {

        public const int MaxAndResultCountForServiceSearch = 5; // if we already have 5 results, including all search terms, don't do service requests

        public SearchWorkerService(SearchVM SearchVM) : base(SearchVM) { this.HasAsyncRequest = true; }

        private bool ServiceCallNeeded { get { return AndResultCount < MaxAndResultCountForServiceSearch; } }

        internal override void RunQueryI(string Query, List<SearchResultVM> results, bool IsAsync)
        {

            if (!IsAsync) return;

            if (ShouldAbort) return;
            System.Threading.Thread.Sleep(500);

            if (ShouldAbort) return;
            if (!ServiceCallNeeded) return;

            SessionLog.RecordTraceValue("Service Query", this.ToString(), Query);
            RunQueryS(Query, results);

        }

        internal abstract void RunQueryS(string Query, List<SearchResultVM> results);

    }

    public class SearchResultNutritionIxVM : SearchResultVM
    {

        #region Property Map

        public static Dictionary<string, StandardProperty> map = new Dictionary<string, StandardProperty>(){
                {"nf_calories",StandardProperty.Calories}, 
                {"nf_protein",StandardProperty.Protein}, 
                {"nf_total_fat",StandardProperty.TotalFat}, 
                {"nf_total_carbohydrate",StandardProperty.Carbs}, 
                {"nf_dietary_fiber",StandardProperty.Fiber},
                {"nf_sugars",StandardProperty.Sugar}, 
                {"nf_calcium_dv", StandardProperty.Calcium},  // TODO: needs fix
                {"nf_iron_dv", StandardProperty.Iron}, 
                //{"Potassium_(mg)","Potassium"}, 
                {"nf_sodium",StandardProperty.Sodium},
                //{"Vit_D_µg", "Vitamin D"}, 
                {"nf_vitamin_c_dv", StandardProperty.VitaminC}, 
                {"nf_saturated_fat", StandardProperty.SatFat}, 
                {"nf_monounsaturated_fat",StandardProperty.MonoFat}, 
                {"nf_polyunsaturated_fat", StandardProperty.PolyFat},
                {"nf_cholesterol", StandardProperty.Cholesterol}};

        //"nf_water_grams", 
        //"nf_calories_from_fat",
        //"nf_trans_fatty_acid",
        //"nf_vitamin_a_dv",
        //"nf_refuse_pct",
        //"allergen_contains_milk",
        //"allergen_contains_eggs": null,
        //"allergen_contains_fish": null,
        //"allergen_contains_shellfish": null,
        //"allergen_contains_tree_nuts": null,
        //"allergen_contains_peanuts": null,
        //"allergen_contains_wheat": null,
        //"allergen_contains_soybeans": null,
        //"allergen_contains_gluten": null

        #endregion

        JToken token;
        public SearchResultNutritionIxVM(string Text, JToken token) : base(Text) { this.token = token; }

        public override FoodItem MakeItem()
        {

            FoodItem result = new FoodItem(Text, false);//, Text + " - Powered by Nutritionix API");

            string servingsize_qty = "";
            string servingsize_unit = "";
            string grams = "";

            foreach (var field in token.Children())
            {
                string column = ((JProperty)field).Name;

                if (((JProperty)field).Value.Type != JTokenType.Null)
                {

                    string value = ((JProperty)field).Value.ToString();
                    if (map.ContainsKey(column))
                    {
                        result.Values[map[column]] = Floats.ParseStorage(value.Replace(",", "."));
                    }
                    switch (column)
                    {
                        //case "item_id": result.ServiceId = value; break;
                        //case "nf_ingredient_statement": result.Description = value; break;
                        //case "nf_servings_per_container": result.ServingsPerContainer = value; break;

                        case "nf_serving_size_qty": servingsize_qty = value; break;
                        case "nf_serving_size_unit": servingsize_unit = value; break;
                        case "nf_serving_weight_grams": grams = value; break;

                    }
                }
            }

            string servingsize;
            servingsize = string.Format("{0} {1}", servingsize_qty, servingsize_unit);

            Single divamounts = 0;

            if (servingsize_unit == "g" || servingsize_unit == "gram" || servingsize_unit == "grams")
            {
                grams = null;
                divamounts = (Amount)servingsize / "100g";
            }

            if (grams != null)
            {
                result.ServingSizes.Add(servingsize, grams + " g");
                divamounts = (Amount)(grams + " g") / "100g";
            }
            else
                result.ServingSizes.Add(servingsize);

            if (divamounts > 0 && divamounts != 1)
                result.Values.DivideAllValues(divamounts);

            return result;
        }

    }

    /// <summary>
    /// Items from the Resource DB
    /// </summary>
    public class SearchWorkerNutritionIx : SearchWorkerService
    {

        public SearchWorkerNutritionIx(SearchVM SearchVM) : base(SearchVM) { }

        internal override void RunQueryS(string Query, List<SearchResultVM> results)
        {

            DateTime start = DateTime.Now;
            string url = "https://api.nutritionix.com:443/v1_1/search/" + Query + "?results=0%3A20&fields=*&appId=54c51034&appKey=c72005c34b95b456fe067e129e4be11e";
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += (e, a) =>
            {

                try
                {

                    if (a.Error != null) { LittleWatson.ReportException(a.Error); return; }

                    JObject o = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(a.Result);

                    foreach (var x in o["hits"])
                    {

                        try
                        {
                            string text = x["fields"]["item_name"].ToString() + " (" + x["fields"]["brand_name"].ToString() + ")";
                            //if (this.IsMatch(text))
                            //int HitCount = searchVM.(terms, text);
                            //ResultVM result = new NutritionIxResultVM(text, x["fields"], HitCount);
                            results.Add(new SearchResultNutritionIxVM(text, x["fields"]));
                        }
                        catch (Exception ex) { LittleWatson.ReportException(ex); }
                    }

                    MessageQueue.Push(new ServiceSearchMessage("NutritionIX", Query, results.Count, (int)DateTime.Now.Subtract(start).TotalMilliseconds, results, a.Result));

                    this.IsDone = true;
                    ReportProgress(results);

                }
                catch (Exception ex)
                {
                    if (ex.Message != "The remote server returned an error: NotFound.")
                        LittleWatson.ReportException(ex);
                }

            };
            wc.DownloadStringAsync(new Uri(url));

        }
    }

}

#endif