# RuleEngine-AzureFunction-POC

#### request example:

`curl -d '{ "rules": [ { "scope": "Servers.Disks", "color": "red", "recommendation_string": "Cap upgrades", "operator": "and", "criteria": null, "metric": "UsedCap", "comparator": "gte", "value": "90" }, { "scope": "Servers.Memory", "color": "blue", "recommendation_string": "Memory Upgrades", "operator": "and", "criteria": [ { "metric": "AverageMemoryUsage", "comparator": "gte", "value": "90" }, { "operator": "or", "criteria": [ { "metric": "OS", "comparator": "eql", "value": "Windows" }, { "metric": "OS", "comparator": "eql", "value": "Linux" } ] }, { "metric": "IsHypervisor", "comparator": "eql", "value": "0" }, { "metric": "Id", "comparator": "uneql", "value": "123" } ], "metric": null, "comparator": null, "value": null } ], "data": { "ProjectId": 5421, "ScanType": 1, "Servers": { "ServerId": 1234, "ServerName": "Foo", "Disks": { "DiskId": 123, "DiskName": "foo", "TotalCap": 1024, "UsedCap": 95 }, "Memory": { "Id": 450, "OS": "Windows", "AverageMemoryUsage": "98", "IOPSAverage": "28", "IsHypervisor": 0 } } } }' -H "Content-Type: application/json" -X POST http://localhost:7071/api/RuleEngine`

its also deployed on Azure under `https://ruleenginepoc.azurewebsites.net/api/RuleEngine?code=MyKey` let me know if you need access
