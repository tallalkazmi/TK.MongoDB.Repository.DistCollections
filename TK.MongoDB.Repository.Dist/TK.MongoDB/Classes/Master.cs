using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TK.MongoDB.Models;

namespace TK.MongoDB.Classes
{
    public class Master : Settings
    {
        private readonly MongoDBContext Context;
        private readonly IMongoCollection<BsonDocument> Collection;

        public Master()
        {
            Context = new MongoDBContext(ConnectionStringSettingName);
            Collection = Context.Database.GetCollection<BsonDocument>(MasterCollectionName);
        }

        public string RetriveCollectionFromMaster<T>(T obj) where T : BaseEntity<ObjectId>
        {
            // Resulted Collection Name
            string result = null;

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
                    { "CreationDate", DateTime.UtcNow},
                    { "UpdationDate", null}
                };

                bsonDoc = bsonDoc.AddRange(AddValues);
                Collection.InsertOne(bsonDoc);

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
                result = GeneratedCollectionId;
            }
            else
            {
                //Create BsonDocument for search
                BsonDocument searchDocument = Utility.CreateSearchBsonDocument(KeyValues);
                var query = Collection.Find(searchDocument);

                //If BsonDocument doesn't exists, create one.
                long count = query.CountDocuments();
                if (count == 0)
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
                        { "CreationDate", DateTime.UtcNow},
                        { "UpdationDate", null}
                    };

                    bsonDoc = bsonDoc.AddRange(AddValues);
                    Collection.InsertOne(bsonDoc);

                    //Create Collection
                    Context.Database.CreateCollection(GeneratedCollectionId);
                    SetCollectionIndexes<T>(GeneratedCollectionId);

                    //Get Max level key for identifiying correct collection to insert message into.
                    var validKeys = KeyValues.Where(x => x.Value != null).Select(x => x.Key).ToList();
                    var toAggrDist = Distribution.Where(v => validKeys.Contains(v.Key)).ToDictionary(x => x.Key, x => x.Value);
                    var MaxKeyValue = toAggrDist.Aggregate((l, r) => l.Value > r.Value ? l : r);
                    if (MaxKeyValue.Value != 0) KeyValues.Remove(MaxKeyValue.Key);

                    BsonDocument newSearchDocument = Utility.CreateSearchBsonDocument(KeyValues);
                    var query2 = Collection.Find(newSearchDocument);

                    //Return the base's CollectionId
                    result = query2.FirstOrDefault().GetValue("CollectionId").AsString;
                }
                else if (count > 1) throw new Exception("More than one collections found for the criterion");
                else
                {
                    //Get CollectionId
                    result = query.FirstOrDefault().GetValue("CollectionId").AsString;
                }
            }

            if (result == null) throw new ArgumentException("Collection with the criterion does not exists");
            return result;
        }

        private void SetCollectionIndexes<T>(string collectionName) where T : BaseEntity<ObjectId>
        {
            IMongoCollection<T> Collection = Context.Database.GetCollection<T>(collectionName);
            TimeSpan timeSpan = new TimeSpan(TimeSpan.TicksPerSecond * ExpireAfterSeconds);

            //Create index for CreationDate (descending) and Expires after 'ExpireAfterSecondsTimeSpan'
            var indexBuilder = Builders<T>.IndexKeys;
            var indexModel = new CreateIndexModel<T>(indexBuilder.Descending(x => x.CreationDate), new CreateIndexOptions { ExpireAfter = timeSpan, Name = "CreationDateIndex" });
            Collection.Indexes.CreateOne(indexModel);
        }
    }
}
