using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TK.MongoDB.Classes;

namespace TK.MongoDB.Data
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

        public async Task<IEnumerable<object>> Get()
        {
            var records = await Collection.Find(new BsonDocument())
                .Project(Builders<BsonDocument>.Projection.Exclude("_id"))
                .Sort(Builders<BsonDocument>.Sort.Descending("CreationDate"))
                .ToListAsync();

            return Utility.Convert<object>(records);
        }

        public async Task<IEnumerable<object>> Get(IDictionary<string, object> keyValuePairs)
        {
            var searchDocument = Utility.CreateSearchBsonDocument(keyValuePairs);
            var records = await Collection.Find(searchDocument)
                .Project(Builders<BsonDocument>.Projection.Exclude("_id"))
                .Sort(Builders<BsonDocument>.Sort.Descending("CreationDate"))
                .ToListAsync();

            return Utility.Convert<object>(records);
        }

        public async Task<IEnumerable<object>> Get(Expression<Func<BsonDocument, bool>> filter)
        {
            var records = await Collection.Find(filter)
                .Project(Builders<BsonDocument>.Projection.Exclude("_id"))
                .Sort(Builders<BsonDocument>.Sort.Descending("CreationDate"))
                .ToListAsync();

            return Utility.Convert<object>(records);
        }

        public async Task<bool> Update(string collectionId, string name)
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
