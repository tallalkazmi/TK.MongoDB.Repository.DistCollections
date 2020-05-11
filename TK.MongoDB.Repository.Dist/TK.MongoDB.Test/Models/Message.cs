using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TK.MongoDB.Models;

namespace TK.MongoDB.Test.Models
{
    public class Message : BaseEntity<ObjectId>
    {
        [BsonRequired]
        public string Text { get; set; }


        [Distribute]
        public long Caterer { get; set; }


        [Distribute]
        public long Client { get; set; }


        [Distribute(Level = 1)]
        public long? Order { get; set; }
    }
}
