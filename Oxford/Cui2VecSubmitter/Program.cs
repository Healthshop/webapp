using System.Collections.Generic;
using System.IO;

namespace Cui2VecSubmitter
{
    class Program
    {
        static void Main(string[] args)
        {
            string cui2Vec = "cui2vec_pretrained.csv";
            Dictionary<string, double[]> cuisDictionary = ReadCuiVector(cui2Vec); //109053 CUI entities
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
