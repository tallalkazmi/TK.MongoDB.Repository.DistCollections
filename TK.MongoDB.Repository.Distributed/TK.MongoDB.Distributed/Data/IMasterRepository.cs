using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TK.MongoDB.Distributed.Data
{
    /// <summary>
    /// Master data repository
    /// </summary>
    public interface IMasterRepository : IDisposable
    {
        /// <summary>
        /// Gets all documents
        /// </summary>
        /// <param name="currentPage">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Documents</returns>
        Task<Tuple<IEnumerable<object>, long>> GetAsync(int currentPage, int pageSize);

        /// <summary>
        /// Gets documents satisfying KeyValuePairs with 'AND' operand
        /// </summary>
        /// <param name="currentPage">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="keyValuePairs">Element name and value to search for</param>
        /// <returns>Documents</returns>
        Task<Tuple<IEnumerable<object>, long>> GetAsync(int currentPage, int pageSize, IDictionary<string, object> keyValuePairs);

        /// <summary>
        /// Gets documents satisfying filter condition
        /// </summary>
        /// <param name="currentPage">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="filter">Filter Definition</param>
        /// <returns>Documents</returns>
        Task<Tuple<IEnumerable<object>, long>> GetAsync(int currentPage, int pageSize, FilterDefinition<BsonDocument> filter);

        /// <summary>
        /// Updates 'Name' of a document in the collection identified by 'Collection Id'
        /// </summary>
        /// <param name="collectionId">Collection Id to update</param>
        /// <param name="name">New 'name' value</param>
        /// <returns></returns>
        Task<bool> UpdateAsync(string collectionId, string name);
    }
}
