﻿using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TK.MongoDB.Distributed.Classes;

namespace TK.MongoDB.Distributed.Data
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class MasterRepository : MasterSettings, IMasterRepository
    {
        private readonly MongoDBContext Context;
        private readonly IMongoCollection<BsonDocument> Collection;

        public MasterRepository()
        {
            Context = new MongoDBContext(ConnectionStringSettingName);
            Collection = Context.Database.GetCollection<BsonDocument>(CollectionName);
        }

        public async Task<Tuple<IEnumerable<object>, long>> GetAsync(int currentPage, int pageSize)
        {
            var query = Collection.Find(new BsonDocument());

            long totalCount = await query.CountDocumentsAsync();
            var records = await query.Sort(Builders<BsonDocument>.Sort.Descending("CreationDate"))
                .Skip((currentPage - 1) * pageSize)
                .Limit(pageSize)
                .Project(Builders<BsonDocument>.Projection.Exclude("_id"))
                .ToListAsync();

            return new Tuple<IEnumerable<object>, long>(Utility.Convert<object>(records), totalCount);
        }

        public async Task<Tuple<IEnumerable<object>, long>> GetAsync(int currentPage, int pageSize, IDictionary<string, object> keyValuePairs)
        {
            var searchDocument = Utility.CreateSearchBsonDocument(keyValuePairs);
            var query = Collection.Find(searchDocument);

            long totalCount = await query.CountDocumentsAsync();
            var records = await query.Sort(Builders<BsonDocument>.Sort.Descending("CreationDate"))
                .Skip((currentPage - 1) * pageSize)
                .Limit(pageSize)
                .Project(Builders<BsonDocument>.Projection.Exclude("_id"))
                .ToListAsync();

            return new Tuple<IEnumerable<object>, long>(Utility.Convert<object>(records), totalCount);
        }

        public async Task<Tuple<IEnumerable<object>, long>> GetAsync(int currentPage, int pageSize, FilterDefinition<BsonDocument> filter)
        {
            var query = Collection.Find(filter);

            long totalCount = await query.CountDocumentsAsync();
            var records = await query.Sort(Builders<BsonDocument>.Sort.Descending("CreationDate"))
                .Skip((currentPage - 1) * pageSize)
                .Limit(pageSize)
                .Project(Builders<BsonDocument>.Projection.Exclude("_id"))
                .ToListAsync();

            return new Tuple<IEnumerable<object>, long>(Utility.Convert<object>(records), totalCount);
        }

        public async Task<bool> UpdateAsync(string collectionId, string property, object value)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("CollectionId", collectionId);
            UpdateResult updateResult = await Collection.UpdateOneAsync(filter, Builders<BsonDocument>.Update.Set(property, value).Set("UpdationDate", DateTime.UtcNow));
            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

        public void Dispose()
        {
            if (Context != null)
                Context.Dispose();
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
