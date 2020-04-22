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


using ASC.Api.Core;
using ASC.Api.CRM.Wrappers;
using ASC.Api.Documents;
using ASC.Common.Web;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.CRM.Core.Enums;
using ASC.MessagingSystem;
using ASC.Web.Api.Routing;
using ASC.Web.CRM.Services.NotifyService;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using OrderBy = ASC.CRM.Core.Entities.OrderBy;

namespace ASC.Api.CRM
{
    public partial class CRMController
    {
        /// <summary>
        ///   Returns the list of all events matching the parameters specified in the request
        /// </summary>
        /// <short>
        ///   Get event list
        /// </short>
        /// <category>History</category>
        /// <param optional="true" name="entityType" remark="Allowed values: opportunity, contact or case">Related entity type</param>
        /// <param optional="true" name="entityId">Related entity ID</param>
        /// <param optional="true" name="categoryId">Task category ID</param>
        /// <param optional="true" name="createBy">Event author</param>
        /// <param optional="true" name="fromDate">Earliest task due date</param>
        /// <param optional="true" name="toDate">Latest task due date</param>
        /// <returns>
        ///   Event list
        /// </returns>
        [Read(@"history/filter")]
        public IEnumerable<RelationshipEventWrapper> GetHistory(
            string entityType,
            int entityId,
            int categoryId,
            Guid createBy,
            ApiDateTime fromDate,
            ApiDateTime toDate)
        {
            var entityTypeObj = ToEntityType(entityType);

            switch (entityTypeObj) {
                case EntityType.Contact:
                    var contact = DaoFactory.GetContactDao().GetByID(entityId);
                    if (contact == null || !CRMSecurity.CanAccessTo(contact))
                        throw new ItemNotFoundException();
                    break;
                case EntityType.Case:
                    var cases = DaoFactory.GetCasesDao().GetByID(entityId);
                    if (cases == null || !CRMSecurity.CanAccessTo(cases))
                        throw new ItemNotFoundException();
                    break;
                case EntityType.Opportunity:
                    var deal = DaoFactory.GetDealDao().GetByID(entityId);
                    if (deal == null || !CRMSecurity.CanAccessTo(deal))
                        throw new ItemNotFoundException();
                    break;
                default:
                    if (entityId != 0)
                    {
                        throw new ArgumentException();
                    }
                    break;
            }

            RelationshipEventByType eventByType;

            IEnumerable<RelationshipEventWrapper> result;

            OrderBy eventOrderBy;

            if (ASC.CRM.Classes.EnumExtension.TryParse(ApiContext.SortBy, true, out eventByType))
            {
                eventOrderBy = new OrderBy(eventByType, !ApiContext.SortDescending);
            }
            else if (string.IsNullOrEmpty(ApiContext.SortBy))
            {
                eventOrderBy = new OrderBy(RelationshipEventByType.Created, false);
            }
            else
            {
                eventOrderBy = null;
            }

            if (eventOrderBy != null)
            {
                result = ToListRelationshipEventWrapper(DaoFactory.GetRelationshipEventDao().GetItems(
                    ApiContext.FilterValue,
                    entityTypeObj,
                    entityId,
                    createBy,
                    categoryId,
                    fromDate,
                    toDate,
                    (int)ApiContext.StartIndex,
                    (int)ApiContext.Count,
                    eventOrderBy));

                ApiContext.SetDataPaginated();
                ApiContext.SetDataFiltered();
                ApiContext.SetDataSorted();
            }
            else
            {
                result = ToListRelationshipEventWrapper(DaoFactory.GetRelationshipEventDao().GetItems(
                    ApiContext.FilterValue,
                    entityTypeObj,
                    entityId,
                    createBy,
                    categoryId,
                    fromDate,
                    toDate,
                    0,
                    0,
                    null));
            }

            return result;
        }

        /// <summary>
        ///     Deletes the event with the ID specified in the request and all the files associated with this event
        /// </summary>
        /// <short>
        ///     Delete event and related files
        /// </short>
        /// <category>History</category>
        /// <param name="id">Event ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///   Event
        /// </returns>
        [Delete(@"history/{id:int}")]
        public RelationshipEventWrapper DeleteHistory(int id)
        {
            if (id <= 0) throw new ArgumentException();

            var item = DaoFactory.GetRelationshipEventDao().GetByID(id);
            if (item == null) throw new ItemNotFoundException();
            var wrapper = RelationshipEventWrapperHelper.Get(item);

            DaoFactory.GetRelationshipEventDao().DeleteItem(id);

            var messageAction = GetHistoryDeletedAction(item.EntityType, item.ContactID);
            var entityTitle = wrapper.Contact == null ? wrapper.Entity.EntityTitle : wrapper.Contact.DisplayName;
            MessageService.Send( messageAction, MessageTarget.Create(item.ID), entityTitle, wrapper.Category.Title);

            return wrapper;
        }

        /// <summary>
        /// Creates a text (.txt) file in the selected folder with the title and contents sent in the request
        /// </summary>
        /// <short>Create txt</short>
        /// <category>Files</category>
        /// <param name="entityType">Entity type</param>
        /// <param name="entityid">Entity ID</param>
        /// <param name="title">File title</param>
        /// <param name="content">File contents</param>
        /// <returns>
        ///     File info
        /// </returns>
        [Create(@"{entityType:(contact|opportunity|case)}/{entityid:int}/files/text")]
        public FileWrapper<int> CreateTextFile(string entityType, int entityid, string title, string content)
        {
            if (title == null) throw new ArgumentNullException("title");
            if (content == null) throw new ArgumentNullException("content");

            var folderid = GetRootFolderID();

            FileWrapper<int> result;

            var extension = ".txt";
            if (!string.IsNullOrEmpty(content))
            {
                if (Regex.IsMatch(content, @"<([^\s>]*)(\s[^<]*)>"))
                {
                    extension = ".html";
                }
            }

            using (var memStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                title = title.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? title : (title + extension);
                result = SaveFile(folderid, memStream, title);
            }

            AttachFiles(entityType, entityid, new List<int> {(int)result.Id});

            return result;
        }

        /// <summary>
        /// Upload file 
        /// </summary>
        /// <short>Upload file</short>
        /// <category>Files</category>
        /// <remarks>
        /// <![CDATA[
        ///  Upload can be done in 2 different ways:
        ///  <ol>
        /// <li>Single file upload. You should set Content-Type &amp; Content-Disposition header to specify filename and content type, and send file in request body</li>
        /// <li>Using standart multipart/form-data method</li>
        /// </ol>]]>
        /// </remarks>
        /// <param name="entityType">Entity type</param>
        /// <param name="entityid">Entity ID</param>
        /// <param name="file" visible="false">Request Input stream</param>
        /// <param name="contentType" visible="false">Content-Type Header</param>
        /// <param name="contentDisposition" visible="false">Content-Disposition Header</param>
        /// <param name="files" visible="false">List of files when posted as multipart/form-data</param>
        /// <param name="storeOriginalFileFlag" visible="false">If True, upload documents in original formats as well</param>
        /// <returns>
        /// File info
        /// </returns>
        [Create(@"{entityType:(contact|opportunity|case)}/{entityid:int}/files/upload")]
        public FileWrapper<int> UploadFileInCRM(
            string entityType,
            int entityid,
            Stream file,
            ContentType contentType,
            ContentDisposition contentDisposition,
            IEnumerable<IFormFile> files,
            bool storeOriginalFileFlag
            )
        {
            FilesSettings.StoreOriginalFilesSetting = storeOriginalFileFlag;

            var folderid = GetRootFolderID();

            var fileNames = new List<string>();

            FileWrapper<int> uploadedFile = null;
            if (files != null && files.Any())
            {
                //For case with multiple files
                foreach (var postedFile in files)
                {
                    using var fileStream = postedFile.OpenReadStream();
                    uploadedFile = SaveFile(folderid, fileStream, postedFile.FileName);
                    fileNames.Add(uploadedFile.Title);
                }
            }
            else if (file != null)
            {
                uploadedFile = SaveFile(folderid, file, contentDisposition.FileName);
                fileNames.Add(uploadedFile.Title);
            }

            return uploadedFile;
        }

        private FileWrapper<int> SaveFile(int folderid, Stream file, string fileName)
        {
            var resultFile = FileUploader.Exec<int>(folderid, fileName, file.Length, file);

            return FileWrapperHelper.Get<int>(resultFile);
        }

        /// <summary>
        ///   Creates the event with the parameters specified in the request
        /// </summary>
        /// <short>
        ///   Create event
        /// </short>
        /// <category>History</category>
        /// <param optional="true" name="contactId">Contact ID</param>
        /// <param optional="true" name="entityType" remark="Allowed values: opportunity or case">Related entity type</param>
        /// <param optional="true" name="entityId">Related entity ID</param>
        /// <remarks>
        /// <![CDATA[
        ///  You should obligatorily set the value for 'contactId' if 'entityId' is not set or the value for parameters 'entityId' and 'entityType' if 'contactId' is not set.
        /// ]]>
        /// </remarks>
        /// <param optional="false" name="content">Contents</param>
        /// <param optional="false" name="categoryId">Category ID</param>
        /// <param optional="true" name="created">Event creation date</param>
        /// <param optional="true" name="fileId">List of IDs of the files associated with the event</param>
        /// <param optional="true" name="notifyUserList">User field list</param>
        /// <returns>
        ///   Created event
        /// </returns>
        [Create(@"history")]
        public RelationshipEventWrapper AddHistoryTo(
            string entityType,
            int entityId,
            int contactId,
            string content,
            int categoryId,
            ApiDateTime created,
            IEnumerable<int> fileId,
            IEnumerable<Guid> notifyUserList)
        {
            if (!string.IsNullOrEmpty(entityType) &&
                !(
                     string.Compare(entityType, "opportunity", StringComparison.OrdinalIgnoreCase) == 0 ||
                     string.Compare(entityType, "case", StringComparison.OrdinalIgnoreCase) == 0)
                )
                throw new ArgumentException();

            var entityTypeObj = ToEntityType(entityType);

            var entityTitle = "";
            if (contactId > 0) {
                var contact = DaoFactory.GetContactDao().GetByID(contactId);
                if (contact == null || !CRMSecurity.CanAccessTo(contact))
                    throw new ArgumentException();
                entityTitle = contact.GetTitle();
            }

            if (entityTypeObj == EntityType.Case) {
                var cases = DaoFactory.GetCasesDao().GetByID(entityId);
                if (cases == null || !CRMSecurity.CanAccessTo(cases))
                    throw new ArgumentException();
                if (contactId <= 0)
                {
                    entityTitle = cases.Title;
                }
            }
            if (entityTypeObj == EntityType.Opportunity)
            {
                var deal = DaoFactory.GetDealDao().GetByID(entityId);
                if (deal == null || !CRMSecurity.CanAccessTo(deal))
                    throw new ArgumentException();
                if (contactId <= 0)
                {
                    entityTitle = deal.Title;
                }
            }

            var relationshipEvent = new RelationshipEvent
                {
                    CategoryID = categoryId,
                    EntityType = entityTypeObj,
                    EntityID = entityId,
                    Content = content,
                    ContactID = contactId,
                    CreateOn = created,
                    CreateBy = SecurityContext.CurrentAccount.ID
                };

            var category = DaoFactory.GetListItemDao().GetByID(categoryId);
            if (category == null) throw new ArgumentException();

            var item = DaoFactory.GetRelationshipEventDao().CreateItem(relationshipEvent);


            notifyUserList = notifyUserList != null ? notifyUserList.ToList() : new List<Guid>();
            var needNotify = notifyUserList.Any();

            var fileListInfoHashtable = new Hashtable();

            if (fileId != null)
            {
                var fileIds = fileId.ToList();
                var files = FilesDaoFactory.GetFileDao<int>().GetFiles(fileIds.ToArray());

                if (needNotify)
                {
                    foreach (var file in files)
                    {
                        var extension = Path.GetExtension(file.Title);
                        if (extension == null) continue;

                        var fileInfo = string.Format("{0} ({1})", file.Title, extension.ToUpper());
                        if (!fileListInfoHashtable.ContainsKey(fileInfo))
                        {
                            fileListInfoHashtable.Add(fileInfo, file.DownloadUrl);
                        }
                        else
                        {
                            fileInfo = string.Format("{0} ({1}, {2})", file.Title, extension.ToUpper(), file.UniqID);
                            fileListInfoHashtable.Add(fileInfo, file.DownloadUrl);
                        }
                    }
                }

                DaoFactory.GetRelationshipEventDao().AttachFiles(item.ID, fileIds.ToArray());

                if (files.Any())
                {
                    var fileAttachAction = GetFilesAttachAction(entityTypeObj, contactId);
                    MessageService.Send( fileAttachAction, MessageTarget.Create(item.ID), entityTitle, files.Select(x => x.Title));
                }
            }

            if (needNotify)
            {
                NotifyClient.SendAboutAddRelationshipEventAdd(item, fileListInfoHashtable, DaoFactory, notifyUserList.ToArray());
            }

            var wrapper = RelationshipEventWrapperHelper.Get(item);

            var historyCreatedAction = GetHistoryCreatedAction(entityTypeObj, contactId);
            MessageService.Send( historyCreatedAction, MessageTarget.Create(item.ID), entityTitle, category.Title);

            return wrapper;
        }

        /// <summary>
        ///     Associates the selected file(s) with the entity with the ID or type specified in the request
        /// </summary>
        /// <short>
        ///     Associate file with entity
        /// </short>
        /// <param name="entityType">Entity type</param>
        /// <param name="entityid">Entity ID</param>
        /// <param name="fileids">List of IDs of the files</param>
        /// <category>Files</category>
        /// <returns>Entity with the file attached</returns>
        [Create(@"{entityType:(contact|opportunity|case)}/{entityid:int}/files")]
        public RelationshipEventWrapper AttachFiles(string entityType, int entityid, IEnumerable<int> fileids)
        {
            if (entityid <= 0 || fileids == null) throw new ArgumentException();

            var files = FilesDaoFactory.GetFileDao<int>().GetFiles(fileids.ToArray());

            var folderid = GetRootFolderID();

            if (files.Exists(file => file.FolderID.ToString() != folderid.ToString()))
                throw new ArgumentException("invalid file folder");

            var entityTypeObj = ToEntityType(entityType);

            DomainObject entityObj;

            var entityTitle = GetEntityTitle(entityTypeObj, entityid, true, out entityObj);

            switch (entityTypeObj)
            {
                case EntityType.Contact:
                    var relationshipEvent1 = DaoFactory.GetRelationshipEventDao().AttachFiles(entityid, EntityType.Any, 0, fileids.ToArray());
                    var messageAction = entityObj is Company ? MessageAction.CompanyAttachedFiles : MessageAction.PersonAttachedFiles;
                    MessageService.Send( messageAction, MessageTarget.Create(entityid), entityTitle, files.Select(x => x.Title));
                    return RelationshipEventWrapperHelper.Get(relationshipEvent1);
                case EntityType.Opportunity:
                    var relationshipEvent2 = DaoFactory.GetRelationshipEventDao().AttachFiles(0, entityTypeObj, entityid, fileids.ToArray());
                    MessageService.Send( MessageAction.OpportunityAttachedFiles, MessageTarget.Create(entityid), entityTitle, files.Select(x => x.Title));
                    return RelationshipEventWrapperHelper.Get(relationshipEvent2);
                case EntityType.Case:
                    var relationshipEvent3 = DaoFactory.GetRelationshipEventDao().AttachFiles(0, entityTypeObj, entityid, fileids.ToArray());
                    MessageService.Send( MessageAction.CaseAttachedFiles, MessageTarget.Create(entityid), entityTitle, files.Select(x => x.Title));
                    return RelationshipEventWrapperHelper.Get(relationshipEvent3);
                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        ///     Returns the ID for the root folder used to store the files for the CRM module
        /// </summary>
        /// <short>Get root folder ID</short> 
        /// <category>Files</category>
        /// <returns>
        ///   Root folder ID
        /// </returns>
        [Read(@"files/root")]
        public int GetRootFolderID()
        {
            return DaoFactory.GetFileDao().GetRoot();
        }

        /// <summary>
        ///    Returns the list of all files for the entity with the ID or type specified in the request
        /// </summary>
        /// <param name="entityType">Entity type</param>
        /// <param name="entityid">Entity ID</param>
        /// <short>Get file list</short> 
        /// <category>Files</category>
        /// <returns>
        ///    File list
        /// </returns>
        [Read(@"{entityType:(contact|opportunity|case)}/{entityid:int}/files")]
        public IEnumerable<FileWrapper<int>> GetFiles(string entityType, int entityid)
        {
            if (entityid <= 0) throw new ArgumentException();

            var entityTypeObj = ToEntityType(entityType);

            switch (entityTypeObj)
            {
                case EntityType.Contact:
                    return DaoFactory.GetRelationshipEventDao().GetAllFiles(new[] {entityid}, EntityType.Any, 0).ConvertAll(file => FileWrapperHelper.Get<int>(file));
                case EntityType.Opportunity:
                case EntityType.Case:
                    return DaoFactory.GetRelationshipEventDao().GetAllFiles(null, entityTypeObj, entityid).ConvertAll(file => FileWrapperHelper.Get<int>(file));
                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        ///     Deletes the file with the ID specified in the request
        /// </summary>
        /// <short>Delete file</short> 
        /// <category>Files</category>
        /// <param name="fileid">File ID</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <returns>
        ///    File Info
        /// </returns>
        [Delete(@"files/{fileid:int}")]
        public FileWrapper<int> DeleteCRMFile(int fileid)
        {
            if (fileid < 0) throw new ArgumentException();

            var file = FilesDaoFactory.GetFileDao<int>().GetFile(fileid);
            if (file == null) throw new ItemNotFoundException();
            var result = FileWrapperHelper.Get<int>(file);

            var _eventsDao = DaoFactory.GetRelationshipEventDao();
            var eventIDs = _eventsDao.RemoveFile(file);
            var events = new List<RelationshipEvent>();

            eventIDs.ForEach(id => events.Add(_eventsDao.GetByID(id)));

            foreach (var evt in events)
            {
                DomainObject entityObj;
                var entityTitle = evt.ContactID > 0
                                  ? GetEntityTitle(EntityType.Contact, evt.ContactID, false, out entityObj)
                                  : GetEntityTitle(evt.EntityType, evt.EntityID, false, out entityObj);
                var messageAction = GetFilesDetachAction(evt.EntityType, evt.ContactID);

                MessageService.Send( messageAction, MessageTarget.Create(file.ID), entityTitle, file.Title);
            }

            return result;
        }

        private IEnumerable<RelationshipEventWrapper> ToListRelationshipEventWrapper(List<RelationshipEvent> itemList)
        {
            if (itemList.Count == 0) return new List<RelationshipEventWrapper>();

            var result = new List<RelationshipEventWrapper>();

            var contactIDs = new List<int>();
            var eventIDs = new List<int>();
            var categoryIDs = new List<int>();
            var entityWrappersIDs = new Dictionary<EntityType, List<int>>();


            foreach (var item in itemList)
            {
                eventIDs.Add(item.ID);

                if (!categoryIDs.Contains(item.CategoryID))
                {
                    categoryIDs.Add(item.CategoryID);
                }

                if (item.ContactID > 0 && !contactIDs.Contains(item.ContactID))
                {
                    contactIDs.Add(item.ContactID);
                }

                if (item.EntityID <= 0) continue;

                if (!entityWrappersIDs.ContainsKey(item.EntityType))
                {
                    entityWrappersIDs.Add(item.EntityType, new List<int>
                        {
                            item.EntityID
                        });
                }
                else if (!entityWrappersIDs[item.EntityType].Contains(item.EntityID))
                {
                    entityWrappersIDs[item.EntityType].Add(item.EntityID);
                }
            }

            var entityWrappers = new Dictionary<string, EntityWrapper>();

            foreach (var entityType in entityWrappersIDs.Keys)
            {
                switch (entityType)
                {
                    case EntityType.Opportunity:
                        DaoFactory.GetDealDao().GetDeals(entityWrappersIDs[entityType].Distinct().ToArray())
                                  .ForEach(item =>
                                      {
                                          if (item == null) return;

                                          entityWrappers.Add(
                                              string.Format("{0}_{1}", (int)entityType, item.ID),
                                              new EntityWrapper
                                                  {
                                                      EntityId = item.ID,
                                                      EntityTitle = item.Title,
                                                      EntityType = "opportunity"
                                                  });
                                      });
                        break;
                    case EntityType.Case:
                        DaoFactory.GetCasesDao().GetByID(entityWrappersIDs[entityType].ToArray())
                                  .ForEach(item =>
                                      {
                                          if (item == null) return;

                                          entityWrappers.Add(
                                              string.Format("{0}_{1}", (int)entityType, item.ID),
                                              new EntityWrapper
                                                  {
                                                      EntityId = item.ID,
                                                      EntityTitle = item.Title,
                                                      EntityType = "case"
                                                  });
                                      });
                        break;
                    default:
                        throw new ArgumentException();
                }
            }

            var categories = DaoFactory.GetListItemDao().GetItems(categoryIDs.ToArray()).ToDictionary(x => x.ID, x => new HistoryCategoryBaseWrapper(x));

            var files = DaoFactory.GetRelationshipEventDao().GetFiles(eventIDs.ToArray());

            var contacts = DaoFactory.GetContactDao().GetContacts(contactIDs.ToArray()).ToDictionary(item => item.ID, x => ContactWrapperHelper.GetContactBaseWrapper(x));

            foreach (var item in itemList)
            {
                var eventObjWrap = RelationshipEventWrapperHelper.Get(item);

                if (contacts.ContainsKey(item.ContactID))
                {
                    eventObjWrap.Contact = contacts[item.ContactID];
                }

                if (item.EntityID > 0)
                {
                    var entityStrKey = string.Format("{0}_{1}", (int)item.EntityType, item.EntityID);

                    if (entityWrappers.ContainsKey(entityStrKey))
                    {
                        eventObjWrap.Entity = entityWrappers[entityStrKey];
                    }
                }

                eventObjWrap.Files = files.ContainsKey(item.ID) ? files[item.ID].ConvertAll(file =>
                FileWrapperHelper.Get<int>(file)) : new List<FileWrapper<int>>();

                if (categories.ContainsKey(item.CategoryID))
                {
                    eventObjWrap.Category = categories[item.CategoryID];
                }

                result.Add(eventObjWrap);
            }

            return result;
        }

        private EntityWrapper ToEntityWrapper(EntityType entityType, int entityID)
        {
            if (entityID == 0) return null;

            var result = new EntityWrapper
                {
                    EntityId = entityID
                };

            switch (entityType)
            {
                case EntityType.Case:
                    var caseObj = DaoFactory.GetCasesDao().GetByID(entityID);
                    if (caseObj == null)
                        return null;

                    result.EntityType = "case";
                    result.EntityTitle = caseObj.Title;

                    break;
                case EntityType.Opportunity:
                    var dealObj = DaoFactory.GetDealDao().GetByID(entityID);
        
                    if (dealObj == null)
                        return null;

                    result.EntityType = "opportunity";
                    result.EntityTitle = dealObj.Title;

                    break;
                default:
                    return null;
            }

            return result;
        }


        private MessageAction GetHistoryCreatedAction(EntityType entityType, int contactId)
        {
            if (contactId > 0)
            {
                var contact = DaoFactory.GetContactDao().GetByID(contactId);
                return contact is Company ? MessageAction.CompanyCreatedHistoryEvent : MessageAction.PersonCreatedHistoryEvent;
            }

            switch (entityType)
            {
                case EntityType.Opportunity:
                    return MessageAction.OpportunityCreatedHistoryEvent;
                case EntityType.Case:
                    return MessageAction.CaseCreatedHistoryEvent;
                case EntityType.Any:
                    var contact = DaoFactory.GetContactDao().GetByID(contactId);
                    return contact is Company ? MessageAction.CompanyCreatedHistoryEvent : MessageAction.PersonCreatedHistoryEvent;
                default:
                    throw new ArgumentException("Invalid entityType: " + entityType);
            }
        }

        private MessageAction GetHistoryDeletedAction(EntityType entityType, int contactId)
        {
            if (contactId > 0)
            {
                var contact = DaoFactory.GetContactDao().GetByID(contactId);
                return contact is Company ? MessageAction.CompanyDeletedHistoryEvent : MessageAction.PersonDeletedHistoryEvent;
            }

            switch (entityType)
            {
                case EntityType.Opportunity:
                    return MessageAction.OpportunityDeletedHistoryEvent;
                case EntityType.Case:
                    return MessageAction.CaseDeletedHistoryEvent;
                case EntityType.Any:
                    var contact = DaoFactory.GetContactDao().GetByID(contactId);
                    return contact is Company ? MessageAction.CompanyDeletedHistoryEvent : MessageAction.PersonDeletedHistoryEvent;
                default:
                    throw new ArgumentException("Invalid entityType: " + entityType);
            }
        }

        private MessageAction GetFilesAttachAction(EntityType entityType, int contactId)
        {
            if (contactId > 0)
            {
                var contact = DaoFactory.GetContactDao().GetByID(contactId);
                return contact is Company ? MessageAction.CompanyAttachedFiles : MessageAction.PersonAttachedFiles;
            }

            switch (entityType)
            {
                case EntityType.Opportunity:
                    return MessageAction.OpportunityAttachedFiles;
                case EntityType.Case:
                    return MessageAction.CaseAttachedFiles;
                case EntityType.Any:
                    var contact = DaoFactory.GetContactDao().GetByID(contactId);
                    return contact is Company ? MessageAction.CompanyAttachedFiles : MessageAction.PersonAttachedFiles;
                default:
                    throw new ArgumentException("Invalid entityType: " + entityType);
            }
        }

        private MessageAction GetFilesDetachAction(EntityType entityType, int contactId)
        {
            if (contactId > 0)
            {
                var contact = DaoFactory.GetContactDao().GetByID(contactId);
                return contact is Company ? MessageAction.CompanyDetachedFile : MessageAction.PersonDetachedFile;
            }

            switch (entityType)
            {
                case EntityType.Opportunity:
                    return MessageAction.OpportunityDetachedFile;
                case EntityType.Case:
                    return MessageAction.CaseDetachedFile;
                case EntityType.Any:
                    var contact = DaoFactory.GetContactDao().GetByID(contactId);
                    return contact is Company ? MessageAction.CompanyDetachedFile : MessageAction.PersonAttachedFiles;
                default:
                    throw new ArgumentException("Invalid entityType: " + entityType);
            }
        }
    }
}