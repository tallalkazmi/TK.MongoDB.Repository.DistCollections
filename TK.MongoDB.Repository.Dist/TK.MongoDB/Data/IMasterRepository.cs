using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TK.MongoDB.Data
{
    public interface IMasterRepository : IDisposable
    {
        Task<string> Get();
        Task<string> Get(IDictionary<string, object> keyValuePairs);
        Task<bool> Update(string collectionId, string name);
    }
}
