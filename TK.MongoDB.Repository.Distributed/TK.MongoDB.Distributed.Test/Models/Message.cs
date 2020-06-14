using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System;
using System.Collections.Generic;
using TK.MongoDB.Distributed.Models;

namespace TK.MongoDB.Distributed.Test.Models
{
    public class Message : BaseEntity
    {
        [BsonRequired]
        public string Text { get; set; }


        [Distribute]
        public long Caterer { get; set; }


        [Distribute]
        public long Client { get; set; }


        [Distribute(Level = 1)]
        public long? Order { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<Guid, DateTime> Read { get; set; }
    }
}
