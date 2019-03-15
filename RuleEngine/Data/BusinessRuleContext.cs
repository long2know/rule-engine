using Microsoft.EntityFrameworkCore;
using RuleEngine.Data.EntityMappings;
using RuleEngine.Models;
using System.Reflection;

namespace RuleEngine.Data
{
	public class BusinessRuleContext : DbContext
	{
		public DbSet<BusinessRule> BusinessRules { get; set; }

		public BusinessRuleContext(DbContextOptions<BusinessRuleContext> options) :
			base(options)
		{ }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory;Trusted_Connection=True;ConnectRetryCount=0");
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			var assembly = typeof(BusinessRuleContext).GetTypeInfo().Assembly;
			modelBuilder.AddAssemblyConfiguration<BusinessRuleContext>(typeof(BusinessRuleConfiguration).Namespace, "dbo");
			modelBuilder.HasDefaultSchema("dbo");
		}
	}
}
