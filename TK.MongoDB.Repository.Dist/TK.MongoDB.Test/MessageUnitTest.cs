using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TK.MongoDB.Data;
using TK.MongoDB.Test.Models;

namespace TK.MongoDB.Test
{
    [TestClass]
    public class MessageUnitTest
    {
        readonly Repository<Message> MessageRepository;
        readonly string CollectionId;

        public MessageUnitTest()
        {
            Settings.Configure("MongoDocConnection");
            MessageRepository = new Repository<Message>();
            CollectionId = "5f9a6a926bf844cd8b50a88824333dfd";
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
                Text = "xyz",
                Client = 2,
                Caterer = 2,
                Order = 1
            };

            Message result = await MessageRepository.InsertAsync(message);
            Console.WriteLine($"Inserted:\n{JToken.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
        }

        [TestMethod]
        public async Task Update()
        {
            Message message = new Message()
            {
                Id = new ObjectId("5eb9a6f598d2c102f861708c"),
                Text = "Changed"
            };

            bool result = await MessageRepository.UpdateAsync(CollectionId, message);
            Console.WriteLine($"Updated: {result}");
        }

        [TestMethod]
        public async Task Delete()
        {
            bool result = await MessageRepository.DeleteAsync(CollectionId, new ObjectId("5eb9a6f598d2c102f861708c"));
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
