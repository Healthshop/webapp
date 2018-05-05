using System;

namespace CosineSimilarity
{
    /// <summary>
    ///Cosine similarity is generally used as a metric for measuring distance when the magnitude of the vectors 
    //does not matter.This happens for example when working with text data represented by word counts.
    //We could assume that when a word(e.g.science) occurs more frequent in document 1 than it does in document 2, 
    //that document 1 is more related to the topic of science.However, 
    //it could also be the case that we are working with documents of uneven lengths(Wikipedia articles for example).
    //Then, science probably occurred more in document 1 just because it was way longer than document 2.
    //Cosine similarity corrects for this.

    //Text data is the most typical example for when to use this metric.However, 
    //you might also want to apply cosine similarity for other cases where some properties of the instances make 
    //so that the weights might be larger without meaning anything different.Sensor values that were captured in 
    //various lengths(in time) between instances could be such an example.
    /// </summary>
    public class CosineSimilairityProgram
    {
        static void Main()
        {
            double[] vecA = { 1, 2, 3, 4, 5 };
            double[] vecB = { 6, 7, 7, 9, 10 };
            double cosSimilarity = CalculateCosineSimilarity(vecA, vecB);
            Console.WriteLine(cosSimilarity);
            Console.Read();
        }

        public static double CalculateCosineSimilarity(double[] vecA, double[] vecB)
        {
            double dotProduct = DotProduct(vecA, vecB);
            double magnitudeOfA = Magnitude(vecA);
            double magnitudeOfB = Magnitude(vecB);
            return dotProduct / (magnitudeOfA * magnitudeOfB);
        }

        private static double DotProduct(double[] vecA, double[] vecB)
        {
            // I'm not validating inputs here for simplicity.            
            double dotProduct = 0;
            for (int i = 0; i < vecA.Length; i++)
            {
                dotProduct += (vecA[i] * vecB[i]);
            }
            return dotProduct;
        }

        // Magnitude of the vector is the square root of the dot product of the vector with itself.
        private static double Magnitude(double[] vector)
        {
            return Math.Sqrt(DotProduct(vector, vector));
        }
    }
}
