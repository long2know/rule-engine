using System;
using System.Collections.Generic;
using System.Text;

namespace RuleEngine.Models
{
	public class User
	{
		public int Id { get; set; }
		public string FullName { get; set; }
		public bool IsInOrg { get; set; }
	}
}
