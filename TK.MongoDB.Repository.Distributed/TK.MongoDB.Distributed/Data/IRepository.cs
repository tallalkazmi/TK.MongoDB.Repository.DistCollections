using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TK.MongoDB.Distributed.Models;

namespace TK.MongoDB.Distributed.Data
{
    public interface IRepository<T> : IDisposable where T : BaseEntity<ObjectId>
    {
        /// <summary>
        /// Find single document by condition specified.
        /// </summary>
        /// <param name="condition">Lamda expression</param>
        /// <returns>Document</returns>
        Task<T> FindAsync(string collectionId, Expression<Func<T, bool>> condition);

        /// <summary>
        /// Gets document by condition specified or gets all documents if condition is not passed. Paged records.
        /// </summary>
        /// <param name="currentPage">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="condition">Lamda expression</param>
        /// <returns>Tuple of records and total number of records</returns>
        Task<Tuple<IEnumerable<T>, long>> GetAsync(string collectionId, int currentPage, int pageSize, Expression<Func<T, bool>> condition = null);

        /// <summary>
        /// Inserts single record.
        /// </summary>
        /// <param name="instance">Document</param>
        /// <returns>Document</returns>
        Task<T> InsertAsync(T instance);

        /// <summary>
        /// Inserts single record.
        /// </summary>
        /// <param name="collectionId">Collection Id to insert data into</param>
        /// <param name="instance">Document</param>
        /// <returns>Document</returns>
        Task<T> InsertAsync(string collectionId, T instance);

        /// <summary>
        /// Updates single record based on Id.
        /// </summary>
        /// <param name="instance">Document</param>
        /// <returns></returns>
        Task<bool> UpdateAsync(string collectionId, T instance);

        /// <summary>
        /// Deletes record based on Id hard or soft based on logical value.
        /// </summary>
        /// <param name="id">Key</param>
        /// <param name="logical">Soft delete</param>
        /// <returns></returns>
        Task<bool> DeleteAsync(string collectionId, ObjectId id, bool logical = true);

        /// <summary>
        /// Counts documents based on condition specifed or counts all documents if condition is not passed.
        /// </summary>
        /// <param name="condition">Lamda expression</param>
        /// <returns></returns>
        Task<long> CountAsync(string collectionId, Expression<Func<T, bool>> condition = null);

        /// <summary>
        /// Checks if the document exists based on the condition specified.
        /// </summary>
        /// <param name="condition">Lamda expression</param>
        /// <returns></returns>
        Task<bool> ExistsAsync(string collectionId, Expression<Func<T, bool>> condition);
    }
}
