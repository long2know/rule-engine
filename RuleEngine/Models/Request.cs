using System;
using System.Collections.Generic;
using System.Text;

namespace RuleEngine.Models
{
	public class Request
	{
		int Id { get; set; }
		public User User { get; set; }
		public Location Location { get; set; }
	}
}
