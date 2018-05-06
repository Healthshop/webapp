using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cui2VecSubmitter
{
    class Program
    {
        static void Main(string[] args)
        {
            string cui2Vec = "cui2vec_pretrained.csv";
            var filter = File.ReadAllText("Filter.txt");
            var f = filter.Split(',').ToList();
            HashSet<string> h = new HashSet<string>();
            char[] whitespace = new char[] { ' ', '\t' };

            foreach (var s in f)
            {
                var ssizes = s.Split(whitespace, StringSplitOptions.RemoveEmptyEntries);
                foreach (var sz in ssizes)
                {
                    h.Add(sz);
                }
            }


            Dictionary<string, double[]> cuisDictionary = ReadCuiVector(cui2Vec); //109053 CUI entities
            StringBuilder sb = new StringBuilder();
            List<string> list = new List<string>();
            foreach (var cuis in cuisDictionary)
            {
                if (h.Contains(cuis.Key))
                {
                    sb.Append(cuis.Key);
                    sb.Append(',');
                    foreach (var d in cuis.Value)
                    {
                        sb.Append(d);
                        sb.Append(',');
                    }
                    list.Add(sb.ToString());
                    sb.Clear();
                }
            }
            File.AppendAllLines(@"Filtered.csv", list);

            //TODO upload to blob
        }

        private static Dictionary<string, double[]> ReadCuiVector(string path)
        {
            Dictionary<string, double[]> cuiVectors = new Dictionary<string, double[]>();
            int lineCounter = 0;
            foreach (string line in File.ReadLines(path))
            {
                if (lineCounter == 0)
                {
                    lineCounter++;
                    continue; //skip header
                }
                string[] values = line.Split(',');
                string cui = string.Empty;
                List<double> vector = new List<double>();

                for (int i = 0; i < values.Length; i++)
                {
                    string value = values[i].Trim('\"');
                    //Trim values
                    if (i == 0)
                    {
                        cui = value;
                        continue;
                    }
                    vector.Add(double.Parse(value));
                }

                cuiVectors.Add(cui, vector.ToArray());
            }
            return cuiVectors;
        }
    }
}
