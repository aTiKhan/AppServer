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


using ASC.Common;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.ElasticSearch;
using System;
using System.Collections.Generic;
using System.Linq;


namespace ASC.Web.CRM.Core.Search
{
    public class EmailWrapper : ContactsWrapper
    {
        [Join(JoinTypeEnum.Sub, "id:contact_id", "tenant_id:tenant_id")]
        public List<EmailInfoWrapper> EmailInfoWrapper { get; set; }

        protected override string IndexName
        {
            get
            {
                return "crm_contact_email";
            }
        }

        public static EmailWrapper GetEmailWrapper(int tenantId, 
                                                    Contact contact, 
                                                    List<ContactInfo> contactInfo)
        {
            var result = new EmailWrapper();

            var person = contact as Person;
            if (person != null)
            {
                result = new EmailWrapper
                {
                    Id = person.ID,
                    Title = person.JobTitle,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Industry = person.Industry,
                    Notes = person.About,
                    TenantId = tenantId
                };
            }

            var company = contact as Company;
            
            if (company != null)
            {
                result = new EmailWrapper
                {
                    Id = company.ID,
                    CompanyName = company.CompanyName,
                    Industry = company.Industry,
                    Notes = company.About,
                    TenantId = tenantId
                };
            }

            result.EmailInfoWrapper = contactInfo.Select(r => ASC.Web.CRM.Core.Search.EmailInfoWrapper.FromContactInfo(tenantId, r)).ToList();

            return result;
        }
    }

    public class EmailInfoWrapper : Wrapper
    {
        [ColumnLastModified("last_modifed_on")]
        public override DateTime LastModifiedOn { get; set; }

        [ColumnCondition("contact_id", 1)]
        public int ContactId { get; set; }

        [Column("data", 2, Analyzer = Analyzer.uax_url_email)]
        public string Data { get; set; }

        [ColumnCondition("type", 3, 1)]
        public int Type { get; set; }

        protected override string Table { get { return "crm_contact_info"; } }

        public static EmailInfoWrapper FromContactInfo(int tenantId, ContactInfo cf)
        {
            return new EmailInfoWrapper
            {
                Id = cf.ID,
                ContactId = cf.ContactID,
                Data = cf.Data,
                Type = (int)cf.InfoType,
                TenantId = tenantId
            };
        }
    }

    public static class EmailWrapperExtention
    {
        public static DIHelper AddEmailWrapperService(this DIHelper services)
        {
            services.TryAddTransient<EmailWrapper>();

            return services
                .AddFactoryIndexerService<EmailWrapper>();
        }
    }

    public static class EmailInfoWrapperExtention
    {
        public static DIHelper AddEmailInfoWrapperService(this DIHelper services)
        {
            services.TryAddTransient<EmailInfoWrapper>();

            return services
                .AddFactoryIndexerService<EmailInfoWrapper>();
        }
    }
}