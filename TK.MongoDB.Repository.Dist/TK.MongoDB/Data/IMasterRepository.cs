using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TK.MongoDB.Data
{
    public interface IMasterRepository : IDisposable
    {
        /// <summary>
        /// Gets all documents
        /// </summary>
        /// <returns>Documents</returns>
        Task<IEnumerable<object>> Get();

        /// <summary>
        /// Gets documents satisfying KeyValuePairs with 'AND' operand
        /// </summary>
        /// <param name="keyValuePairs">Element name and value to search for</param>
        /// <returns>Documents</returns>
        Task<IEnumerable<object>> Get(IDictionary<string, object> keyValuePairs);

        /// <summary>
        /// Gets documents satisfying filter condition
        /// </summary>
        /// <param name="filter">Lambda expression</param>
        /// <returns>Documents</returns>
        Task<IEnumerable<object>> Get(Expression<Func<BsonDocument, bool>> filter);

        /// <summary>
        /// Updates 'Name' of a document in the collection identified by 'Collection Id'
        /// </summary>
        /// <param name="collectionId">Collection Id to update</param>
        /// <param name="name">New 'name' value</param>
        /// <returns></returns>
        Task<bool> Update(string collectionId, string name);
    }
}
