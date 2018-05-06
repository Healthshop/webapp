using System;
using System.Collections.Generic;
using System.Globalization;

namespace RankingAndRelevance
{
    public class Provider : IEquatable<Provider>, IComparable<Provider>
    {
        public string PrvIdn { get; set; }
        public string ProviderNpi { get; set; }
        public string EffectiveDt { get; set; }
        public string TermDt { get; set; }
        public string FacilityName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string SuffixName { get; set; }
        public string ProviderSpecialityCode { get; set; }
        public string ProviderSpecialityDesc { get; set; }
        public string ProviderCity { get; set; }
        public string ProviderCounty { get; set; }
        public string ProviderState { get; set; }
        public string ProviderZip { get; set; }
        public string DeaNumber { get; set; }
        public string StateLicense { get; set; }
        public string TaxId { get; set; }
        public string CreateTimestamp { get; set; }
        public string ModifyTimestamp { get; set; }
        public string Keywords { get; set; }
        public string Price { get; set; }
        public string Distance { get; set; }
        public List<string> Matches = new List<string>();

        public double AverageMatchRank;

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Provider objAsPart = obj as Provider;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }

        public int SortByRankAscending(double rank1, double rank2)
        {
            return rank1.CompareTo(rank2);
        }

        // Default comparer for Part type.
        public int CompareTo(Provider compareSimilarity)
        {
            // A null value means that this object is greater.
            if (compareSimilarity == null)
                return 1;
            else
                return this.AverageMatchRank.CompareTo(compareSimilarity.AverageMatchRank);
        }
        public override int GetHashCode()
        {
            return PrvIdn.GetHashCode();
        }

        public bool Equals(Provider other)
        {
            if (other == null) return false;
            return (this.AverageMatchRank.Equals(other.AverageMatchRank));
        }
    }
}
