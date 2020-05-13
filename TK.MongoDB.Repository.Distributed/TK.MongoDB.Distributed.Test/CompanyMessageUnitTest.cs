using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TK.MongoDB.Distributed.Test.Models;

namespace TK.MongoDB.Distributed.Test
{
    [TestClass]
    public class CompanyMessageUnitTest : BaseTest
    {
        public CompanyMessageUnitTest()
        {
            Settings.ConnectionStringSettingName = "MongoDocConnection";
        }

        [TestMethod]
        public async Task Insert()
        {
            CompanyMessage message = new CompanyMessage()
            {
                Text = $"Test message # {DateTime.UtcNow.ToShortTimeString()}",
                Company = 1
            };

            var result = await CompanyMessageRepository.InsertAsync(message);
            Console.WriteLine($"Inserted:\n{JToken.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
            
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(CompanyMessage));
        }
    }
}
