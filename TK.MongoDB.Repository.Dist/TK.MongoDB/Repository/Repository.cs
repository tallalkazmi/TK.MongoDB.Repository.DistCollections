using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using TK.MongoDB.Models;

namespace TK.MongoDB
{
    public class Repository<T> : Settings, IRepository<T> where T : BaseEntity<ObjectId>
    {
        protected MongoDBContext Context { get; private set; }
        protected IMongoCollection<T> Collection { get; private set; }

        public Repository()
        {
            //if (distribution == null || distribution.Count() == 0)
            //    distribution = GetDistribution();

            Context = new MongoDBContext(ConnectionStringSettingName);
            //CollectionName = typeof(T).Name.ToLower();
            //Collection = Context.Database.GetCollection<T>(CollectionName);

            ////Create index for CreationDate (descending)
            //var indexBuilder = Builders<T>.IndexKeys;
            //var indexModel = new CreateIndexModel<T>(indexBuilder.Descending(x => x.CreationDate), new CreateIndexOptions { Name = "CreationDateIndex" });
            //Collection.Indexes.CreateOneAsync(indexModel);
        }

        public async Task<T> FindAsync(string collectionId, Expression<Func<T, bool>> condition)
        {
            Collection = Context.Database.GetCollection<T>(collectionId);

            var query = await Collection.FindAsync<T>(condition);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<Tuple<IEnumerable<T>, long>> GetAsync(string collectionId, int currentPage, int pageSize, Expression<Func<T, bool>> condition = null)
        {
            Collection = Context.Database.GetCollection<T>(collectionId);

            if (condition == null) condition = _ => true;
            var query = Collection.Find<T>(condition);
            long totalCount = await query.CountDocumentsAsync();
            List<T> records = await query.SortByDescending(x => x.CreationDate).Skip((currentPage - 1) * pageSize).Limit(pageSize).ToListAsync();
            return new Tuple<IEnumerable<T>, long>(records, totalCount);
        }

        public async Task<T> InsertAsync(T instance)
        {
            SetCollectionFromMaster(instance);

            instance.Id = ObjectId.GenerateNewId();
            instance.CreationDate = DateTime.UtcNow;
            instance.UpdationDate = null;
            await Collection.InsertOneAsync(instance);
            return instance;
        }

        public async Task<T> InsertAsync(string collectionId, T instance)
        {
            Collection = Context.Database.GetCollection<T>(collectionId);

            instance.Id = ObjectId.GenerateNewId();
            instance.CreationDate = DateTime.UtcNow;
            instance.UpdationDate = null;
            await Collection.InsertOneAsync(instance);
            return instance;
        }

        public async Task<bool> UpdateAsync(string collectionId, T instance)
        {
            Collection = Context.Database.GetCollection<T>(collectionId);

            var query = await Collection.FindAsync<T>(x => x.Id == instance.Id);
            T _instance = await query.FirstOrDefaultAsync();
            if (_instance == null) throw new KeyNotFoundException($"Object with Id: '{instance.Id}' was not found.");
            else
            {
                instance.CreationDate = _instance.CreationDate;
                instance.UpdationDate = DateTime.UtcNow;
            }

            ReplaceOneResult result = await Collection.ReplaceOneAsync<T>(x => x.Id == instance.Id, instance);
            return result.ModifiedCount != 0;
        }

        public async Task<bool> DeleteAsync(string collectionId, ObjectId id, bool logical = true)
        {
            Collection = Context.Database.GetCollection<T>(collectionId);

            var query = await Collection.FindAsync<T>(x => x.Id == id);
            T _instance = await query.FirstOrDefaultAsync();
            if (_instance == null)
                throw new KeyNotFoundException($"Object with Id: '{id}' was not found.");

            if (logical)
            {
                UpdateDefinition<T> update = Builders<T>.Update
                    .Set(x => x.Deleted, true)
                    .Set(x => x.UpdationDate, DateTime.UtcNow);
                UpdateResult result = await Collection.UpdateOneAsync(x => x.Id == id, update);
                return result.ModifiedCount != 0;
            }
            else
            {
                DeleteResult result = await Collection.DeleteOneAsync(x => x.Id == id);
                return result.DeletedCount != 0;
            }
        }

        public async Task<long> CountAsync(string collectionId, Expression<Func<T, bool>> condition = null)
        {
            Collection = Context.Database.GetCollection<T>(collectionId);

            if (condition == null) condition = _ => true;
            return await Collection.CountDocumentsAsync(condition);
        }

        public async Task<bool> ExistsAsync(string collectionId, Expression<Func<T, bool>> condition)
        {
            Collection = Context.Database.GetCollection<T>(collectionId);

            var result = await CountAsync(collectionId, condition);
            return result > 0;
        }

        public void Dispose()
        {
            if (Context != null)
                Context.Dispose();
        }

        //private IDictionary<string, int> GetDistribution()
        //{
        //    Dictionary<string, int> distribution = new Dictionary<string, int>();
        //    PropertyInfo[] props = typeof(T).GetProperties();

        //    foreach (PropertyInfo prop in props)
        //    {
        //        DistributionAttribute distAttr = (DistributionAttribute)Attribute.GetCustomAttribute(prop, typeof(DistributionAttribute));
        //        if (distAttr == null) continue;
        //        distribution.Add(prop.Name, distAttr.Order);
        //    }

        //    return distribution;
        //}

        //private void SetCollection(T obj)
        //{
        //    string collectionName = string.Empty;
        //    foreach (var item in distribution.OrderBy(x => x.Value))
        //    {
        //        var value = typeof(T).GetProperty(item.Key).GetValue(obj);
        //        if (value == null) continue;

        //        if (!string.IsNullOrWhiteSpace(collectionName)) collectionName += "_";
        //        collectionName += $"{item.Key}:{value}";
        //    }

        //    Collection = Context.Database.GetCollection<T>(collectionName);
        //}

        private void SetCollectionFromMaster(T obj)
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
            var MasterCollection = Context.Database.GetCollection<BsonDocument>("Master");
            var _1stDoc = MasterCollection.Find<BsonDocument>(new BsonDocument()).Limit(1).SingleOrDefault();

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
                MasterCollection.InsertOne(bsonDoc);

                //Create Collection
                Context.Database.CreateCollection(GeneratedCollectionId);
                SetCollectionIndexes(GeneratedCollectionId);
                result = GeneratedCollectionId;
            }
            else
            {
                //Create BsonDocument for search
                BsonDocument searchDocument = SearchBsonDocument(KeyValues);
                var query = MasterCollection.Find(searchDocument);

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
                    MasterCollection.InsertOne(bsonDoc);

                    //Create Collection
                    Context.Database.CreateCollection(GeneratedCollectionId);
                    SetCollectionIndexes(GeneratedCollectionId);

                    //Get Max level key for identifiying correct collection to insert message into.
                    var validKeys = KeyValues.Where(x => x.Value != null).Select(x => x.Key).ToList();
                    var toAggrDist = Distribution.Where(v => validKeys.Contains(v.Key)).ToDictionary(x => x.Key, x => x.Value);
                    var MaxKeyValue = toAggrDist.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                    KeyValues.Remove(MaxKeyValue);

                    BsonDocument newSearchDocument = SearchBsonDocument(KeyValues);
                    var query2 = MasterCollection.Find(newSearchDocument);

                    //Return the base's CollectionId
                    result = query2.FirstOrDefault().GetValue("CollectionId").AsString;
                }
                else if (count > 1) throw new Exception("More than one collections found for the criterion");
                
                //Get CollectionId
                result = query.FirstOrDefault().GetValue("CollectionId").AsString;
            }

            if (result == null) throw new ArgumentException("Collection with the criterion does not exists");
            Collection = Context.Database.GetCollection<T>(result);
        }

        private BsonDocument SearchBsonDocument(IDictionary<string, object> keyValues)
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

        private void SetCollectionIndexes(string collectionName)
        {
            IMongoCollection<T> Collection = Context.Database.GetCollection<T>(collectionName);
            TimeSpan timeSpan = new TimeSpan(TimeSpan.TicksPerSecond * ExpireAfterSeconds);

            //Create index for CreationDate (descending) and Expires after 'ExpireAfterSecondsTimeSpan'
            var indexBuilder = Builders<T>.IndexKeys;
            var indexModel = new CreateIndexModel<T>(indexBuilder.Descending(x => x.CreationDate), new CreateIndexOptions { ExpireAfter = timeSpan, Name = "CreationDateIndex" });
            Collection.Indexes.CreateOne(indexModel);
        }

        #region Master Records
        public async Task <string> MasterGet()
        {
            var collection = Context.Database.GetCollection<BsonDocument>("Master");

            var records = await collection.Find(new BsonDocument())
                .Project(Builders<BsonDocument>.Projection.Exclude("_id"))
                .Sort(Builders<BsonDocument>.Sort.Descending("CreationDate"))
                .ToListAsync();

            var writterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Shell };
            var result = records.ToJson();
            return result;
        }

        public async Task<string> MasterGet(IDictionary<string,object> keyValuePairs)
        {
            var collection = Context.Database.GetCollection<BsonDocument>("Master");
            var searchDocument = SearchBsonDocument(keyValuePairs);

            var records = await collection.Find(searchDocument)
                .Project(Builders<BsonDocument>.Projection.Exclude("_id"))
                .Sort(Builders<BsonDocument>.Sort.Descending("CreationDate"))
                .ToListAsync();

            var writterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Shell };
            var result = records.ToJson();
            return result;
        }

        public async Task<bool> MasterUpdateName(string collectionId, string name)
        {
            var collection = Context.Database.GetCollection<BsonDocument>("Master");

            var filter = Builders<BsonDocument>.Filter.Eq("CollectionId", collectionId);
            UpdateResult updateResult = await collection.UpdateOneAsync(filter, Builders<BsonDocument>.Update.Set("Name", name).Set("UpdationDate", DateTime.UtcNow));
            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

        #endregion
    }
}
