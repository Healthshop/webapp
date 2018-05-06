using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CosineSimilarity;
using NLUclient;

namespace RankingAndRelevance
{
    public class Ranker
    {
        public static HashSet<string> UniqueSimilarities = new HashSet<string>();

        static void Main()
        {
            Dictionary<string, double[]> cuisDictionary = LoadData();

            DateTime dt1 = DateTime.Now;
            List<Provider> providers = ReadProviders("Files\\Provider.tsv"); //191 provider

            string patientDescriptionInputText = "Testinq OCT: OD central foveal thickness 200 um nl foveal contour. No SRF/lntraretinal Fluid OS central foveal thickness 166 um nl foveal contour. Pseudohole with ERM, developed after 12/6/17 laser Impression/Plan and Discussion 1 Rhegmatogenous Retinal detachment left eye. sip barricade laser around subretinal fluid. Irecommend that we proceed with scleral buckle to treat detachment and prevent futher posterior PVR (epiretinal membrane) from developing. I discussed staged procedure with scleral buckle with small gas bubble initially, and possible membrane peel in the future. I recommend that we proceed with surgery 12/29/17 with buckle and small gas bubble with 3-4 days of face down positioning to help With resolution of macular subretinal fluid. I discussed that it is unclear whether her scotoma is related to the detachment or the laser, but that fixing the detachment will minimize the progression of ERM 2. Limited peripheral rhegmatogenous retinal detachment right eye. sip laser barricade 12/6/17. No further tears noted. plan dilated exam OD at the time of surgery OS. 3. High myopia both eyes. The clinical findings noted were reviewed in detail. Printed information was provided. The patient expressed an understanding of this explanation and further expressed a desire to proceed in the manner outlined. Follow up scheduled for 12.29.17 cryo, scleral buckle with gas left eye. Allerqies. Medications and Problem list Problems Medical History: None Allerqies";
            CuiEntities patientCuiEntities = NuClient.ExtractCuiEntities(patientDescriptionInputText); //270 entities

            foreach (Provider provider in providers)
            {
                string k = provider.Keywords;
                CuiEntities providerCuiEntities = NuClient.ExtractCuiEntities(k); //235 entities
                var topFiveResults = ExtractSimilarities(providerCuiEntities, patientCuiEntities, cuisDictionary);
                PrintSimilarities(topFiveResults);
            }

            //string providerDescriptionInputText = "Small-incision phacoemulsification cataract surgery, minor in-office procedures including small eyelid anomalies and chalazion excisions and laser surgical procedures for secondary cataracts, glaucoma and diabetic complications Special Interests General ophthalmology including cataract surgery, glaucoma and diabetic care.";
            //CuiEntities providerCuiEntities = NuClient.ExtractCuiEntities(providerDescriptionInputText); //235 entities
            //StringBuilder sb = ExtractSimilarities(providerCuiEntities, patientCuiEntities, cuisDictionary);
            //Console.WriteLine(sb.ToString());

            string all = string.Join(", ", UniqueSimilarities);
            DateTime dt3 = DateTime.Now;
            TimeSpan duration2 = dt3.Subtract(dt1);
            Console.WriteLine($"Seconds to rank: {duration2.TotalSeconds}");
        }

        public static List<Similarity> ExtractSimilarities(
            CuiEntities providerCuiEntities, 
            CuiEntities patientCuiEntities,
            Dictionary<string, double[]> cuisDictionary)
        {
            List<Similarity> similarities = RankSimilarities(providerCuiEntities, patientCuiEntities, cuisDictionary);
            similarities.Reverse();
            List<Similarity> top = similarities.Take(10).ToList();
            return top;
        }

        private static void PrintSimilarities(IEnumerable<Similarity> topFiveResults)
        {
            StringBuilder sb = new StringBuilder();
            HashSet<string> h = new HashSet<string>();
            foreach (Similarity similarity in topFiveResults)
            {
                string text =
                    $"Provider Surface Form: {similarity.ProviderSurfaceForm} Patient Surface Form: {similarity.PatientSurfaceForm} ";
                if (!h.Contains(text))
                {
                    sb.AppendLine(text);
                    h.Add(text);
                }
            }
        }

        public static Dictionary<string, double[]> LoadData(string path = "Files\\cui2vec_pretrained.csv")
        {
            DateTime dt = DateTime.Now;
            //List<Patient> patients = ReadPatients("Files\\Patient.tsv"); //280 patients
            //List<Member> members = ReadMembers("Files\\Member.tsv"); //280 members
            //List<PlaceOfService> placeOfServices = ReadPlaceOfService("Files\\PlaceOfService.tsv"); //70 places
            //List<Provider> providers = ReadProviders("Files\\Provider.tsv"); //191 provider
            //http://ec2-52-14-191-192.us-east-2.compute.amazonaws.com:1234/ 
            //https://arxiv.org/abs/1804.01486 
            //This tab displays an interactive t-sne visualization of all 108,477 concepts. 
            Dictionary<string, double[]> cuisDictionary = ReadCuiVector(path); //109053 CUI entities 
            DateTime dt2 = DateTime.Now;
            TimeSpan duration = dt2.Subtract(dt);
            Console.WriteLine("Seconds to load: " + duration.TotalSeconds);
            return cuisDictionary;
        }

        private static List<Similarity> RankSimilarities(CuiEntities providerCuiEntities, 
            CuiEntities patientCuiEntities, 
            Dictionary<string, double[]> cuisDictionary)
        {
            List<Similarity> cosineSimilarites = new List<Similarity>();
            foreach (Entity providerCuiEntity in providerCuiEntities.entities)
            {
                if (providerCuiEntity == null) continue;
                foreach (var patientCuiEntity in patientCuiEntities.entities)
                {
                    if (patientCuiEntity == null) continue;

                    string providerCuiEntityCui = providerCuiEntity.cui;
                    string providerSurfaceForm = providerCuiEntity.surface_form;

                    string patientCuiEntityCui = patientCuiEntity.cui;
                    string patientSurfaceForm = patientCuiEntity.surface_form;

                    double[] providerVector;
                    if (cuisDictionary.TryGetValue(providerCuiEntityCui, out providerVector))
                    {
                        double[] patientVector;
                        if (cuisDictionary.TryGetValue(patientCuiEntityCui, out patientVector))
                        {
                            string uniqueIds = $"{providerCuiEntityCui} {patientCuiEntityCui}";
                            if (UniqueSimilarities.Contains(uniqueIds))
                                continue;

                            UniqueSimilarities.Add(uniqueIds);

                            double similarity = CosineSimilairityProgram.CalculateCosineSimilarity(patientVector, providerVector);
                            Similarity s = new Similarity
                            {
                                Rank = similarity,
                                PatientSurfaceForm = patientSurfaceForm,
                                ProviderSurfaceForm = providerSurfaceForm,
                            };

                            cosineSimilarites.Add(s);
                        }
                        else
                        {
                            Console.WriteLine($"Patient CUI not found: {patientCuiEntityCui} surfaceForm: {patientSurfaceForm}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Provider CUI not found: {providerCuiEntityCui} surfaceForm: {providerSurfaceForm}");
                    }
                }
            }

            // This shows calling the Sort(Comparison(T) overload using 
            // an anonymous method for the Comparison delegate. 
            // This method treats null as the lesser of two values.
            cosineSimilarites.Sort((x, y) => x.Rank.CompareTo(y.Rank));

            return cosineSimilarites;
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
                char[] comma = new char[] { ',' };

                string[] values = line.Split(comma, StringSplitOptions.RemoveEmptyEntries);
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

        public static List<PlaceOfService> ReadPlaceOfService(string path)
        {
            List<PlaceOfService> places = new List<PlaceOfService>();

            var lines = File.ReadLines(path);
            int lineCounter = 0;
            foreach (string line in lines)
            {
                if (lineCounter == 0)
                {
                    lineCounter++;
                    continue; //skip header
                }

                string[] columns = line.Split('\t');
                string placeOfServiceKey = columns[0];
                string servicePlaceCd = columns[1];
                string servicePlaceDs = columns[2];
                string placeOfServiceEffDt = columns[3];
                string placeOfServiceExpDt = columns[4];
                string dwProcessId = columns[5];
                string dwTimestamp = columns[6];

                PlaceOfService p = new PlaceOfService
                {
                    PlaceOfServiceKey = placeOfServiceKey,
                    ServicePlaceCd = servicePlaceCd,
                    ServicePlaceDs = servicePlaceDs,
                    PlaceOfServiceEffDt = placeOfServiceEffDt,
                    PlaceOfServiceExpDt = placeOfServiceExpDt,
                    DwProcessId = dwProcessId,
                    DwTimestamp = dwTimestamp,
                };

                places.Add(p);
                lineCounter++;
            }
            return places;
        }

        public static List<Member> ReadMembers(string path)
        {
            List<Member> members = new List<Member>();

            var lines = File.ReadLines(path);
            int lineCounter = 0;
            foreach (string line in lines)
            {
                if (lineCounter == 0)
                {
                    lineCounter++;
                    continue; //skip header
                }

                string[] columns = line.Split('\t');
                string mbrId = columns[0];
                string groupName = columns[1];
                string mbrEffectiveDt = columns[2];
                string mbrTermDt = columns[3];
                string mbrFirstName = columns[4];
                string mbrLastName = columns[5];
                string mbrMiddleName = columns[6];
                string mbrRelation = columns[7];
                string mbrBirthDt = columns[8];
                string mbrCity = columns[9];
                string mbrCounty = columns[10];
                string mbrState = columns[11];
                string mbrZip = columns[12];
                string mbrGenderCd = columns[13];
                string maritalStatusCd = columns[14];

                Member p = new Member
                {
                    MbrId = mbrId,
                    GroupName = groupName,
                    MbrEffectiveDt = mbrEffectiveDt,
                    MbrTermDt = mbrTermDt,
                    MbrFirstName = mbrFirstName,
                    MbrLastName = mbrLastName,
                    MbrMiddleName = mbrMiddleName,
                    MbrRelation = mbrRelation,
                    MbrBirthDt = mbrBirthDt,
                    MbrCity = mbrCity,
                    MbrCounty = mbrCounty,
                    MbrState = mbrState,
                    MbrZip = mbrZip,
                    MbrGenderCd = mbrGenderCd,
                    MaritalStatusCd = maritalStatusCd,
                };

                members.Add(p);
                lineCounter++;
            }
            return members;
        }

        public static List<Provider> ReadProviders(string path)
        {
            List<Provider> providers = new List<Provider>();

            var lines = File.ReadLines(path);
            int lineCounter = 0;
            foreach (string line in lines)
            {
                if (lineCounter == 0)
                {
                    lineCounter++;
                    continue; //skip header
                }

                string[] columns = line.Split('\t');
                string prvIdn = columns[1];
                string providerNpi = columns[2];
                string effectiveDt = columns[3];
                string termDt = columns[4];
                string facilityName = columns[5];
                string lastName = columns[5];
                string firstName = columns[6];
                string middleName = columns[7];
                string suffixName = columns[8];
                string providerSpecialityCode = columns[9];
                string providerSpecialityDesc = columns[10];
                string providerCity = columns[11];
                string providerCounty = columns[12];
                string providerState = columns[13];
                string zip = columns[14];
                string deaNumber = columns[16];
                string stateLicense = columns[17];
                string taxId = columns[18];
                string createTimestamp = columns[19];
                string keywords = columns[20];
                string price = columns[21];

                Provider p = new Provider
                {
                    PrvIdn = prvIdn,
                    ProviderNpi = providerNpi,
                    EffectiveDt = effectiveDt,
                    TermDt = termDt,
                    FacilityName = facilityName,
                    LastName = lastName,
                    FirstName = firstName,
                    MiddleName = middleName,
                    SuffixName = suffixName,
                    ProviderSpecialityCode = providerSpecialityCode,
                    ProviderSpecialityDesc = providerSpecialityDesc,
                    ProviderCity = providerCity,
                    ProviderCounty = providerCounty,
                    ProviderState = providerState,
                    ProviderZip = zip,
                    DeaNumber = deaNumber,
                    StateLicense = stateLicense,
                    TaxId = taxId,
                    CreateTimestamp = createTimestamp,
                    Keywords = keywords,
                    Price = price,
                };

                providers.Add(p);
                lineCounter++;
            }
            return providers;
        }

        public static List<Patient> ReadPatients(string path)
        {
            var lines = File.ReadLines(path);
            int lineCounter = 0;
            List<Patient> patients = new List<Patient>();
            foreach (string line in lines)
            {
                if (lineCounter == 0)
                {
                    lineCounter++;
                    continue; //skip header
                }

                string[] columns = line.Split('\t');
                string patientRelCode = columns[0];
                string patientLast = columns[1];
                string patientFirst = columns[2];
                string patientMiddle = columns[3];
                string patientDob = columns[4];
                string patientSex = columns[5];
                string patientMaritalStatus = columns[6];
                string patientAddressLine1 = columns[7];
                string patientAddressLine2 = columns[8];
                string patientCity = columns[9];
                string patientState = columns[10];
                string patientZip = columns[11];
                string keywords = columns[12];

                Patient p = new Patient
                {
                    PatientRelCode = patientRelCode,
                    PatientLast = patientLast,
                    PatientFirst = patientFirst,
                    PatientMiddle = patientMiddle,
                    PatientDob = patientDob,
                    PatientSex = patientSex,
                    PatientMaritalStatus = patientMaritalStatus,
                    PatientAddressLine1 = patientAddressLine1,
                    PatientAddressLine2 = patientAddressLine2,
                    PatientCity = patientCity,
                    PatientState = patientState,
                    PatientZip = patientZip,
                    Keywords = keywords,
                };

                patients.Add(p);
                lineCounter++;
            }
            return patients;
        }
    }
}
