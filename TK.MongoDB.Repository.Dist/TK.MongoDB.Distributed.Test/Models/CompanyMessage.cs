using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TK.MongoDB.Models;

namespace TK.MongoDB.Test.Models
{
    public class CompanyMessage : BaseEntity<ObjectId>
    {
        [BsonRequired]
        public string Text { get; set; }


        [Distribute]
        public long Company { get; set; }
    }
}
