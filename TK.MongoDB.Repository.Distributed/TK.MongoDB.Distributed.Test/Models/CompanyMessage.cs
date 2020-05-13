using MongoDB.Bson.Serialization.Attributes;
using TK.MongoDB.Distributed.Models;

namespace TK.MongoDB.Distributed.Test.Models
{
    public class CompanyMessage : BaseEntity
    {
        [BsonRequired]
        public string Text { get; set; }


        [Distribute]
        public long Company { get; set; }
    }
}
