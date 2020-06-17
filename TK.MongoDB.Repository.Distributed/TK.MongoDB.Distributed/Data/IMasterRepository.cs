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
        /// Gets a single document.
        /// </summary>
        /// <param name="filter">Filter Definition</param>
        /// <returns>Document</returns>
        Task<BsonDocument> FindAsync(FilterDefinition<BsonDocument> filter);

        /// <summary>
        /// Gets a single document.
        /// </summary>
        /// <param name="filter">Filter Definition</param>
        /// <returns>Document</returns>
        BsonDocument Find(FilterDefinition<BsonDocument> filter);

        /// <summary>
        /// Gets all documents
        /// </summary>
        /// <param name="currentPage">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Documents</returns>
        Task<Tuple<IEnumerable<BsonDocument>, long>> GetAsync(int currentPage, int pageSize);

        /// <summary>
        /// Gets documents satisfying KeyValuePairs with 'AND' operand
        /// </summary>
        /// <param name="currentPage">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="keyValuePairs">Element name and value to search for</param>
        /// <param name="orderbyColumn">Column to Order by</param>
        /// <param name="orderByDescending">Order by Descending</param>
        /// <returns>Documents</returns>
        Task<Tuple<IEnumerable<BsonDocument>, long>> GetAsync(int currentPage, int pageSize, IDictionary<string, object> keyValuePairs, string orderbyColumn = "CreationDate", bool orderByDescending = true);

        /// <summary>
        /// Gets documents satisfying filter condition
        /// </summary>
        /// <param name="currentPage">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="filter">Filter Definition</param>
        /// <param name="sort">Sort Definition</param>
        /// <returns>Documents</returns>
        Task<Tuple<IEnumerable<BsonDocument>, long>> GetAsync(int currentPage, int pageSize, FilterDefinition<BsonDocument> filter, SortDefinition<BsonDocument> sort = null);

        /// <summary>
        /// Updates 'Name' of a document in the collection identified by 'Collection Id'
        /// </summary>
        /// <param name="collectionId">Collection Id to update</param>
        /// <param name="property">Property name to update</param>
        /// <param name="value">Property value to update</param>
        /// <returns>Boolean</returns>
        Task<bool> UpdateAsync(string collectionId, string property, object value);
    }
}
