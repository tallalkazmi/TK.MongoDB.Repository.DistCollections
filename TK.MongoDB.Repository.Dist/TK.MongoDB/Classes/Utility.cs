using MongoDB.Bson;
using MongoDB.Bson.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TK.MongoDB.Classes
{
    public static class Utility
    {
        public static BsonDocument CreateSearchBsonDocument(IDictionary<string, object> keyValues)
        {
            var searchCriteria = new BsonArray();

            foreach (var item in keyValues)
            {
                if (item.Value == null) continue;
                searchCriteria.Add(new BsonDocument(item.Key, BsonValue.Create(item.Value)));
            }

            var filterDocument = new BsonDocument { { "$and", searchCriteria } };
            return filterDocument;
        }

        public static IEnumerable<T> Convert<T>(IEnumerable<BsonDocument> bson) where T : class
        {
            var writterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Shell };
            var temp = bson.ToJson(writterSettings);

            //Replace using MatchEvaluator:
            var temp_EvalReplaced = Regex.Replace(temp, @"ISODate\((?<dt>.*?)\)", m => m.Groups["dt"].Value);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<T>>(temp_EvalReplaced);
        }
    }
}
