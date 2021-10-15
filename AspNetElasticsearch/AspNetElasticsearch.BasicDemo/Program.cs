using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Elasticsearch.Net;

namespace AspNetElasticsearch.BasicDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Single node
            var node = new Uri("http://172.16.6.47:9200");
            var settings = new ConnectionConfiguration(node).RequestTimeout(TimeSpan.FromMinutes(2));

            ////Multiple nodes
            //var uris = new[]
            //{
            //    new Uri("http://172.16.6.47:9200"),
            //    new Uri("http://172.16.6.47:9200"),
            //    new Uri("http://172.16.6.47:9200")
            //};
            //var connectionPool = new SniffingConnectionPool(uris);
            //var settings = new ConnectionConfiguration(connectionPool);

            var lowlevelClient = new ElasticLowLevelClient(settings);

            #region Indexing
            //var person = new Person
            //{
            //    FirstName = "Martijn",
            //    LastName = "Laarman"
            //};
            //var indexResponse = await lowlevelClient.IndexAsync<StringResponse>("people", PostData.Serializable(person));
            //string responseString = indexResponse.Body;
            #endregion

            #region Buld indexing
            //var people = new object[]
            //{
            //    new{ index = new { _index = "people", _type = "_doc" }},
            //    new{ FirstName = "Martijn", LastName = "Laarman" },
            //    new{ index = new { _index = "people", _type = "_doc" }},
            //    new{ FirstName = "Greg", LastName = "Marzouka" },
            //    new{ index = new { _index = "people", _type = "_doc" }},
            //    new{ FirstName = "Russ", LastName = "Cam" }
            //};
            //var indexResponse = await lowlevelClient.BulkAsync<StringResponse>(PostData.MultiJson(people));
            //string responseString = indexResponse.Body;
            #endregion

            #region Searching
            var searchResponse = await lowlevelClient.SearchAsync<StringResponse>("people", PostData.Serializable(new
            {
                from = 0,
                size = 10,
                query = new
                {
                    match = new
                    {
                        FirstName = new
                        {
                            query = "Martijn"
                        }
                    }
                }
            }));
            var result = new List<Tuple<string, string, string, Person>>();
            if (searchResponse.Success)
            {
                var responseJson = searchResponse.Body;
                var obj = System.Text.Json.JsonSerializer.Deserialize<SearchResponseBody<Person>>(responseJson);

                var total = obj.hits.total.value;
                foreach (var item in obj.hits.hits)
                {
                    string index = item._index;
                    string type = item._type;
                    string id = item._id;
                    Person person = item._source;
                    result.Add(new Tuple<string, string, string, Person>(index, type, id, person));
                }
            }
            else
                throw new Exception(searchResponse.OriginalException.Message, searchResponse.OriginalException);
            #endregion

            Console.Read();
        }
    }

    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class SearchResponseBody<T>
    {
        public long took { get; set; }
        public bool timed_out { get; set; }
        public SearchResponseBodyShards _shards { get; set; }
        public SearchResponseBodyHits<T> hits { get; set; }
    }
    public class SearchResponseBodyShards
    {
        public long total { get; set; }
        public long successful { get; set; }
        public long skipped { get; set; }
        public long failed { get; set; }
    }
    public class SearchResponseBodyHits<T>
    {
        public double max_score { get; set; }
        public SearchResponseBodyHitsTotal total { get; set; }
        public SearchResponseBodyHitsHits<T>[] hits { get; set; }
    }
    public class SearchResponseBodyHitsTotal
    {
        public long value { get; set; }
        public string relation { get; set; }
    }
    public class SearchResponseBodyHitsHits<T>
    {
        public string _index { get; set; }
        public string _type { get; set; }
        public string _id { get; set; }
        public double _score { get; set; }
        public T _source { get; set; }
    }
}
