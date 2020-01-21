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
        [FunctionName("RuleEngine")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            dynamic ProjectData = data?.data;
            dynamic rules = data?.rules;
            log.LogInformation($"===================> {RuleEngine.RuleEval(ProjectData, rules, log)}");

            return data != null
                ? (ActionResult)new OkObjectResult($"Hello, {RuleEngine.RuleEval(ProjectData, rules, log)}")
                : new BadRequestObjectResult("Please pass a data on the query string or in the request body");
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

            Console.WriteLine($"========= {pathlist[0]}");
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

            return false;
        }

        public static bool RuleMatch(dynamic data, dynamic rule)
        {
            if (rule.ContainsKey("criteria"))
            {
                bool match = true;
                foreach (var criteria in rule["criteria"])
                    match &= RuleEngine.RuleMatch(data, criteria);

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
                log.LogInformation($"{rule}");
                if (RuleEngine.RuleMatch(data, rule))
                {
                    recommendations.Add((string)rule["recommendation_string"]);
                }
            }

            return recommendations;
        }

    }
}
