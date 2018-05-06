using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public Dictionary<string, double[]> cuisDictionary = Ranker.LoadData("Filtered.csv");

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

            Dictionary<RankingAndRelevance.Similarity, RankingAndRelevance.Provider> cosineSimilarites = 
                new Dictionary<RankingAndRelevance.Similarity, RankingAndRelevance.Provider>();

            NLUclient.CuiEntities patientCuiEntities = NuClient.ExtractCuiEntities(patientDescriptionInputText);
            foreach (RankingAndRelevance.Provider provider in providers)
            {
                try
                {
                    NLUclient.CuiEntities providerCuiEntities = NuClient.ExtractCuiEntities(provider.Keywords);
                    var similar = RankingAndRelevance.Ranker.ExtractSimilarities(providerCuiEntities, patientCuiEntities, cuisDictionary);
                    List<double> avg = new List<double>();
                    List<string> matches = new List<string>();
                    foreach (var s in similar)
                    {
                        avg.Add(s.Rank);
                        matches.Add($"{s.ProviderSurfaceForm} : {s.PatientSurfaceForm}");
                    }
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
            
            providers.Sort();
            providers.Reverse();
            string json = JsonConvert.SerializeObject(providers, Formatting.Indented);
            Request.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return json;
        }

        //[EnableCors("CorsPolicy")]
        //[HttpGet]
        //public string GetText2(string url2)
        //{
        //    if (string.IsNullOrWhiteSpace(url2)) return "url2 cannot be empty";
        //    string result = OxfordOCR.OcrProgram.MakeAnalysisRequest(url2);
        //    if (result == "Bad Request") throw new ApplicationException("Bad request");

        //    if (string.IsNullOrWhiteSpace(result))
        //    {
        //        throw new ApplicationException("No text on image was recognized");
        //    }
        //    List<string> resultWords = OxfordOCR.OcrProgram.ExtractWords(result);
        //    string patientDescriptionInputText = OxfordOCR.OcrProgram.DisplayWords(resultWords);

        //    List<RankingAndRelevance.Provider> providers = Ranker.ReadProviders("Provider.tsv"); //191 provider
        //    CuiEntities patientCuiEntities = NuClient.ExtractCuiEntities(patientDescriptionInputText);

        //    foreach (RankingAndRelevance.Provider provider in providers)
        //    {
        //        CuiEntities providerCuiEntities = NuClient.ExtractCuiEntities(provider.Keywords);
        //    }

        //    //string providerDescriptionInputText = "Small-incision phacoemulsification cataract surgery, minor in-office procedures including small eyelid anomalies and chalazion excisions and laser surgical procedures for secondary cataracts, glaucoma and diabetic complications Special Interests General ophthalmology including cataract surgery, glaucoma and diabetic care.";
        //    //CuiEntities providerCuiEntities = NuClient.ExtractCuiEntities(providerDescriptionInputText);
        //    //CuiEntities patientCuiEntities = NuClient.ExtractCuiEntities(patientDescriptionInputText);

        //    string json = JsonConvert.SerializeObject(providers, Formatting.Indented);
        //    return json;
        //}

        //// POST api/ocr
        //[HttpPost]
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT api/ocr/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/ocr/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
