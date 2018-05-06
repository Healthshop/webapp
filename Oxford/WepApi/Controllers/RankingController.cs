using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace WepApi.Controllers
{
    [Route("api/[controller]")]
    public class RankingController : Controller
    {
        [HttpGet]
        public string GetRanking(string keywords)
        {
            if (string.IsNullOrWhiteSpace(keywords)) return "keywords cannot be empty";
            string result = OxfordOCR.OcrProgram.MakeAnalysisRequest(keywords);
            if (result == "Bad Request") throw new ApplicationException("Bad request");

            List<string> resultWords = OxfordOCR.OcrProgram.ExtractWords(result);
            string fullText = OxfordOCR.OcrProgram.DisplayWords(resultWords);
            if(string.IsNullOrWhiteSpace(fullText)) throw new ApplicationException("No text on image was recognized");
            return fullText;
        }
    }
}
