using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuleEngine.Data;
using RuleEngine.Models;
using RuleEngine.Interfaces;

namespace RuleEngine
{
	public class Application
	{
		private IParser _parser;
		private ILogger _log;
		private DbContext _context;

		public Application(IParser parser, ILogger<Application> log, BusinessRuleContext context)
		{
			_parser = parser;
			_log = log;
			_context = context;

			// Seed our data - this shows that the rule can evaluate a numeric or truthy result
			context.BusinessRules.Add(new BusinessRule() { Expression = "4/3 * Math.Pi * Math.Pow(25,3)", ReturnType = "double", IsActive = true, Version = "v1" });
			context.BusinessRules.Add(new BusinessRule() { Expression = "(Request.User.IsInOrg && Request.Location.Name == \"Test\") && Request.Location.Name != \"Test2\"", InputType = "Request", ReturnType = "bool", IsActive = true, Version = "v1" });
			context.BusinessRules.Add(new BusinessRule() { Expression = "(Request.User.IsInOrg && Request.Location.Name == \"Test2\") && Request.Location.Name != \"Test\"", InputType = "Request", ReturnType = "bool", IsActive = true, Version = "v1" });
			context.BusinessRules.Add(new BusinessRule() { Expression = "(Request.User.IsInOrg && Request.Location.Name == \"Test2\") && Request.Location.Name != \"Test\"", InputType = "Request", ReturnType = "bool", IsActive = true, Version = "v1" });
			context.BusinessRules.Add(new BusinessRule() { Expression = "(Request.User.IsInOrg && Request.Location.Name == \"Test2\") || Request.Location.Name == \"Test\"", InputType = "Request", ReturnType = "bool", IsActive = true, Version = "v1" });
			context.BusinessRules.Add(new BusinessRule() { Expression = "!Request.User.IsInOrg && Request.Location.Name == \"Test2\"", InputType = "Request", ReturnType = "bool", IsActive = true, Version = "v1" });
			context.SaveChanges();
		}

		public void Run(string[] args)
		{
			var rules = _context.Set<BusinessRule>().ToListAsync().Result;
			var request = new Request
			{
				User = new User() { FullName = "Test User", IsInOrg = true },
				Location = new Location() { Name = "Test" }
			};

			double doubleResult = 0;
			bool boolResult = false;

			foreach (var rule in rules)
			{
				if (string.IsNullOrWhiteSpace(rule.InputType))
				{
					doubleResult = _parser.Parse<double>(rule.Expression);
					_log.LogInformation($"Result of expression {rule.Expression} is {doubleResult}");
				}
				else
				{
					boolResult = _parser.Parse<Request, bool>(rule.Expression, request);
					_log.LogInformation($"Result of expression {rule.Expression} is {boolResult}");
				}
			}

			Console.ReadKey();
		}

		public static bool IsDebug
		{
			get
			{
				bool isDebug = false;
#if DEBUG
				isDebug = true;
#endif
				return isDebug;
			}
		}
	}
}

