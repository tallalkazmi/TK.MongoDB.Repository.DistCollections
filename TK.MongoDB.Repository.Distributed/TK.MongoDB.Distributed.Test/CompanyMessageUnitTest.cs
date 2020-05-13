using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TK.MongoDB.Distributed.Data;
using TK.MongoDB.Distributed.Test.Models;

namespace TK.MongoDB.Distributed.Test
{
    [TestClass]
    public class CompanyMessageUnitTest
    {
        readonly Repository<CompanyMessage> CompanyMessageRepository;

        public CompanyMessageUnitTest()
        {
            Settings.Configure("MongoDocConnection");
            CompanyMessageRepository = new Repository<CompanyMessage>();
        }

        [TestMethod]
        public async Task Insert()
        {
            CompanyMessage message = new CompanyMessage()
            {
                Text = "xyz",
                Company = 1
            };

            CompanyMessage result = await CompanyMessageRepository.InsertAsync(message);
            Console.WriteLine($"Inserted:\n{JToken.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
        }
    }
}
