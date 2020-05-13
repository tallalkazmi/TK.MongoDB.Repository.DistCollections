using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TK.MongoDB.Distributed.Test.Models;

namespace TK.MongoDB.Distributed.Test
{
    [TestClass]
    public class MessageUnitTest:BaseTest
    {
        readonly string CollectionId;

        public MessageUnitTest()
        {
            Settings.ConnectionStringSettingName = "MongoDocConnection";
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
        public async Task Insert()
        {
            Message message = new Message()
            {
                Text = $"Test message # {DateTime.UtcNow.ToShortTimeString()}",
                Client = 2,
                Caterer = 2
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
