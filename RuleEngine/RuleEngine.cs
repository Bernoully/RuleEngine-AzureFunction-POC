using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace RuleEngine
{
    public static class RuleEngine
    {
        public static Dictionary<string, Func<dynamic, dynamic, bool>> actions = new Dictionary<string, Func<dynamic, dynamic, bool>>()
        {
            { "and", (a, b) => { return a & b; } },
            { "or", (a, b) => { return a | b; } }
        };

        [FunctionName("RuleEngine")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            dynamic ProjectData = data?.data;
            dynamic rules = data?.rules;

            return data != null
                ? (ActionResult)new OkObjectResult($"{JsonConvert.SerializeObject(RuleEngine.RuleEval(ProjectData, rules, log))}")
                : new BadRequestObjectResult("Please pass a data on the query string or in the request body");
        }
        public static string GetEnvironmentVariable(string name)
        {
            return name + ": " +
                System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        public static dynamic GetValue(dynamic data, string path)
        {
            String[] spearator = { "." };
            Int32 count = 2;
            String[] pathlist = path.Split(spearator, count,
                           StringSplitOptions.RemoveEmptyEntries);

            if (!data.ContainsKey(pathlist[0]))
                return null;

            return pathlist.Length == 1 ? data[pathlist[0]] : RuleEngine.GetValue(data[pathlist[0]], pathlist[1]);
        }

        public static bool Compare(dynamic value, string compare, dynamic targetValue)
        {
            if (value is null)
                return false;

            if (compare == "gte")
                return value >= targetValue;

            if (compare == "range")
                return value >= targetValue[0] && value <= targetValue[1];

            if (compare == "eql")
                return value == targetValue;

            if (compare == "uneql")
                return value != targetValue;

            return false;
        }

        public static bool RuleMatch(dynamic data, dynamic rule)
        {
            if (rule.ContainsKey("criteria") && rule["criteria"] != null)
            {
                bool match = true;
                foreach (var criteria in rule["criteria"])
                    match = RuleEngine.actions[(string)rule["operator"]](match, RuleEngine.RuleMatch(data, criteria));

                return match;
            }

            var value = RuleEngine.GetValue(data, (string)rule["metric"]);

            return RuleEngine.Compare(value, (string)rule["comparator"], rule["value"]);
        }
        public static List<string> RuleEval(dynamic data, dynamic rules, ILogger log)
        {
            List<string> recommendations = new List<string>();
            foreach (var rule in rules)
            {
                if (RuleEngine.RuleMatch(RuleEngine.GetValue(data, (string)rule["scope"]), rule))
                {
                    Console.WriteLine($"========= {rule["recommendation_string"]}");

                    recommendations.Add((string)rule["recommendation_string"]);
                }
            }

            return recommendations;
        }

    }
}
