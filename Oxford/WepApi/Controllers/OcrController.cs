using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WepApi.Controllers
{
    [Route("api/[controller]")]
    public class OcrController : Controller
    {
        // GET api/ocr
        //[HttpGet]
        //public IEnumerable<string> GetWords(string url)
        //{
        //    List<string> resultWords = new List<string>();
        //    if (string.IsNullOrWhiteSpace(url)) return resultWords;
        //    string result = OcrProgram.MakeAnalysisRequest(url);
        //    resultWords = OcrProgram.ExtractWords(result);
        //    return resultWords;
        //}

        //// GET api/ocr/url
        //[HttpGet("{url}")]
        //public string GetText(string url)
        //{
        //    if (string.IsNullOrWhiteSpace(url)) return "url cannot be empty";
        //    string result = OcrProgram.MakeAnalysisRequest(url);
        //    List<string> resultWords = OcrProgram.ExtractWords(result);
        //    string words = OcrProgram.DisplayWords(resultWords);
        //    return words;
        //}

        [HttpGet]
        public string GetText(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return "url cannot be empty";
            string result = OxfordOCR.OcrProgram.MakeAnalysisRequest(url);
            if (result == "Bad Request") throw new ApplicationException("Bad request");

            List<string> resultWords = OxfordOCR.OcrProgram.ExtractWords(result);
            string fullText = OxfordOCR.OcrProgram.DisplayWords(resultWords);
            if(string.IsNullOrWhiteSpace(fullText)) throw new ApplicationException("No text on image was recognized");
            return fullText;
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
