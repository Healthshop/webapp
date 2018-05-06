using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLUclient;
using RankingAndRelevance;

namespace WepApi.Controllers
{
    [EnableCors("CorsPolicy")]
    [Route("api/[controller]")]
    public class OcrController : Controller
    {
        public static List<RankingAndRelevance.Provider> providers = Ranker.ReadProviders("Provider.tsv"); //191 provider
        public static Dictionary<string, double[]> cuisDictionary = Ranker.LoadData("Filtered.csv");
        public static Dictionary<string, CuiEntities> cuiProviderDictionary = LoadProviderCuiEntities();

        public static Dictionary<string, CuiEntities> LoadProviderCuiEntities()
        {
            if (cuiProviderDictionary != null && cuiProviderDictionary.Count > 0) return cuiProviderDictionary;
            Dictionary<string, CuiEntities> cuiEntities = new Dictionary<string, CuiEntities>();
            foreach (Provider provider in providers)
            {
                NLUclient.CuiEntities providerCuiEntities = NuClient.ExtractCuiEntities(provider.Keywords);
                cuiEntities.Add(provider.PrvIdn, providerCuiEntities);
            }
            return cuiEntities;
        }

        //[HttpGet]
        //public string GetText(string url)
        //{
        //    if (string.IsNullOrWhiteSpace(url)) return "url cannot be empty";
        //    string result = OxfordOCR.OcrProgram.MakeAnalysisRequest(url);
        //    if (result == "Bad Request") throw new ApplicationException("Bad request");

        //    if (string.IsNullOrWhiteSpace(result))
        //    {
        //        throw new ApplicationException("No text on image was recognized");
        //    }

        //    List<string> resultWords = OxfordOCR.OcrProgram.ExtractWords(result);
        //    string patientDescriptionInputText = OxfordOCR.OcrProgram.DisplayWords(resultWords);

        //    string providerDescriptionInputText = "Small-incision phacoemulsification cataract surgery, minor in-office procedures including small eyelid anomalies and chalazion excisions and laser surgical procedures for secondary cataracts, glaucoma and diabetic complications Special Interests General ophthalmology including cataract surgery, glaucoma and diabetic care.";
        //    CuiEntities providerCuiEntities = NuClient.ExtractCuiEntities(providerDescriptionInputText);
        //    CuiEntities patientCuiEntities = NuClient.ExtractCuiEntities(patientDescriptionInputText);
        //    var similarities = Ranker.ExtractSimilarities(providerCuiEntities, patientCuiEntities, cuisDictionary);

        //    string json = JsonConvert.SerializeObject(providers, Formatting.Indented);
        //    Request.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        //    return json;
        //}

        [HttpGet]
        public string GetText(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return "url cannot be empty";
            string result = OxfordOCR.OcrProgram.MakeAnalysisRequest(url);
            if (result == "Bad Request") throw new ApplicationException("Bad request");

            if (string.IsNullOrWhiteSpace(result))
            {
                throw new ApplicationException("No text on image was recognized");
            }

            List<string> resultWords = OxfordOCR.OcrProgram.ExtractWords(result);
            string patientDescriptionInputText = OxfordOCR.OcrProgram.DisplayWords(resultWords);

            NLUclient.CuiEntities patientCuiEntities = NuClient.ExtractCuiEntities(patientDescriptionInputText);

            //Parallel.ForEach(providers, provider =>
            foreach (KeyValuePair<string, CuiEntities> providerCuiEntities in cuiProviderDictionary)
            {
                try
                {
                    //NLUclient.CuiEntities providerCuiEntities = NuClient.ExtractCuiEntities(provider.Keywords);
                    var similar = RankingAndRelevance.Ranker.ExtractSimilarities(providerCuiEntities.Value, patientCuiEntities, cuisDictionary);
                    List<double> avg = new List<double>();
                    List<string> matches = new List<string>();
                    foreach (var s in similar)
                    {
                        avg.Add(s.Rank);
                        matches.Add($"{s.ProviderSurfaceForm} : {s.PatientSurfaceForm}");
                    }
                    var provider = providers.Find(p => p.PrvIdn == providerCuiEntities.Key);
                    provider.Matches = matches;
                    if (avg.Count > 0)
                    {
                        double providerAvgCosineRank = avg.Average();
                        provider.AverageMatchRank = providerAvgCosineRank;
                    }
                    else
                    {
                        provider.AverageMatchRank = 0;
                    }
                    provider.Distance = NuClient.ExtractZipCode("98004", provider.ProviderZip).text;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            //});

            providers.Sort();
            providers.Reverse();
            string json = JsonConvert.SerializeObject(providers, Formatting.Indented);
            Request.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return json;
        }
    }
}
