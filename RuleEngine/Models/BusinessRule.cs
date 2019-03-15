using System;
using System.Collections.Generic;
using System.Text;

namespace RuleEngine.Models
{
	public class BusinessRule
	{
		public int Id { get; set; }
		public string Expression { get; set; }
		public string InputType { get; set; }
		public string ReturnType { get; set; }
		public string Version { get; set; }
		public bool IsActive { get; set; } = true;
		public bool ApplyGlobally { get; set; } = true;
	}
}
