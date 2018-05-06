using System;
using System.Globalization;

namespace RankingAndRelevance
{
    public class Similarity : IEquatable<Similarity>, IComparable<Similarity>
    {
        public Similarity(string patientSurfaceForm, string providerSurfaceForm, double rank)
        {
            PatientSurfaceForm = patientSurfaceForm;
            ProviderSurfaceForm = providerSurfaceForm;
            Rank = rank;
        }

        public Similarity()
        {
        }

        public string ProviderSurfaceForm { get; set; }

        public string PatientSurfaceForm { get; set; }

        /// <summary>
        /// 0 to 1
        /// </summary>
        public double Rank { get; set; }

        public override string ToString()
        {
            return $"ProviderSurfaceForm: {ProviderSurfaceForm} PatientSurfaceForm: {PatientSurfaceForm}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Similarity objAsPart = obj as Similarity;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }

        public int SortByRankAscending(double rank1, double rank2)
        {
            return rank2.CompareTo(rank1);
        }

        // Default comparer for Part type.
        public int CompareTo(Similarity compareSimilarity)
        {
            // A null value means that this object is greater.
            if (compareSimilarity == null)
                return 1;
            else
                return this.Rank.CompareTo(compareSimilarity.Rank);
        }

        public override int GetHashCode()
        {
            return int.Parse(s: Rank.ToString(CultureInfo.InvariantCulture)) ^ PatientSurfaceForm.Length ^ ProviderSurfaceForm.Length;
        }

        public bool Equals(Similarity other)
        {
            if (other == null) return false;
            return (this.Rank.Equals(other.Rank));
        }
    }
}
