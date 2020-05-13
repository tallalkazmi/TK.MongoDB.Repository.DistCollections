using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TK.MongoDB.Data;

namespace TK.MongoDB.Test
{
    [TestClass]
    public class MasterUnitTest
    {
        readonly MasterRepository MasterRepository;

        public MasterUnitTest()
        {
            Settings.Configure("MongoDocConnection");
            MasterRepository = new MasterRepository();
        }

        [TestMethod]
        public async Task Get()
        {
            var result = await MasterRepository.GetAsync();
            Console.WriteLine($"Output:\n{JToken.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
        }

        [TestMethod]
        public async Task GetByKeys()
        {
            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
                { "Client", 1 }
            };

            var result = await MasterRepository.GetAsync(keyValuePairs);
            Console.WriteLine($"Output:\n{JToken.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
        }

        //[TestMethod]
        //public async Task GetByDef()
        //{
        //    var result = await MasterRepository.Get();
        //    Console.WriteLine($"Output:\n{JToken.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
        //}

        [TestMethod]
        public async Task Update()
        {
            bool result = await MasterRepository.UpdateAsync("53df73c45d7e493b86746066a693534c", "Untitled-3");
            Console.WriteLine($"Updated: {result}");
        }
    }
}
