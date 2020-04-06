﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using ASC.Common;
using ASC.Core.Common.EF;
using ASC.Mail.Core.Dao.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ASC.Mail.Core.Dao
{
    public partial class MailDbContext : BaseDbContext
    {
        public MailDbContext()
        {
        }

        public MailDbContext(DbContextOptions<MailDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<MailAlerts> MailAlerts { get; set; }
        public virtual DbSet<MailAttachment> MailAttachment { get; set; }
        public virtual DbSet<MailChain> MailChain { get; set; }
        public virtual DbSet<MailChainXCrmEntity> MailChainXCrmEntity { get; set; }
        public virtual DbSet<MailContactInfo> MailContactInfo { get; set; }
        public virtual DbSet<MailContacts> MailContacts { get; set; }
        public virtual DbSet<MailDisplayImages> MailDisplayImages { get; set; }
        public virtual DbSet<MailFilter> MailFilter { get; set; }
        public virtual DbSet<MailFolder> MailFolder { get; set; }
        public virtual DbSet<MailFolderCounters> MailFolderCounters { get; set; }
        public virtual DbSet<MailImapFlags> MailImapFlags { get; set; }
        public virtual DbSet<MailImapSpecialMailbox> MailImapSpecialMailbox { get; set; }
        public virtual DbSet<MailMail> MailMail { get; set; }
        public virtual DbSet<MailMailbox> MailMailbox { get; set; }
        public virtual DbSet<MailMailboxAutoreply> MailMailboxAutoreply { get; set; }
        public virtual DbSet<MailMailboxAutoreplyHistory> MailMailboxAutoreplyHistory { get; set; }
        public virtual DbSet<MailMailboxDomain> MailMailboxDomain { get; set; }
        public virtual DbSet<MailMailboxProvider> MailMailboxProvider { get; set; }
        public virtual DbSet<MailMailboxServer> MailMailboxServer { get; set; }
        public virtual DbSet<MailMailboxSignature> MailMailboxSignature { get; set; }
        public virtual DbSet<MailPopUnorderedDomain> MailPopUnorderedDomain { get; set; }
        public virtual DbSet<MailServerAddress> MailServerAddress { get; set; }
        public virtual DbSet<MailServerDns> MailServerDns { get; set; }
        public virtual DbSet<MailServerDomain> MailServerDomain { get; set; }
        public virtual DbSet<MailServerMailGroup> MailServerMailGroup { get; set; }
        public virtual DbSet<MailServerMailGroupXMailServerAddress> MailServerMailGroupXMailServerAddress { get; set; }
        public virtual DbSet<MailServerServer> MailServerServer { get; set; }
        public virtual DbSet<MailServerServerType> MailServerServerType { get; set; }
        public virtual DbSet<MailServerServerXTenant> MailServerServerXTenant { get; set; }
        public virtual DbSet<MailTag> MailTag { get; set; }
        public virtual DbSet<CrmTag> CrmTag { get; set; }
        public virtual DbSet<CrmEntityTag> CrmEntityTag { get; set; }
        public virtual DbSet<MailTagAddresses> MailTagAddresses { get; set; }
        public virtual DbSet<MailTagMail> MailTagMail { get; set; }
        public virtual DbSet<MailUserFolder> MailUserFolder { get; set; }
        public virtual DbSet<MailUserFolderTree> MailUserFolderTree { get; set; }
        public virtual DbSet<MailUserFolderXMail> MailUserFolderXMail { get; set; }
        public virtual DbSet<CrmContact> CrmContact { get; set; }
        public virtual DbSet<CrmContactInfo> CrmContactInfo { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //    }
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MailAlerts>(entity =>
            {
                entity.HasIndex(e => new { e.Tenant, e.IdUser, e.IdMailbox, e.Type })
                    .HasName("tenant_id_user_id_mailbox_type");

                entity.Property(e => e.Data)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.IdMailbox).HasDefaultValueSql("'-1'");

                entity.Property(e => e.IdUser)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailAttachment>(entity =>
            {
                entity.HasIndex(e => new { e.IdMail, e.ContentId })
                    .HasName("id_mail");

                entity.HasIndex(e => new { e.IdMailbox, e.Tenant })
                    .HasName("id_mailbox");

                entity.HasIndex(e => new { e.Tenant, e.IdMail })
                    .HasName("tenant");

                entity.Property(e => e.ContentId)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Name)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.StoredName)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Type)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.HasOne(a => a.Mail)
                    .WithMany(m => m.Attachments)
                    .HasForeignKey(a => a.IdMail);
                    //.HasPrincipalKey(o => o.Id);

            });

            modelBuilder.Entity<MailChain>(entity =>
            {
                entity.HasKey(e => new { e.Tenant, e.IdUser, e.Id, e.IdMailbox, e.Folder })
                    .HasName("PRIMARY");

                entity.Property(e => e.IdUser)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Id)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Tags)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailChainXCrmEntity>(entity =>
            {
                entity.HasKey(e => new { e.IdTenant, e.IdMailbox, e.IdChain, e.EntityId, e.EntityType })
                    .HasName("PRIMARY");

                entity.Property(e => e.IdChain)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailContactInfo>(entity =>
            {
                entity.HasIndex(e => e.IdContact)
                    .HasName("contact_id");

                entity.HasIndex(e => e.LastModified)
                    .HasName("last_modified");

                entity.HasIndex(e => new { e.Tenant, e.IdUser, e.Data })
                    .HasName("tenant_id_user_data");

                entity.Property(e => e.Data)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.IdUser)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.LastModified)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<MailContacts>(entity =>
            {
                entity.HasIndex(e => e.LastModified)
                    .HasName("last_modified");

                entity.HasIndex(e => new { e.Tenant, e.IdUser, e.Address })
                    .HasName("tenant_id_user_name_address");

                entity.Property(e => e.Address)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Description)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.IdUser)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.LastModified)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.Name)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailDisplayImages>(entity =>
            {
                entity.HasKey(e => new { e.Tenant, e.IdUser, e.Address })
                    .HasName("PRIMARY");

                entity.Property(e => e.IdUser)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Address)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailFilter>(entity =>
            {
                entity.HasIndex(e => new { e.Tenant, e.IdUser })
                    .HasName("tenant_id_user");

                entity.Property(e => e.DateModified)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.Enabled).HasDefaultValueSql("'1'");

                entity.Property(e => e.Filter)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.IdUser)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailFolder>(entity =>
            {
                entity.HasKey(e => new { e.Tenant, e.IdUser, e.Folder })
                    .HasName("PRIMARY");

                entity.Property(e => e.IdUser)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.TimeModified)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<MailFolderCounters>(entity =>
            {
                entity.HasKey(e => new { e.Tenant, e.IdUser, e.Folder })
                    .HasName("PRIMARY");

                entity.Property(e => e.IdUser)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.TimeModified)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<MailImapFlags>(entity =>
            {
                entity.HasKey(e => e.Name)
                    .HasName("PRIMARY");

                entity.Property(e => e.Name)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailImapSpecialMailbox>(entity =>
            {
                entity.HasKey(e => new { e.Server, e.Name })
                    .HasName("PRIMARY");

                entity.Property(e => e.Server)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Name)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailMail>(entity =>
            {
                entity.HasIndex(e => e.TimeModified)
                    .HasName("time_modified");

                entity.HasIndex(e => new { e.IdMailbox, e.MimeMessageId })
                    .HasName("mime_message_id");

                entity.HasIndex(e => new { e.Md5, e.IdMailbox })
                    .HasName("md5");

                entity.HasIndex(e => new { e.Uidl, e.IdMailbox })
                    .HasName("uidl");

                entity.HasIndex(e => new { e.ChainId, e.IdMailbox, e.Folder })
                    .HasName("chain_index_folders");

                entity.HasIndex(e => new { e.Tenant, e.IdUser, e.Folder, e.ChainDate })
                    .HasName("list_conversations");

                entity.HasIndex(e => new { e.Tenant, e.IdUser, e.Folder, e.DateSent })
                    .HasName("list_messages");

                entity.Property(e => e.Address)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Bcc)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.CalendarUid)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Cc)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.ChainDate).HasDefaultValueSql("'1975-01-01 00:00:00'");

                entity.Property(e => e.ChainId)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.DateReceived).HasDefaultValueSql("'1975-01-01 00:00:00'");

                entity.Property(e => e.DateSent).HasDefaultValueSql("'1975-01-01 00:00:00'");

                entity.Property(e => e.Folder).HasDefaultValueSql("'1'");

                entity.Property(e => e.FolderRestore).HasDefaultValueSql("'1'");

                entity.Property(e => e.FromText)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.IdUser)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Introduction)
                    .HasDefaultValueSql("''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Md5)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.MimeInReplyTo)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.MimeMessageId)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.ReplyTo)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Stream)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Subject)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.TimeModified)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.ToText)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Uidl)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.HasMany(m => m.Attachments)
                    .WithOne(a => a.Mail)
                    .HasForeignKey(a => a.IdMail);
            });

            modelBuilder.Entity<MailMailbox>(entity =>
            {
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
                    //.HasConversion(v => (uint)v, v => (int)v);

                entity.HasIndex(e => e.Address)
                    .HasName("address_index");

                entity.HasIndex(e => e.IdInServer)
                    .HasName("main_mailbox_id_in_server_mail_mailbox_server_id");

                entity.HasIndex(e => e.IdSmtpServer)
                    .HasName("main_mailbox_id_smtp_server_mail_mailbox_server_id");

                entity.HasIndex(e => new { e.DateChecked, e.DateLoginDelayExpires })
                    .HasName("date_login_delay_expires");

                entity.HasIndex(e => new { e.Tenant, e.IdUser })
                    .HasName("user_id_index");

                entity.Property(e => e.Address)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.BeginDate).HasDefaultValueSql("'1975-01-01 00:00:00'");

                entity.Property(e => e.DateLoginDelayExpires).HasDefaultValueSql("'1975-01-01 00:00:00'");

                entity.Property(e => e.DateModified)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.EmailInFolder)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Enabled).HasDefaultValueSql("'1'");

                entity.Property(e => e.IdUser)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.ImapIntervals)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.LoginDelay).HasDefaultValueSql("'30'");

                entity.Property(e => e.Name)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Pop3Password)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.SmtpPassword)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Token)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailMailboxAutoreply>(entity =>
            {
                entity.HasKey(e => e.IdMailbox)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.Tenant)
                    .HasName("tenant");

                entity.Property(e => e.Html)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Subject)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailMailboxAutoreplyHistory>(entity =>
            {
                entity.HasKey(e => new { e.IdMailbox, e.SendingEmail })
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.Tenant)
                    .HasName("tenant");

                entity.Property(e => e.SendingEmail)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailMailboxDomain>(entity =>
            {
                entity.HasIndex(e => new { e.Name, e.IdProvider })
                    .HasName("id_provider");

                entity.Property(e => e.Name)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailMailboxProvider>(entity =>
            {
                entity.Property(e => e.DisplayName)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.DisplayShortName)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Documentation)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Name)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailMailboxServer>(entity =>
            {
                entity.HasIndex(e => e.IdProvider)
                    .HasName("id_provider");

                entity.Property(e => e.Authentication)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Hostname)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.SocketType)
                    .HasDefaultValueSql("'plain'")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Type)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Username)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailMailboxSignature>(entity =>
            {
                entity.HasKey(e => e.IdMailbox)
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.Tenant)
                    .HasName("tenant");

                entity.Property(e => e.Html)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailPopUnorderedDomain>(entity =>
            {
                entity.HasKey(e => e.Server)
                    .HasName("PRIMARY");

                entity.Property(e => e.Server)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailServerAddress>(entity =>
            {
                entity.HasIndex(e => e.IdDomain)
                    .HasName("domain_index");

                entity.HasIndex(e => e.IdMailbox)
                    .HasName("id_mailbox_fk_index");

                entity.Property(e => e.Name)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailServerDns>(entity =>
            {
                entity.HasIndex(e => new { e.IdDomain, e.Tenant, e.IdUser })
                    .HasName("id_domain_tenant_id_user");

                entity.Property(e => e.DkimPrivateKey)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.DkimPublicKey)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.DkimSelector)
                    .HasDefaultValueSql("'dkim'")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.DkimTtl).HasDefaultValueSql("'300'");

                entity.Property(e => e.DomainCheck)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.IdDomain).HasDefaultValueSql("'-1'");

                entity.Property(e => e.IdUser)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Mx)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.MxTtl).HasDefaultValueSql("'300'");

                entity.Property(e => e.Spf)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.SpfTtl).HasDefaultValueSql("'300'");

                entity.Property(e => e.TimeModified)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<MailServerDomain>(entity =>
            {
                entity.HasIndex(e => e.DateChecked)
                    .HasName("date_checked");

                entity.HasIndex(e => e.Name)
                    .HasName("name")
                    .IsUnique();

                entity.HasIndex(e => e.Tenant)
                    .HasName("tenant");

                entity.Property(e => e.DateChecked).HasDefaultValueSql("'1975-01-01 00:00:00'");

                entity.Property(e => e.Name)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailServerMailGroup>(entity =>
            {
                entity.HasIndex(e => e.IdAddress)
                    .HasName("mail_server_address_fk_id");

                entity.HasIndex(e => e.IdTenant)
                    .HasName("tenant");

                entity.Property(e => e.Address)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailServerMailGroupXMailServerAddress>(entity =>
            {
                entity.HasKey(e => new { e.IdAddress, e.IdMailGroup })
                    .HasName("PRIMARY");
            });

            modelBuilder.Entity<MailServerServer>(entity =>
            {
                entity.HasIndex(e => e.ServerType)
                    .HasName("mail_server_server_type_server_type_fk_id");

                entity.Property(e => e.ConnectionString)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.MxRecord)
                    .HasDefaultValueSql("''")
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailServerServerType>(entity =>
            {
                entity.Property(e => e.Name)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailServerServerXTenant>(entity =>
            {
                entity.HasKey(e => new { e.IdTenant, e.IdServer })
                    .HasName("PRIMARY");
            });

            modelBuilder.Entity<MailTag>(entity =>
            {
                entity.HasIndex(e => new { e.Tenant, e.IdUser })
                    .HasName("username");

                entity.Property(e => e.Addresses)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.IdUser)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Name)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Style)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<CrmTag>(entity =>
            {
                entity.HasIndex(e => new { e.IdTenant })
                    .HasName("username");

                entity.Property(e => e.Title)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.EntityType)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<CrmEntityTag>(entity =>
            {
                entity.HasIndex(e => new { e.TagId, e.EntityId, e.EntityType })
                    .HasName("PRIMARY");

                entity.Property(e => e.TagId)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.EntityId)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.EntityType)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailTagAddresses>(entity =>
            {
                entity.HasKey(e => new { e.IdTag, e.Address, e.Tenant })
                    .HasName("PRIMARY");

                entity.Property(e => e.Address)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailTagMail>(entity =>
            {
                entity.HasKey(e => new { e.Tenant, e.IdUser, e.IdMail, e.IdTag })
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.IdMail)
                    .HasName("id_mail");

                entity.HasIndex(e => e.IdTag)
                    .HasName("id_tag");

                entity.Property(e => e.IdUser)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.TimeCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<MailUserFolder>(entity =>
            {
                entity.HasIndex(e => new { e.Tenant, e.IdUser, e.ParentId })
                    .HasName("tenant_user_parent");

                entity.Property(e => e.IdUser)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.ModifiedOn)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.Name)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<MailUserFolderTree>(entity =>
            {
                entity.HasKey(e => new { e.ParentId, e.FolderId })
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.FolderId)
                    .HasName("folder_id");
            });

            modelBuilder.Entity<MailUserFolderXMail>(entity =>
            {
                entity.HasKey(e => new { e.Tenant, e.IdUser, e.IdMail, e.IdFolder })
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.IdFolder)
                    .HasName("id_tag");

                entity.HasIndex(e => e.IdMail)
                    .HasName("id_mail");

                entity.Property(e => e.IdUser)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.TimeCreated)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<CrmContact>(entity =>
            {
                entity.HasIndex(e => e.CreateOn)
                    .HasName("create_on");

                entity.HasIndex(e => new { e.LastModifedOn, e.TenantId })
                    .HasName("last_modifed_on");

                entity.HasIndex(e => new { e.TenantId, e.CompanyId })
                    .HasName("company_id");

                entity.HasIndex(e => new { e.TenantId, e.DisplayName })
                    .HasName("display_name");

                entity.Property(e => e.CompanyName)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.CreateBy)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Currency)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.DisplayName)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.FirstName)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Industry)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.LastModifedBy)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.LastName)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Notes)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.Title)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            modelBuilder.Entity<CrmContactInfo>(entity =>
            {
                entity.HasIndex(e => e.LastModifedOn)
                    .HasName("last_modifed_on");

                entity.HasIndex(e => new { e.TenantId, e.ContactId })
                    .HasName("IX_Contact");

                entity.Property(e => e.Data)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");

                entity.Property(e => e.LastModifedBy)
                    .HasCharSet("utf8")
                    .HasCollation("utf8_general_ci");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

    public static class MailDbExtension
    {
        public static DIHelper AddMailDbContextService(this DIHelper services)
        {
            return services.AddDbContextManagerService<MailDbContext>();
        }
    }
}