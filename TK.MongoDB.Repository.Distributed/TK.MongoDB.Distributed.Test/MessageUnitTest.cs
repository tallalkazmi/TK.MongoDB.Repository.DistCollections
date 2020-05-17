using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TK.MongoDB.Distributed.Test.Models;
using TK.MongoDB.Distributed.Test.ViewModels;

namespace TK.MongoDB.Distributed.Test
{
    [TestClass]
    public class MessageUnitTest : BaseTest
    {
        readonly string CollectionId;

        public MessageUnitTest()
        {
            Settings.ConnectionStringSettingName = "MongoDocConnection";
            MasterSettings.AdditionalProperties = new string[] { "CreatedBy" };

            CollectionId = "2b9f7ce62870424e84cfeedcaf2670fe";
        }

        [TestMethod]
        public async Task Find()
        {
            Message result = await MessageRepository.FindAsync(CollectionId, x => x.Text == "xyz" && x.Client == 1);
            Console.WriteLine($"Output:\n{JToken.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
        }

        [TestMethod]
        public async Task Get()
        {
            var result = await MessageRepository.GetAsync(CollectionId, 1, 10, x => x.Text.Contains("abc"));
            Console.WriteLine($"Output:\nTotal: {result.Item2}\n{JToken.Parse(JsonConvert.SerializeObject(result.Item1)).ToString(Formatting.Indented)}");
        }

        [TestMethod]
        public async Task Search()
        {
            MessageSearchParameters searchParameters = new MessageSearchParameters()
            {
                Text = "Change",
                Caterer = null,
                Client = null,
                Order = null
            };

            var builder = Builders<Message>.Filter;
            var filter = builder.Empty;
            if (!string.IsNullOrWhiteSpace(searchParameters.Text))
            {
                var criteriaFilter = builder.Regex(x => x.Text, new BsonRegularExpression($".*{searchParameters.Text}.*"));
                filter &= criteriaFilter;
            }

            if (searchParameters.Caterer.HasValue)
            {
                var criteriaFilter = builder.Eq(x => x.Caterer, searchParameters.Caterer.Value);
                filter &= criteriaFilter;
            }

            if (searchParameters.Client.HasValue)
            {
                var criteriaFilter = builder.Eq(x => x.Client, searchParameters.Client.Value);
                filter &= criteriaFilter;
            }

            if (searchParameters.Order.HasValue)
            {
                var criteriaFilter = builder.Eq(x => x.Order, searchParameters.Order.Value);
                filter &= criteriaFilter;
            }

            var result = await MessageRepository.GetAsync(CollectionId, 1, 10, filter);
            Console.WriteLine($"Output:\nTotal: {result.Item2}\n{JToken.Parse(JsonConvert.SerializeObject(result.Item1)).ToString(Formatting.Indented)}");
        }

        [TestMethod]
        public async Task Insert()
        {
            MasterSettings.SetProperties(new Dictionary<string, object>() { { "CreatedBy", Guid.Parse("FC09E7EE-5E78-E811-80C7-000C29DADC00") } }, MasterSettings.Triggers.BeforeInsert);
            //MasterSettings.SetProperties(new Dictionary<string, object>() { { "CreatedBy", Guid.Parse("6B9F4B43-5F78-E811-80C7-000C29DADC00") } }, MasterSettings.Triggers.AfterInsert);
            Message message = new Message()
            {
                Text = $"Test message # {DateTime.UtcNow.ToShortTimeString()}",
                Client = 3,
                Caterer = 4
            };

            Message result = await MessageRepository.InsertAsync(message);
            Console.WriteLine($"Inserted:\n{JToken.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Message));
        }

        [TestMethod]
        public async Task Update()
        {
            Message message = new Message()
            {
                Id = new ObjectId("5ebc525898d2c15c8839b4f5"),
                Text = "Changed"
            };

            bool result = await MessageRepository.UpdateAsync(CollectionId, message);
            Console.WriteLine($"Updated: {result}");
        }

        [TestMethod]
        public async Task Delete()
        {
            bool result = await MessageRepository.DeleteAsync(CollectionId, new ObjectId("5ebc525898d2c15c8839b4f5"));
            Console.WriteLine($"Deleted: {result}");
        }

        [TestMethod]
        public async Task Count()
        {
            long result = await MessageRepository.CountAsync(CollectionId);
            Console.WriteLine($"Count: {result}");
        }

        [TestMethod]
        public async Task Exists()
        {
            bool result = await MessageRepository.ExistsAsync(CollectionId, x => x.Text == "abc");
            Console.WriteLine($"Exists: {result}");
        }
    }
}
