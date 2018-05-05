using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CosineSimilarity;

namespace RankingAndRelevance
{
    class Program
    {
        static void Main()
        {
            DateTime dt = DateTime.Now;
            List<Patient> patients = ReadPatients("Files\\Patient.tsv");
            List<Member> members = ReadMembers("Files\\Member.tsv");
            List<PlaceOfService> placeOfServices = ReadPlaceOfService("Files\\PlaceOfService.tsv");
            List<Provider> providers = ReadProviders("Files\\Provider.tsv");
            //http://ec2-52-14-191-192.us-east-2.compute.amazonaws.com:1234/ 
            //https://arxiv.org/abs/1804.01486 
            //This tab displays an interactive t-sne visualization of all 108,477 concepts. 
            Dictionary<string, double[]> cuisDictionary = ReadCuiVector("Files\\cui2vec_pretrained.csv");
            DateTime dt2 = DateTime.Now;
            TimeSpan duration = dt2.Subtract(dt);
            Console.WriteLine("Seconds to load: " + duration.TotalSeconds);

            string providerDescriptionInputText = File.ReadAllText("Files\\ProviderDescriptionInput.txt");
            string providerCuiOuput = File.ReadAllText("Files\\ProviderCuiOutput.json");

            //string cuiUserInput = @"{ ""entities"": [{ ""surface_form"": ""Vitrectomy"",""cui"": ""C0042903""}, { ""surface_form"": ""Scleral Buckling"", ""cui"": ""C0036411""}]}";
            string patientDescriptionInputText = File.ReadAllText("Files\\PatientOcrInput.txt");
            string patientCuiOuput = File.ReadAllText("Files\\PatientCuiOutput.json");

            CuiEntities providerCuiEntities = CuiEntities.ReadToObject(providerCuiOuput);
            CuiEntities patientCuiEntities = CuiEntities.ReadToObject(patientCuiOuput);
            List<string> surfaces = new List<string>();

            List<Similarity> similarities = RankSimilarities(providerCuiEntities, patientCuiEntities, cuisDictionary);
            similarities.Reverse();
            var topFiveResults = similarities.Take(10);
            StringBuilder sb = new StringBuilder();
            foreach (Similarity similarity in topFiveResults)
            {
                string text = $"Provider Surface Form: {similarity.ProviderSurfaceForm} Patient Surface Form: {similarity.PatientSurfaceForm}";
                sb.Append(text);
            }
            Console.Write(sb.ToString());
        }

        private static List<Similarity> RankSimilarities(
            CuiEntities providerCuiEntities, CuiEntities patientCuiEntities, Dictionary<string, double[]> cuisDictionary)
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
            cosineSimilarites.Sort(delegate (Similarity x, Similarity y)
            {
                if (x.Rank == null && y.Rank == null) return 0;
                else if (x.Rank == null) return -1;
                else if (y.Rank == null) return 1;
                else return x.Rank.CompareTo(y.Rank);
            });

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
                string prvIdn = columns[0];
                string providerNpi = columns[1];
                string effectiveDt = columns[2];
                string termDt = columns[3];
                string facilityName = columns[4];
                string lastName = columns[5];
                string firstName = columns[6];
                string middleName = columns[7];
                string suffixName = columns[8];
                string providerSpecialityCode = columns[9];
                string providerSpecialityDesc = columns[10];
                string providerCity = columns[11];
                string providerCounty = columns[12];
                string providerState = columns[13];
                string deaNumber = columns[14];
                string stateLicense = columns[15];
                string taxId = columns[16];
                string createTimestamp = columns[17];
                string modifyTimestamp = columns[18];
                string keywords = columns[19];
                string price = columns[20];

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
                    DeaNumber = deaNumber,
                    StateLicense = stateLicense,
                    TaxId = taxId,
                    CreateTimestamp = createTimestamp,
                    ModifyTimestamp = modifyTimestamp,
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
