﻿
using System;

using ASC.Api.Core;
using ASC.Api.Core.Auth;
using ASC.Api.Core.Core;
using ASC.Api.Core.Middleware;
using ASC.Common;
using ASC.Common.DependencyInjection;
using ASC.Common.Logging;
using ASC.Common.Threading.Progress;
using ASC.Common.Threading.Workers;
using ASC.Data.Reassigns;
using ASC.Employee.Core.Controllers;
using ASC.Web.Core.Users;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ASC.People
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostEnvironment HostEnvironment { get; }

        public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            HostEnvironment = hostEnvironment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            
            var diHelper = new DIHelper(services);

            diHelper
                .AddConfirmAuthHandler()
                .AddCookieAuthHandler()
                .AddCultureMiddleware()
                .AddIpSecurityFilter()
                .AddPaymentFilter()
                .AddProductSecurityFilter()
                .AddTenantStatusFilter();

            diHelper.Configure<WorkerQueue<ResizeWorkerItem>>(r =>
            {
                r.workerCount = 2;
                r.waitInterval = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
                r.errorCount = 1;
                r.stopAfterFinsih = true;
            });

            diHelper.Configure<ProgressQueue<ReassignProgressItem>>(r =>
            {
                r.workerCount = 1;
                r.waitInterval = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;
                r.removeAfterCompleted = true;
                r.stopAfterFinsih = false;
                r.errorCount = 0;
            });

            diHelper.Configure<ProgressQueue<RemoveProgressItem>>(r =>
            {
                r.workerCount = 1;
                r.waitInterval = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;
                r.removeAfterCompleted = true;
                r.stopAfterFinsih = false;
                r.errorCount = 0;
            });

            diHelper.AddNLogManager("ASC.Api", "ASC.Web");

            diHelper
                .AddPeopleController()
                .AddGroupController();
            GeneralStartup.ConfigureServices(services, true);
            services.AddAutofac(Configuration, HostEnvironment.ContentRootPath);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseCors(builder =>
                builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseCultureMiddleware();

            app.UseDisposeMiddleware();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapCustom();
            });
        }
    }
}