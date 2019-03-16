using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RuleEngine.Data;
using RuleEngine.Models;
using RuleEngine.Interfaces;

namespace RuleEngine
{
    using System.Collections.Generic;
    using System.Linq;

    public class Application
	{
        private static readonly Dictionary<string, Type> _knownTypes =
            new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        private IParser _parser;
		private ILogger _log;
		private DbContext _context;

        static Application()
        {
            _knownTypes["int"] = typeof(int);
            _knownTypes["double"] = typeof(double);
            _knownTypes["long"] = typeof(long);
            _knownTypes["float"] = typeof(float);
            _knownTypes["bool"] = typeof(bool);
        }

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

			//double doubleResult = 0;
			//bool boolResult = false;

			foreach (var rule in rules)
			{
                var returnType = GetShortcutType(rule.ReturnType) ?? Type.GetType(rule.ReturnType, false, true);
                var methods = typeof(IParser).GetMethods().Where(m => m.ContainsGenericParameters).ToList();

                if (string.IsNullOrWhiteSpace(rule.InputType))
                {
                    var method = methods.First(
                        m => m.ReturnType.IsGenericParameter && m.GetGenericArguments().Length == 1 &&
                             m.GetGenericArguments()[0] == m.ReturnType);
                    var generic = method.MakeGenericMethod(returnType);
                    var result = generic.Invoke(_parser, new object[] { rule.Expression });
                    var finalResult = Convert.ChangeType(result, returnType);

                    _log.LogInformation($"Result of expression {rule.Expression} is {finalResult}");
				}
				else
                {
                    var inputType = AppDomain.CurrentDomain.GetAssemblies().
                        SelectMany(t => t.GetTypes()).First(t => string.Equals(t.Name, rule.InputType, StringComparison.Ordinal));
                    var method = methods.First(m => m.ReturnType.IsGenericParameter && m.GetGenericArguments().Length == 2);
                    var generic = method.MakeGenericMethod(new Type[] {inputType, returnType});
                    var result = generic.Invoke(_parser, new object[] { rule.Expression, request });
                    var finalResult = Convert.ChangeType(result, returnType);

					_log.LogInformation($"Result of expression {rule.Expression} is {finalResult}");
				}
			}

			Console.ReadKey();
		}

        private static Type GetShortcutType(string name)
        {
            Type toReturn = null;
            _knownTypes.TryGetValue(name, out toReturn);
            return toReturn; //returns null if not found
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

