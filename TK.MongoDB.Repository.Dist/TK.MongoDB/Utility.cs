using MongoDB.Bson;
using System.Collections.Generic;

namespace TK.MongoDB
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
    }
}
