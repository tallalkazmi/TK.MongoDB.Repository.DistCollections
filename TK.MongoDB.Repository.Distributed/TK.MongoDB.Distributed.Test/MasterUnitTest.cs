using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TK.MongoDB.Distributed.Test.ViewModels;

namespace TK.MongoDB.Distributed.Test
{
    [TestClass]
    public class MasterUnitTest : BaseTest
    {
        public MasterUnitTest()
        {
            Settings.ConnectionStringSettingName = "MongoDocConnection";
        }

        [TestMethod]
        public async Task Find()
        {
            var builder = Builders<BsonDocument>.Filter;
            var collectionFilter = builder.Eq("CollectionId", "c7a7935f1ebd440e9b85003c1b81b3c3");

            var result = await MasterRepository.FindAsync(collectionFilter);
            var record = BsonSerializer.Deserialize<MasterGetViewModel>(result.ToJson());
            Console.WriteLine($"Output:{JToken.Parse(JsonConvert.SerializeObject(record)).ToString(Formatting.Indented)}");
        }

        [TestMethod]
        public async Task Get()
        {
            var result = await MasterRepository.GetAsync(1, 20);
            var records = BsonSerializer.Deserialize<IEnumerable<MasterGetViewModel>>(result.Item1.ToJson());
            Console.WriteLine($"Output:Total:{result.Item2}\nRecords:{JToken.Parse(JsonConvert.SerializeObject(records)).ToString(Formatting.Indented)}");
        }

        [TestMethod]
        public async Task GetByKeys()
        {
            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { "Client", 1 }
            };

            var result = await MasterRepository.GetAsync(1, 20, keyValuePairs);
            var records = BsonSerializer.Deserialize<IEnumerable<MasterGetViewModel>>(result.Item1.ToJson());
            Console.WriteLine($"Output:Total:{result.Item2}\nRecords:{JToken.Parse(JsonConvert.SerializeObject(records)).ToString(Formatting.Indented)}");
        }

        [TestMethod]
        public async Task GetByDefinition()
        {
            var builder = Builders<BsonDocument>.Filter;

            var clientFilter = builder.Eq("Client", 1);
            var catererFilter = builder.Eq("Caterer", 1);
            var filter = clientFilter & catererFilter;

            var result = await MasterRepository.GetAsync(1, 20, filter);
            var records = BsonSerializer.Deserialize<IEnumerable<MasterGetViewModel>>(result.Item1.ToJson());
            Console.WriteLine($"Output:Total:{result.Item2}\nRecords:{JToken.Parse(JsonConvert.SerializeObject(records)).ToString(Formatting.Indented)}");
        }

        [TestMethod]
        public async Task Update()
        {
            bool result = await MasterRepository.UpdateAsync("53df73c45d7e493b86746066a693534c", "Name", "Untitled-3");
            Console.WriteLine($"Updated: {result}");
        }
    }
}
