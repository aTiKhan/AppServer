﻿using ASC.Core.Common.EF.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace ASC.Core.Common.EF.Context
{
    public class NotifyDbContext : BaseDbContext
    {
        public DbSet<NotifyInfo> NotifyInfo { get; set; }
        public DbSet<NotifyQueue> NotifyQueue { get; set; }
    }

    public static class NotifyDbExtension
    {
        public static IServiceCollection AddNotifyDbContext(this IServiceCollection services)
        {
            services.TryAddScoped<DbContextManager<NotifyDbContext>>();
            services.TryAddScoped<IConfigureOptions<NotifyDbContext>, ConfigureDbContext>();
            services.TryAddScoped<NotifyDbContext>();

            return services;
        }
    }
}
