using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TK.MongoDB.Distributed.Classes;

namespace TK.MongoDB.Distributed.Data
{
    public class MasterRepository : Settings, IMasterRepository
    {
        protected MongoDBContext Context { get; private set; }
        protected IMongoCollection<BsonDocument> Collection { get; private set; }

        public MasterRepository()
        {
            Context = new MongoDBContext(ConnectionStringSettingName);
            Collection = Context.Database.GetCollection<BsonDocument>(MasterCollectionName);
        }

        public async Task<IEnumerable<object>> GetAsync()
        {
            var records = await Collection.Find(new BsonDocument())
                .Project(Builders<BsonDocument>.Projection.Exclude("_id"))
                .Sort(Builders<BsonDocument>.Sort.Descending("CreationDate"))
                .ToListAsync();

            return Utility.Convert<object>(records);
        }

        public async Task<IEnumerable<object>> GetAsync(IDictionary<string, object> keyValuePairs)
        {
            var searchDocument = Utility.CreateSearchBsonDocument(keyValuePairs);
            var records = await Collection.Find(searchDocument)
                .Project(Builders<BsonDocument>.Projection.Exclude("_id"))
                .Sort(Builders<BsonDocument>.Sort.Descending("CreationDate"))
                .ToListAsync();

            return Utility.Convert<object>(records);
        }

        public async Task<IEnumerable<object>> GetAsync(FilterDefinition<BsonDocument> filter)
        {
            var records = await Collection.Find(filter)
                .Project(Builders<BsonDocument>.Projection.Exclude("_id"))
                .Sort(Builders<BsonDocument>.Sort.Descending("CreationDate"))
                .ToListAsync();

            return Utility.Convert<object>(records);
        }

        public async Task<bool> UpdateAsync(string collectionId, string name)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("CollectionId", collectionId);
            UpdateResult updateResult = await Collection.UpdateOneAsync(filter, Builders<BsonDocument>.Update.Set("Name", name).Set("UpdationDate", DateTime.UtcNow));
            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

        public void Dispose()
        {
            if (Context != null)
                Context.Dispose();
        }
    }
}
