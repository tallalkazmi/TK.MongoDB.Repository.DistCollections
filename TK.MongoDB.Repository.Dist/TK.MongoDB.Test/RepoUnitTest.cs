using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TK.MongoDB.Test.Models;

namespace TK.MongoDB.Test
{
    [TestClass]
    public class RepoUnitTest
    {
        Repository<Message> MessageRepository;
        public RepoUnitTest()
        {
            Settings.Configure("MongoDocConnection");
            //Settings.Configure<Activity>(2592000);
            MessageRepository = new Repository<Message>();
        }

        [TestMethod]
        public void MasterFind()
        {
            var result = MessageRepository.MasterGet();
            Console.WriteLine($"Output:\n{JToken.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
        }

        //[TestMethod]
        //public void Find()
        //{
        //    Message result = MessageRepository.FindAsync(x => x.Text == "abc" && x.Client == 1).Result;
        //    Console.WriteLine($"Output:\n{JToken.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
        //}

        //[TestMethod]
        //public void GetById()
        //{
        //    Message result = MessageRepository.GetAsync(new ObjectId("5e36997898d2c15a400f8968")).Result;
        //    Console.WriteLine($"Output:\n{JToken.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
        //}

        //[TestMethod]
        //public void Get()
        //{
        //    var result = MessageRepository.GetAsync(1, 10, x => x.Name.Contains("abc") && x.Deleted == false).Result;
        //    Console.WriteLine($"Output:\nTotal: {result.Item2}\n{JToken.Parse(JsonConvert.SerializeObject(result.Item1)).ToString(Formatting.Indented)}");
        //}

        //[TestMethod]
        //public void GetNonPaged()
        //{
        //    var result = MessageRepository.Get(x => x.Name.Contains("abc") && x.Deleted == false);
        //    Console.WriteLine($"Output:\n{JToken.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
        //}

        //[TestMethod]
        //public void In()
        //{
        //    List<string> names = new List<string> { "abc", "def", "ghi" };
        //    var result = MessageRepository.In(x => x.Name, names);
        //    Console.WriteLine($"Output:\n{JToken.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
        //}

        [TestMethod]
        public void Insert()
        {
            Message message = new Message()
            {
                Text = "xyz",
                Client = 1,
                Caterer = 2,
                Order = 4
            };

            Message result = MessageRepository.InsertAsync(message).Result;
            Console.WriteLine($"Inserted:\n{JToken.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
        }

        //[TestMethod]
        //public void Update()
        //{
        //    Activity activity = new Activity()
        //    {
        //        Id = new ObjectId("5e36998998d2c1540ca23894"),
        //        Name = "abc3"
        //    };

        //    bool result = MessageRepository.UpdateAsync(activity).Result;
        //    Console.WriteLine($"Updated: {result}");
        //}

        //[TestMethod]
        //public void Delete()
        //{
        //    bool result = MessageRepository.DeleteAsync(new ObjectId("5e36998998d2c1540ca23894")).Result;
        //    Console.WriteLine($"Deleted: {result}");
        //}

        //[TestMethod]
        //public void Count()
        //{
        //    long result = MessageRepository.CountAsync().Result;
        //    Console.WriteLine($"Count: {result}");
        //}

        //[TestMethod]
        //public void Exists()
        //{
        //    bool result = MessageRepository.ExistsAsync(x => x.Name == "abc").Result;
        //    Console.WriteLine($"Exists: {result}");
        //}
    }
}
