using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TK.MongoDB.Distributed.Models;

namespace TK.MongoDB.Distributed.Data
{
    /// <summary>
    /// Data Repository
    /// </summary>
    /// <typeparam name="T">Type of BaseEntity</typeparam>
    public interface IRepository<T> : IDisposable where T : BaseEntity
    {
        /// <summary>
        /// Find single document by condition specified.
        /// </summary>
        /// <param name="collectionId">Targeted Collection Id</param>
        /// <param name="condition">Lamda expression</param>
        /// <returns>Document</returns>
        Task<T> FindAsync(string collectionId, Expression<Func<T, bool>> condition);

        /// <summary>
        /// Gets document by condition specified or gets all documents if condition is not passed. Paged records.
        /// </summary>
        /// <param name="collectionId">Targeted Collection Id</param>
        /// <param name="currentPage">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="condition">Lamda expression</param>
        /// <param name="orderBy">Order by column</param>
        /// <param name="orderByDescending">Order By Descending</param>
        /// <returns>Tuple of records and total number of records</returns>
        Task<Tuple<IEnumerable<T>, long>> GetAsync(string collectionId, int currentPage, int pageSize, Expression<Func<T, bool>> condition = null, Expression<Func<T, object>> orderBy = null, bool orderByDescending = true);

        /// <summary>
        /// Gets document by filter specified. Paged records.
        /// </summary>
        /// <param name="collectionId">Targeted Collection Id</param>
        /// <param name="currentPage">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="filter">Filter definition</param>
        /// <param name="sort">Sort definition</param>
        /// <returns>Tuple of records and total number of records</returns>
        Task<Tuple<IEnumerable<T>, long>> GetAsync(string collectionId, int currentPage, int pageSize, FilterDefinition<T> filter, SortDefinition<T> sort = null);

        /// <summary>
        /// Gets document by condition specified or gets all documents if condition is not passed.
        /// </summary>
        /// <param name="collectionId">Targeted Collection Id</param>
        /// <param name="condition">Lamda expression</param>
        /// <param name="orderBy">Order by column</param>
        /// <param name="orderByDescending">Order By Descending</param>
        /// <returns>Enumerable records</returns>
        Task<IEnumerable<T>> GetAsync(string collectionId, Expression<Func<T, bool>> condition = null, Expression<Func<T, object>> orderBy = null, bool orderByDescending = true);

        /// <summary>
        /// Gets document by filter specified.
        /// </summary>
        /// <param name="collectionId">Targeted Collection Id</param>
        /// <param name="filter">Filter definition</param>
        /// <param name="sort">Sort definition</param>
        /// <returns>Enumerable records</returns>
        Task<IEnumerable<T>> GetAsync(string collectionId, FilterDefinition<T> filter, SortDefinition<T> sort = null);

        /// <summary>
        /// Inserts single record.
        /// </summary>
        /// <param name="instance">Document</param>
        /// <returns>Document</returns>
        Task<InsertResult<T>> InsertAsync(T instance);

        /// <summary>
        /// Inserts single record.
        /// </summary>
        /// <param name="instance">Document</param>
        /// <returns>Document</returns>
        InsertResult<T> Insert(T instance);

        /// <summary>
        /// Inserts single record.
        /// </summary>
        /// <param name="collectionId">Targeted Collection Id</param>
        /// <param name="instance">Document</param>
        /// <returns>Document</returns>
        Task<InsertResult<T>> InsertAsync(string collectionId, T instance);

        /// <summary>
        /// Updates single record based on Id.
        /// </summary>
        /// <param name="collectionId">Targeted Collection Id</param>
        /// <param name="instance">Document</param>
        /// <returns>Boolean</returns>
        Task<UpdateResult<T>> UpdateAsync(string collectionId, T instance);

        /// <summary>
        /// Bulk update records based on their Id with IsUpsert = false.
        /// </summary>
        /// <param name="collectionId">Targeted Collection Id</param>
        /// <param name="instances">Documents to update</param>
        /// <returns>Number of documents updated</returns>
        Task<long> BulkUpdateAsync(string collectionId, IEnumerable<T> instances);

        /// <summary>
        /// Deletes record based on Id hard or soft based on logical value.
        /// </summary>
        /// <param name="collectionId">Targeted Collection Id</param>
        /// <param name="id">Key</param>
        /// <param name="logical">Soft delete</param>
        /// <returns>Boolean</returns>
        Task<bool> DeleteAsync(string collectionId, ObjectId id, bool logical = true);

        /// <summary>
        /// Counts documents based on condition specifed or counts all documents if condition is not passed.
        /// </summary>
        /// <param name="collectionId">Targeted Collection Id</param>
        /// <param name="condition">Lamda expression</param>
        /// <returns>Count</returns>
        Task<long> CountAsync(string collectionId, Expression<Func<T, bool>> condition = null);

        /// <summary>
        /// Counts documents based on filter specifed.
        /// </summary>
        /// <param name="collectionId">Targeted Collection Id</param>
        /// <param name="filter">Filter definition</param>
        /// <returns>Count</returns>
        Task<long> CountAsync(string collectionId, FilterDefinition<T> filter);

        /// <summary>
        /// Checks if the document exists based on the condition specified.
        /// </summary>
        /// <param name="collectionId">Targeted Collection Id</param>
        /// <param name="condition">Lamda expression</param>
        /// <returns>Boolean</returns>
        Task<bool> ExistsAsync(string collectionId, Expression<Func<T, bool>> condition);
    }
}
