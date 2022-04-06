using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RestSharp;

namespace SmarterContract
{
    public class Settings
    {
        public List<string> WrongNumber { get; set; } = new List<string>();
        public List<string> Sold { get; set; } = new List<string>();
        public List<string> HotLead { get; set; } = new List<string>();
        public List<string> Talking { get; set; } = new List<string>();
        public List<string> NotInterested { get; set; } = new List<string>();
    }
    class Program
    {
        static Settings settings = new Settings();
        static void Main(string[] args)
        {
            settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));

            Console.WriteLine("Please enter the authorization token: ");
            string token = Console.ReadLine();

            if (!string.IsNullOrEmpty(token))
            {
                FetchData(token);
            }
            else
                Console.WriteLine("Token is not provided.");

            Console.WriteLine("Operation Completed");
            Console.ReadKey();
        }

        static void FetchData(string token)
        {
            int TotalPages = 1;

            for (int page = 1; page <= TotalPages; page++)
            {
                Console.WriteLine("Processing page: " + page);
                var client = new RestClient("https://us-central1-smartercontact-prod.cloudfunctions.net/search-contacts");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("authorization", token);
                request.AddHeader("content-type", " application/json");
                request.AddHeader("origin", " https://smartercontact.com");
                request.AddHeader("referer", " https://smartercontact.com/");
                request.AddJsonBody(new Payload
                {
                    data = new Data
                    {
                        grade = new List<string> { "5f2da4889f9d3a002dd2fa17" },
                        size = 250,
                        from = (page==1)?0:(((page-1)*250)+1),
                        status = "active"
                    }
                });
                var response = client.Execute<Contracts>(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK && response != null && response.Data != null)
                {
                    if (TotalPages == 1)
                    {
                        TotalPages =  (response.Data.result.hits.total.value + 250 - 1) / 250; 
                    }
                    foreach (var entry in response.Data.result.hits.hits)
                    {
                        string id = entry._source.id.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                        if (settings.WrongNumber.Any(y => Regex.IsMatch(entry._source.last_message.ToLower(), $@"\b{y.ToLower()}\b")))
                        {
                            UpdateStatus(id, "Wrong Number", "5f2da4889f9d3a002dd2fa18", token);
                        }
                        else if (settings.NotInterested.Any(y => Regex.IsMatch(entry._source.last_message.ToLower(), $@"\b{y.ToLower()}\b")))
                        {
                            UpdateStatus(id, "Not Interested", "H6myBkly5Z9SKXu9DumL", token);
                        }
                        else if (settings.Sold.Any(y => Regex.IsMatch(entry._source.last_message.ToLower(), $@"\b{y.ToLower()}\b")))
                        {
                            UpdateStatus(id, "Sold", "5f2da4889f9d3a002dd2fa19", token);
                        }
                        else if (settings.Talking.Any(y => Regex.IsMatch(entry._source.last_message.ToLower(), $@"\b{y.ToLower()}\b")))
                        {
                            UpdateStatus(id, "Talking", "DzcR7BqcBR0Y6bdtsonn", token);
                        }
                        else if (settings.HotLead.Any(y => Regex.IsMatch(entry._source.last_message.ToLower(), $@"\b{y.ToLower()}\b")))
                        {
                            UpdateStatus(id, "Hot Lead", "5f2da4889f9d3a002dd2fa1a", token);
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Unable to fetch data for page {page}");
                }
            }

        }

        static void UpdateStatus(string id, string statusName, string status, string token)
        {
            var client = new RestClient("https://us-central1-smartercontact-prod.cloudfunctions.net/contacts-updateOne");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", token);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(new Payload2 { data = new Datax { contactId = id, gradeId = status } });
            var response = client.Execute<AnotherResponse>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK && response != null && response.Data != null)
            {
                Console.WriteLine($"Status has been updated for message: {response.Data.result.last_message} => {statusName}");
            }
            else
            {
                Console.WriteLine("Unable to update status for records. id: " + id);
            }
        }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Page
    {
        public int size { get; set; }
        public int current { get; set; }
    }

    public class Data
    {
        public int size { get; set; }
        public int from { get; set; }
        public string status { get; set; }
        public List<string> grade { get; set; } = new List<string>();
    }

    public class Payload
    {
        public Data data { get; set; }
    }

    ///////////////
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Bucket
    {
        public string key { get; set; }
        public int doc_count { get; set; }
    }

    public class Sources
    {
        public int doc_count_error_upper_bound { get; set; }
        public int sum_other_doc_count { get; set; }
        public List<Bucket> buckets { get; set; }
    }

    public class Labels
    {
        public int doc_count_error_upper_bound { get; set; }
        public int sum_other_doc_count { get; set; }
        public List<Bucket> buckets { get; set; }
    }

    public class Aggregations
    {
        public Sources sources { get; set; }
        public Labels labels { get; set; }
    }

    public class Total
    {
        public int value { get; set; }
    }

    public class Source
    {
        public bool read { get; set; }
        public bool is_favorite { get; set; }
        public string full_name { get; set; }
        public string phone { get; set; }
        public string grade { get; set; }
        public object last_message_at { get; set; }
        public string last_message { get; set; }
        public string id { get; set; }
        public List<string> labels { get; set; }
        public string status { get; set; }
        public List<string> replied_campaigns { get; set; }
    }

    public class Hit
    {
        public Source _source { get; set; }
        public Total total { get; set; }
        public List<Hit> hits { get; set; }
    }

    public class Result
    {
        public Aggregations aggregations { get; set; }
        public Hit hits { get; set; }
    }

    public class Contracts
    {
        public Result result { get; set; }
    }


    //////
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Datax
    {
        public string contactId { get; set; }
        public string gradeId { get; set; }
    }

    public class Payload2
    {
        public Datax data { get; set; }
    }
    public class Result3
    {
        public string id { get; set; }
        public string grade { get; set; }
        public bool is_favorite { get; set; }
        public bool read { get; set; }
        public bool is_blocked { get; set; }
        public string full_name { get; set; }
        public string last_message { get; set; }
        public long last_message_at { get; set; }
        public string phone { get; set; }
        public string status { get; set; }
        public long updated_at { get; set; }
    }
    public class AnotherResponse
    {
        public Result3 result { get; set; }
    }


}
