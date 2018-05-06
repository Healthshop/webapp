using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLUclient;
using RankingAndRelevance;

namespace WepApi.Controllers
{
    [Route("api/[controller]")]
    public class OcrController : Controller
    {
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

            List<Provider> providers = Ranker.ReadProviders("Provider.tsv"); //191 provider
            List<string> resultWords = OxfordOCR.OcrProgram.ExtractWords(result);

            string patientDescriptionInputText = OxfordOCR.OcrProgram.DisplayWords(resultWords);

            string providerDescriptionInputText = "Small-incision phacoemulsification cataract surgery, minor in-office procedures including small eyelid anomalies and chalazion excisions and laser surgical procedures for secondary cataracts, glaucoma and diabetic complications Special Interests General ophthalmology including cataract surgery, glaucoma and diabetic care.";
            CuiEntities providerCuiEntities = NuClient.ExtractCuiEntities(providerDescriptionInputText);
            CuiEntities patientCuiEntities = NuClient.ExtractCuiEntities(patientDescriptionInputText);

            string json = JsonConvert.SerializeObject(providers, Formatting.Indented);
            return json;
        }

        // POST api/ocr
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/ocr/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/ocr/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
