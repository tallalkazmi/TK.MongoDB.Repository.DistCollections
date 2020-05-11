using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TK.MongoDB.Classes;

namespace TK.MongoDB
{
    public class MasterRepository : Settings
    {
        protected MongoDBContext Context { get; private set; }
        protected IMongoCollection<BsonDocument> Collection { get; private set; }

        public MasterRepository()
        {
            Context = new MongoDBContext(ConnectionStringSettingName);
            Collection = Context.Database.GetCollection<BsonDocument>(MasterCollectionName);
        }

        public async Task<string> Get()
        {
            var records = await Collection.Find(new BsonDocument())
                .Project(Builders<BsonDocument>.Projection.Exclude("_id"))
                .Sort(Builders<BsonDocument>.Sort.Descending("CreationDate"))
                .ToListAsync();

            var writterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Shell };
            var result = records.ToJson();
            return result;
        }

        public async Task<string> Get(IDictionary<string, object> keyValuePairs)
        {
            var searchDocument = Utility.CreateSearchBsonDocument(keyValuePairs);
            var records = await Collection.Find(searchDocument)
                .Project(Builders<BsonDocument>.Projection.Exclude("_id"))
                .Sort(Builders<BsonDocument>.Sort.Descending("CreationDate"))
                .ToListAsync();

            var writterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Shell };
            var result = records.ToJson();
            return result;
        }

        public async Task<bool> Update(string collectionId, string name)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("CollectionId", collectionId);
            UpdateResult updateResult = await Collection.UpdateOneAsync(filter, Builders<BsonDocument>.Update.Set("Name", name).Set("UpdationDate", DateTime.UtcNow));
            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }
    }
}
