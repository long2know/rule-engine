using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RuleEngine.Data;
using RuleEngine.Models;

namespace RuleEngine.Data.EntityMappings
{
    internal class BusinessRuleConfiguration : DbEntityConfiguration<BusinessRule>
    {
        public override void Configure(EntityTypeBuilder<BusinessRule> entity)
        {
			var schema = string.IsNullOrWhiteSpace(this.DbSchema) ? "dbo" : this.DbSchema;
			entity.ToTable("BusinessRule", schema);

            entity.HasKey(x => x.Id);
            entity.Property(p => p.Id).HasColumnName("RuleId").ValueGeneratedOnAdd();
        }
    }
}
