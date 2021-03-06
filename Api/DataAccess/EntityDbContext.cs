﻿namespace Api.DataAccess
{
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using Extensions.Pack;
    using Microsoft.EntityFrameworkCore;

    public class EntityDbContext : DbContext
    {
        private readonly IEntityProvider _entityProvider;

        public EntityDbContext(IEntityProvider entityProvider)
        {
            _entityProvider = entityProvider;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (optionsBuilder.IsConfigured.IsFalse())
            {
                optionsBuilder.UseInMemoryDatabase("GenericDatabase");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var types = _entityProvider.GetAll();

            foreach (var type in types)
            {
                var propertyInfos = type.GetProperties();
                var propertyNameOfKey = propertyInfos.FirstOrDefault(p => p.HasCustomAttribute<KeyAttribute>());
                if (propertyNameOfKey.IsNull())
                {
                    throw new InvalidDataException($"Type: {type.Name} does not have defined key property");
                }

                modelBuilder.Entity(type, item =>
                {
                    item.HasKey(propertyNameOfKey.Name);
                    foreach (var propertyInfo in propertyInfos)
                    {
                        if (propertyInfo.EqualsTo(propertyNameOfKey))
                        {
                            continue;
                        }

                        item.Property(propertyInfo.PropertyType, propertyInfo.Name);
                    }
                });
            }
        }
    }
}
