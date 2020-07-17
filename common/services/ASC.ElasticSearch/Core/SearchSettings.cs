/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;

using ASC.Common;
using ASC.Common.Caching;
using ASC.Core;
using ASC.Core.Common.Settings;

using Autofac;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

namespace ASC.ElasticSearch.Core
{
    [Serializable]
    public class SearchSettings : ISettings
    {
        public string Data { get; set; }

        public Guid ID
        {
            get { return new Guid("{93784AB2-10B5-4C2F-9B36-F2662CCCF316}"); }
        }

        public ISettings GetDefault(IServiceProvider serviceProvider)
        {
            return new SearchSettings();
        }

        private List<SearchSettingsItem> items;
        internal List<SearchSettingsItem> Items
        {
            get
            {
                if (items != null) return items;
                var parsed = JsonConvert.DeserializeObject<List<SearchSettingsItem>>(Data ?? "");
                return items = parsed ?? new List<SearchSettingsItem>();
            }
            set
            {
                items = value;
            }
        }

        internal bool IsEnabled(string name)
        {
            var wrapper = Items.FirstOrDefault(r => r.ID == name);

            return wrapper != null && wrapper.Enabled;
        }
    }

    public class SearchSettingsHelper
    {
        public TenantManager TenantManager { get; }
        public SettingsManager SettingsManager { get; }
        public CoreBaseSettings CoreBaseSettings { get; }
        public FactoryIndexer FactoryIndexer { get; }
        public ICacheNotify<ReIndexAction> CacheNotify { get; }
        public IServiceProvider ServiceProvider { get; }

        public SearchSettingsHelper(
            TenantManager tenantManager,
            SettingsManager settingsManager,
            CoreBaseSettings coreBaseSettings,
            FactoryIndexer factoryIndexer,
            ICacheNotify<ReIndexAction> cacheNotify,
            IServiceProvider serviceProvider)
        {
            TenantManager = tenantManager;
            SettingsManager = settingsManager;
            CoreBaseSettings = coreBaseSettings;
            FactoryIndexer = factoryIndexer;
            CacheNotify = cacheNotify;
            ServiceProvider = serviceProvider;
        }

        public List<SearchSettingsItem> GetAllItems()
        {
            if (!SearchByContentEnabled) return new List<SearchSettingsItem>();

            var settings = SettingsManager.Load<SearchSettings>();

            return AllItems.Select(r => new SearchSettingsItem
            {
                ID = r.IndexName,
                Enabled = settings.IsEnabled(r.IndexName),
                Title = r.SettingsTitle
            }).ToList();
        }

        private List<IFactoryIndexer> allItems;
        internal List<IFactoryIndexer> AllItems
        {
            get
            {
                return allItems ?? (allItems = FactoryIndexer.Builder.Resolve<IEnumerable<IFactoryIndexer>>().ToList());
            }
        }

        public void Set(List<SearchSettingsItem> items)
        {
            if (!SearchByContentEnabled) return;

            var settings = SettingsManager.Load<SearchSettings>();

            var settingsItems = settings.Items;
            var toReIndex = !settingsItems.Any() ? items.Where(r => r.Enabled).ToList() : items.Where(item => settingsItems.Any(r => r.ID == item.ID && r.Enabled != item.Enabled)).ToList();

            settings.Items = items;
            settings.Data = JsonConvert.SerializeObject(items);
            SettingsManager.Save(settings);

            var action = new ReIndexAction() { Tenant = TenantManager.GetCurrentTenant().TenantId };
            action.Names.AddRange(toReIndex.Select(r => r.ID).ToList());

            CacheNotify.Publish(action, CacheNotifyAction.Any);
        }

        public bool CanSearchByContent<T>(int tenantId) where T : class, ISearchItem
        {
            if (!SearchByContentEnabled) return false;

            if (typeof(ISearchItemDocument).IsAssignableFrom(typeof(T)))
            {
                return false;
            }

            var settings = SettingsManager.LoadForTenant<SearchSettings>(tenantId);

            return settings.IsEnabled(ServiceProvider.GetService<T>().IndexName);
        }

        private bool SearchByContentEnabled
        {
            get
            {
                return CoreBaseSettings.Standalone;
            }
        }
    }

    [Serializable]
    public class SearchSettingsItem
    {
        public string ID { get; set; }

        public bool Enabled { get; set; }

        public string Title { get; set; }
    }

    public static class SearchSettingsHelperExtention
    {
        public static DIHelper AddSearchSettingsHelperService(this DIHelper services)
        {
            if (services.TryAddScoped<SearchSettingsHelper>())
            {
                return services
                    .AddSettingsManagerService()
                    .AddCoreBaseSettingsService()
                    .AddFactoryIndexerService()
                    .AddTenantManagerService();
            }

            return services;
        }
    }
}
