using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TK.MongoDB.Distributed.Test.ViewModels
{
    [BsonIgnoreExtraElements]
    public class MasterGetViewModel
    {
        public string Name { get; set; }
        public string CollectionId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? UpdationDate { get; set; }
        public long? ClientId { get; set; }
        public long? CatererId { get; set; }
        public long? OrderId { get; set; }
        public long? CompanyId { get; set; }
        //public Guid? CreatedBy { get; set; }
        //public Guid? UpdatedBy { get; set; }
    }
}
