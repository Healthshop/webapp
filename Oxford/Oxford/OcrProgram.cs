using System.Net;

namespace Oxford
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Runtime.Serialization.Json;

    namespace CSHttpClientSample
    {
        /// <summary>
        /// https://westcentralus.dev.cognitive.microsoft.com/docs/services/56f91f2d778daf23d8ec6739/operations/56f91f2e778daf14a499e1fa
        /// OCR technology detects text content in an image and extracts the identified text into a machine-readable character stream. 
        /// You can use the result for search and numerous other purposes like medical records, security, and banking. 
        /// It automatically detects the language. OCR saves time and 
        /// provides convenience for users by allowing them to take photos of text instead of transcribing the text.
        /// OCR supports 25 languages.These languages are: Arabic, Chinese Simplified, 
        /// Chinese Traditional, Czech, Danish, Dutch, English, Finnish, French, German, Greek, Hungarian, 
        /// Italian, Japanese, Korean, Norwegian, Polish, Portuguese, Romanian, Russian, 
        /// Serbian(Cyrillic and Latin), Slovak, Spanish, Swedish, and Turkish.
        /// If needed, OCR corrects the rotation of the recognized text, in degrees, 
        /// around the horizontal image axis.OCR provides the frame coordinates of each word as seen in below illustration. 
        /// Supported image formats: JPEG, PNG, GIF, BMP.Image file size must be less than 4MB
        /// </summary>
        public static class OcrProgram
        {
            // Replace the CognitiveSubscriptionKey string value with your valid subscription key.
            public const string CognitiveSubscriptionKey = "955338d6435f4263ac55686d2f3945e7";

            // Replace or verify the region.
            //
            // You must use the same region in your REST API call as you used to obtain your subscription keys.
            // For example, if you obtained your subscription keys from the westus region, replace 
            // "westcentralus" in the URI below with "westus".
            //
            // NOTE: Free trial subscription keys are generated in the westcentralus region, so if you are using
            // a free trial subscription key, you should not need to change this region.
            public const string OcrUriBase = "https://westus.api.cognitive.microsoft.com/vision/v1.0/ocr";
            public const string HandUriBase = "https://westus.api.cognitive.microsoft.com/vision/v1.0/hand";

            public static void Main()
            {
                // Get the path and filename to process from the user.
                Console.WriteLine("Analyze an image:");
                Console.Write("Enter the path to an image you wish to analyze: ");
                string imageFilePath = @"Untitled.png";

                // Execute the REST API call.
                string result = MakeAnalysisRequest(imageFilePath);

                List<string> resultWords = ExtractWords(result);
                DisplayWords(resultWords);
                Console.WriteLine();
                Console.WriteLine(JsonPrettyPrint(result));
                Console.ReadLine();
            }

            public static string DisplayWords(List<string> resultWords)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string word in resultWords)
                {
                    sb.Append($"{word} ");
                }
                Console.WriteLine(sb.ToString());
                return sb.ToString();
            }

            public static List<string> ExtractWords(string result)
            {
                OcrObject ocrObject = ReadToObject(result);
                List<string> resultWords = ExtractWords(ocrObject);

                return resultWords;
            }

            /// <summary>
            /// Gets the analysis of the specified image file by using the Computer Vision REST API.
            /// </summary>
            /// <param name="url">The image file.</param>
            public static string MakeAnalysisRequest(string url)
            {
                if (string.IsNullOrWhiteSpace(url)) return "";

                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", CognitiveSubscriptionKey);

                // Request parameters. A third optional parameter is "details".
                string requestParameters = "visualFeatures=Categories,Description,Color&details&language=en";

                // Assemble the URI for the REST API Call.
                string uri = OcrUriBase + "?" + requestParameters;

                HttpResponseMessage response;

                // Request body. Posts a locally stored JPEG image.
                byte[] byteData = GetImageAsByteArray(url);

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses content type "application/octet-stream".
                    // The other content types you can use are "application/json" and "multipart/form-data".
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    // Execute the REST API call.
                    response = client.PostAsync(uri, content).Result;
                    if (response.StatusCode == HttpStatusCode.BadRequest)
                        return response.ReasonPhrase;

                    // Get the JSON response.
                    string contentString = response.Content.ReadAsStringAsync().Result;
                    return contentString;
                }
            }

            public static List<string> ExtractWords(OcrObject ocrObject)
            {
                List<string> words = new List<string>();
                if (ocrObject.regions == null)
                    throw new Exception("OCR image result was not correct and no regions extracted.");

                foreach (Region region in ocrObject.regions)
                {
                    foreach (Line regionLine in region.lines)
                    {
                        foreach (Word regionLineWord in regionLine.words)
                        {
                            words.Add(regionLineWord.text);
                        }
                    }
                }
                return words;
            }

            // Deserialize a JSON stream to a User object.  
            public static OcrObject ReadToObject(string json)
            {
                OcrObject deserializedUser = new OcrObject();
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
                DataContractJsonSerializer ser = new DataContractJsonSerializer(deserializedUser.GetType());
                deserializedUser = ser.ReadObject(ms) as OcrObject;
                ms.Close();
                return deserializedUser;
            }

            /// <summary>
            /// Returns the contents of the specified file as a byte array.
            /// </summary>
            /// <param name="imageFilePath">The image file to read.</param>
            /// <returns>The byte array of the image data.</returns>
            static byte[] GetImageAsByteArray(string imageFilePath)
            {
                if (!File.Exists(imageFilePath))
                {
                    //Try download from the web
                    using (WebClient client = new WebClient())
                    {
                        byte[] pic = client.DownloadData(imageFilePath);
                        return pic;
                    }
                }
                FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }


            /// <summary>
            /// Formats the given JSON string by adding line breaks and indents.
            /// </summary>
            /// <param name="json">The raw JSON string to format.</param>
            /// <returns>The formatted JSON string.</returns>
            static string JsonPrettyPrint(string json)
            {
                if (string.IsNullOrEmpty(json))
                    return string.Empty;

                json = json.Replace(Environment.NewLine, "").Replace("\t", "");

                string INDENT_STRING = "    ";
                var indent = 0;
                var quoted = false;
                var sb = new StringBuilder();
                for (var i = 0; i < json.Length; i++)
                {
                    var ch = json[i];
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            if (!quoted)
                            {
                                sb.AppendLine();
                                Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
                            }
                            break;
                        case '}':
                        case ']':
                            if (!quoted)
                            {
                                sb.AppendLine();
                                Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
                            }
                            sb.Append(ch);
                            break;
                        case '"':
                            sb.Append(ch);
                            bool escaped = false;
                            var index = i;
                            while (index > 0 && json[--index] == '\\')
                                escaped = !escaped;
                            if (!escaped)
                                quoted = !quoted;
                            break;
                        case ',':
                            sb.Append(ch);
                            if (!quoted)
                            {
                                sb.AppendLine();
                                Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
                            }
                            break;
                        case ':':
                            sb.Append(ch);
                            if (!quoted)
                                sb.Append(" ");
                            break;
                        default:
                            sb.Append(ch);
                            break;
                    }
                }
                return sb.ToString();
            }
        }
        static class Extensions
        {
            public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
            {
                foreach (var i in ie)
                {
                    action(i);
                }
            }
        }
    }
}
