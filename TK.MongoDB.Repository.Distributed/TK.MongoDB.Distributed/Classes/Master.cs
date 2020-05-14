using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TK.MongoDB.Distributed.Models;

namespace TK.MongoDB.Distributed.Classes
{
    internal class Master : MasterSettings
    {
        private readonly MongoDBContext Context;
        private readonly IMongoCollection<BsonDocument> Collection;

        public Master()
        {
            Context = new MongoDBContext(ConnectionStringSettingName);
            Collection = Context.Database.GetCollection<BsonDocument>(CollectionName);
        }

        public string RetriveCollectionFromMaster<T>(T obj) where T : BaseEntity
        {
            // Resulted Collection Name
            string RetrivedCollectionId = null;

            //Get Distributed Columns
            Dictionary<string, object> KeyValues = new Dictionary<string, object>();
            Dictionary<string, int> Distribution = new Dictionary<string, int>();

            PropertyInfo[] props = typeof(T).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                DistributeAttribute distAttr = (DistributeAttribute)Attribute.GetCustomAttribute(prop, typeof(DistributeAttribute));
                if (distAttr == null) continue;
                KeyValues.Add(prop.Name, prop.GetValue(obj));
                Distribution.Add(prop.Name, distAttr.Level);
            }

            //Search Master for BsonDocument
            var _1stDoc = Collection.Find<BsonDocument>(new BsonDocument()).Limit(1).SingleOrDefault();

            //If no CollectionId exists, create one and return
            if (_1stDoc == null)
            {
                //Generate CollectionId for Collection Name.
                string GeneratedCollectionId = Guid.NewGuid().ToString("N");

                //Convert Dictionary to BsonDocument for insertion
                var jsonDoc = Newtonsoft.Json.JsonConvert.SerializeObject(KeyValues);
                var bsonDoc = BsonSerializer.Deserialize<BsonDocument>(jsonDoc);

                //Add additional fields to create
                Dictionary<string, object> AddValues = new Dictionary<string, object>
                {
                    { "Name", "Untitled" },
                    { "CollectionId", GeneratedCollectionId },
                    { "ParentCollectionId", null },
                    { "CreationDate", DateTime.UtcNow},
                    { "UpdationDate", null}
                };

                AddValues.AddKeys(AdditionalProperties);
                bsonDoc = bsonDoc.AddRange(AddValues);

                BeforeInsert(bsonDoc);
                Collection.InsertOne(bsonDoc);
                AfterInsert(bsonDoc);

                //Create indexes
                var indexBuilder = Builders<BsonDocument>.IndexKeys;
                List<CreateIndexModel<BsonDocument>> indexes = new List<CreateIndexModel<BsonDocument>>
                {
                    new CreateIndexModel<BsonDocument>(indexBuilder.Ascending("CollectionId"), new CreateIndexOptions { Name = "CollectionIdIndex", Unique = true }),
                    new CreateIndexModel<BsonDocument>(indexBuilder.Descending("CreationDate"), new CreateIndexOptions { Name = "CreationDateIndex" })
                };
                Collection.Indexes.CreateMany(indexes);

                //Create Collection
                Context.Database.CreateCollection(GeneratedCollectionId);
                SetCollectionIndexes<T>(GeneratedCollectionId);
                RetrivedCollectionId = GeneratedCollectionId;
            }
            else
            {
                //Create BsonDocument for search
                BsonDocument searchDocumentByKV = Utility.CreateSearchBsonDocument(KeyValues);
                var query = Collection.Find(searchDocumentByKV);

                //If BsonDocument doesn't exists, create one.
                long count = query.CountDocuments();
                if (count == 0)
                {
                    string parentCollectionId = GetRootCollectionId(KeyValues, Distribution);

                    //Generate CollectionId for Collection Name.
                    string GeneratedCollectionId = Guid.NewGuid().ToString("N");

                    //Convert Dictionary to BsonDocument for insertion
                    var jsonDoc = Newtonsoft.Json.JsonConvert.SerializeObject(KeyValues);
                    var bsonDoc = BsonSerializer.Deserialize<BsonDocument>(jsonDoc);

                    //Add additional fields to create
                    Dictionary<string, object> AddValues = new Dictionary<string, object>
                    {
                        { "Name", "Untitled" },
                        { "CollectionId", GeneratedCollectionId },
                        { "ParentCollectionId", string.IsNullOrWhiteSpace(parentCollectionId) ? null : parentCollectionId },
                        { "CreationDate", DateTime.UtcNow},
                        { "UpdationDate", null}
                    };

                    AddValues.AddKeys(AdditionalProperties);
                    bsonDoc = bsonDoc.AddRange(AddValues);

                    BeforeInsert(bsonDoc);
                    Collection.InsertOne(bsonDoc);
                    AfterInsert(bsonDoc);

                    //Create Collection
                    Context.Database.CreateCollection(GeneratedCollectionId);
                    SetCollectionIndexes<T>(GeneratedCollectionId);

                    //Return GeneratedCollectionId if ParentCollectionId is null
                    RetrivedCollectionId = string.IsNullOrWhiteSpace(parentCollectionId) ? GeneratedCollectionId : parentCollectionId;
                }
                else if (count > 1) throw new SystemException("More than one collections found for the criterion");
                else
                {
                    //Get CollectionId
                    RetrivedCollectionId = query.FirstOrDefault().GetValue("CollectionId").AsString;

                    string parentCollectionId = GetRootCollectionId(KeyValues, Distribution);
                    if (RetrivedCollectionId != parentCollectionId) SetParentUpdateDateTime(KeyValues, parentCollectionId);
                }
            }

            if (RetrivedCollectionId == null) throw new SystemException("Collection with the criterion does not exists");
            return RetrivedCollectionId;
        }

        public bool SetUpdateDateTime(string collectionId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("CollectionId", collectionId);
            UpdateResult updateResult = Collection.UpdateOne(filter, Builders<BsonDocument>.Update.Set("UpdationDate", DateTime.UtcNow));
            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

        private bool SetParentUpdateDateTime(Dictionary<string, object> keyValues, string parentCollectionId)
        {
            //Load base's CollectionId
            var tempCollection = Context.Database.GetCollection<BsonDocument>(parentCollectionId);
            BsonDocument searchDocumentByKV = Utility.CreateSearchBsonDocument(keyValues);

            //Update DateTime
            UpdateResult updateResult = tempCollection.UpdateOne(searchDocumentByKV, Builders<BsonDocument>.Update.Set("UpdationDate", DateTime.UtcNow));
            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

        private void SetCollectionIndexes<T>(string collectionName) where T : BaseEntity
        {
            IMongoCollection<T> Collection = Context.Database.GetCollection<T>(collectionName);
            TimeSpan timeSpan = new TimeSpan(TimeSpan.TicksPerSecond * ExpireAfterSeconds);

            //Create index for CreationDate (descending) and Expires after 'ExpireAfterSecondsTimeSpan'
            var indexBuilder = Builders<T>.IndexKeys;
            var indexModel = new CreateIndexModel<T>(indexBuilder.Descending(x => x.CreationDate), new CreateIndexOptions { ExpireAfter = timeSpan, Name = "CreationDateIndex" });
            Collection.Indexes.CreateOne(indexModel);
        }

        private string GetRootCollectionId(Dictionary<string, object> keyValues, Dictionary<string, int> distribution)
        {
            //Deep copy
            Dictionary<string, object> keyValuesCopy = new Dictionary<string, object>(keyValues);

            //Get Max level key for identifiying correct collection to insert message into.
            var validKeys = keyValuesCopy.Where(x => x.Value != null).Select(x => x.Key).ToList();
            var toAggrDist = distribution.Where(v => validKeys.Contains(v.Key)).ToDictionary(x => x.Key, x => x.Value);
            var MaxKeyValue = toAggrDist.Aggregate((l, r) => l.Value > r.Value ? l : r);
            if (MaxKeyValue.Value != 0) keyValuesCopy.Remove(MaxKeyValue.Key);

            BsonDocument searchDocumentByKV = Utility.CreateSearchBsonDocument(keyValuesCopy, ignoreNullValues: true);
            var query = Collection.Find(searchDocumentByKV);

            //Return the base's CollectionId
            return query.FirstOrDefault()?.GetValue("CollectionId").AsString;
        }

        private void BeforeInsert(BsonDocument doc)
        {
            foreach (var prop in PropertiesBeforeInsert)
            {
                doc.SetElement(new BsonElement(prop.Key, BsonValue.Create(prop.Value)));
            }
        }

        private void AfterInsert(BsonDocument doc)
        {
            var filterDef = new FilterDefinitionBuilder<BsonDocument>();
            var updateDef = new UpdateDefinitionBuilder<BsonDocument>();

            foreach (var prop in PropertiesAfterInsert)
            {
                Collection.UpdateOne(filterDef.Eq("CollectionId", doc.GetValue("CollectionId")), updateDef.Set(prop.Key, BsonValue.Create(prop.Value)));
            }
        }
    }
}
