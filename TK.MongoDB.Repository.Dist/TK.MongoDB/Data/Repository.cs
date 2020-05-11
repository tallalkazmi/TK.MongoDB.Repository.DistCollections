﻿using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TK.MongoDB.Classes;
using TK.MongoDB.Models;

namespace TK.MongoDB.Data
{
    public class Repository<T> : Settings, IRepository<T> where T : BaseEntity<ObjectId>
    {
        private readonly Master Master;
        private readonly Expression<Func<T, bool>> IsDeleted;

        protected MongoDBContext Context { get; private set; }
        protected IMongoCollection<T> Collection { get; private set; }

        public Repository()
        {
            Context = new MongoDBContext(ConnectionStringSettingName);
            Master = new Master();
            IsDeleted = x => x.Deleted == false;
        }

        public async Task<T> FindAsync(string collectionId, Expression<Func<T, bool>> condition)
        {
            Collection = Context.Database.GetCollection<T>(collectionId);

            var body = Expression.AndAlso(condition, IsDeleted);
            var lamda = Expression.Lambda<Func<T, bool>>(body, condition.Parameters[0]);

            var query = await Collection.FindAsync<T>(lamda);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<Tuple<IEnumerable<T>, long>> GetAsync(string collectionId, int currentPage, int pageSize, Expression<Func<T, bool>> condition = null)
        {
            Collection = Context.Database.GetCollection<T>(collectionId);

            if (condition == null) condition = _ => true;
            var body = Expression.AndAlso(condition, IsDeleted);
            var lamda = Expression.Lambda<Func<T, bool>>(body, condition.Parameters[0]);
            
            var query = Collection.Find<T>(lamda);

            long totalCount = await query.CountDocumentsAsync();
            List<T> records = await query.SortByDescending(x => x.CreationDate).Skip((currentPage - 1) * pageSize).Limit(pageSize).ToListAsync();
            return new Tuple<IEnumerable<T>, long>(records, totalCount);
        }

        public async Task<T> InsertAsync(T instance)
        {
            string CollectionName = Master.RetriveCollectionFromMaster(instance);
            Collection = Context.Database.GetCollection<T>(CollectionName);

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
            if (condition == null) condition = _ => true;
            var body = Expression.AndAlso(condition, IsDeleted);
            var lamda = Expression.Lambda<Func<T, bool>>(body, condition.Parameters[0]);

            return await Collection.CountDocumentsAsync(lamda);
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
    }
}
