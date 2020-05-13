using MongoDB.Bson;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace TK.MongoDB.Distributed.Classes
{
    public static class Utility
    {
        public static BsonDocument CreateSearchBsonDocument(IDictionary<string, object> keyValues, bool disregardNullValues = false)
        {
            var searchCriteria = new BsonArray();

            foreach (var item in keyValues)
            {
                if (disregardNullValues == true && item.Value == null) continue;
                searchCriteria.Add(new BsonDocument(item.Key, BsonValue.Create(item.Value)));
            }

            var filterDocument = new BsonDocument { { "$and", searchCriteria } };
            return filterDocument;
        }

        public static IEnumerable<T> Convert<T>(IEnumerable<BsonDocument> bson) where T : class
        {
            var writterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Shell };
            var temp = bson.ToJson(writterSettings);

            //Replace using MatchEvaluator:
            var temp_EvalReplaced = Regex.Replace(temp, @"ISODate\((?<dt>.*?)\)", m => m.Groups["dt"].Value);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<T>>(temp_EvalReplaced);
        }

        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            //Need to detect whether they use the same parameter instance; if not, they need fixing
            ParameterExpression param = expr1.Parameters[0];
            if (ReferenceEquals(param, expr2.Parameters[0]))
            {
                // simple version
                return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, expr2.Body), param);
            }

            //Otherwise, keep expr1 "as is" and invoke expr2
            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(expr1.Body, Expression.Invoke(expr2, param)), param);
        }
    }
}
