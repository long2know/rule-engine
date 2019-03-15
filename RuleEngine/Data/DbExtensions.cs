using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RuleEngine.Data
{
    public static class ModelBuilderExtensions
    {
        private static List<Type> GetEntityConfigurations(this Assembly assembly, string filterNamespace = "")
        {
            var types = assembly
                .GetTypes()
                .Where(x =>
                {
                    var ti = x.GetTypeInfo();
                    return !ti.IsAbstract && ti.GetInterfaces().Any(y => y.GetTypeInfo().IsGenericType && y.GetGenericTypeDefinition() == typeof(IDbEntityConfiguration<>))
                    && (string.IsNullOrWhiteSpace(filterNamespace) || x.Namespace.ToLower().Trim() == filterNamespace.ToLower().Trim());
                })
                .ToList();
            return types;
        }

        public static void AddConfiguration<TEntity>(
          this ModelBuilder modelBuilder,
          DbEntityConfiguration<TEntity> dbEntityConfiguration) where TEntity : class
        {
            modelBuilder.Entity<TEntity>(dbEntityConfiguration.Configure);
        }

        /// <summary>
        /// Specify a type in an assembly that has DbConfigurations you want to add
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="modelBuilder"></param>
        public static void AddAssemblyConfiguration<TType>(this ModelBuilder modelBuilder, string filterNamespace = "", string schema = "") where TType : class
        {
            var assembly = typeof(TType).GetTypeInfo().Assembly;
            var entityConfigs = assembly.GetEntityConfigurations(filterNamespace);
            foreach (var entityConfig in entityConfigs.Select(Activator.CreateInstance).Cast<IDbEntityConfiguration>())
            {
                entityConfig.Configure(modelBuilder, schema);
            }
        }

        /// <summary>
        /// Specify a type in an assembly that has DbConfigurations you want to add
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="modelBuilder"></param>
        public static void AddAssemblyConfiguration(this ModelBuilder modelBuilder, Assembly assembly, string filterNamespace = "")
        {
            var entityConfigs = assembly.GetEntityConfigurations(filterNamespace);
            foreach (var entityConfig in entityConfigs.Select(Activator.CreateInstance).Cast<IDbEntityConfiguration>())
            {
                entityConfig.Configure(modelBuilder);
            }
        }
    }

    public interface IDbEntityConfiguration
    {
        void Configure(ModelBuilder modelBuilder, string schema = "");
    }

    public interface IDbEntityConfiguration<TEntity> : IDbEntityConfiguration where TEntity : class
    {
        void Configure(EntityTypeBuilder<TEntity> modelBuilder);
    }

    public abstract class DbEntityConfiguration<TEntity> : IDbEntityConfiguration<TEntity> where TEntity : class
    {
        private string _dbSchema = string.Empty;
        public string DbSchema { get => _dbSchema; set => _dbSchema = value; }

        public abstract void Configure(EntityTypeBuilder<TEntity> entity);
        public void Configure(ModelBuilder modelBuilder, string schema = "")
        {
            _dbSchema = schema;
            Configure(modelBuilder.Entity<TEntity>());
        }
    }
}