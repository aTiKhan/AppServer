﻿using ASC.Common;
using ASC.Core.Common.EF.Model.Resource;

using Microsoft.EntityFrameworkCore;

namespace ASC.Core.Common.EF.Context
{
    public class ResourceDbContext : BaseDbContext
    {
        public DbSet<ResAuthors> Authors { get; set; }
        public DbSet<ResAuthorsFile> ResAuthorsFiles { get; set; }
        public DbSet<ResAuthorsLang> ResAuthorsLang { get; set; }
        public DbSet<ResCultures> ResCultures { get; set; }
        public DbSet<ResData> ResData { get; set; }
        public DbSet<ResFiles> ResFiles { get; set; }
        public DbSet<ResReserve> ResReserve { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .AddResAuthorsLang()
                .AddResAuthorsFile();
        }
    }

    public static class ResourceDbExtension
    {
        public static DIHelper AddResourceDbService(this DIHelper services)
        {
            return services.AddDbContextManagerService<ResourceDbContext>();
        }
    }
}
