﻿using ASC.Mail.Core.Dao.Expressions.Mailbox;
using ASC.Mail.Enums;
using ASC.Mail.Exceptions;
using ASC.Mail.Extensions;
using ASC.Mail.Models;
using ASC.Web.Api.Routing;
using ASC.Web.Mail.Resources;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace ASC.Mail.Controllers
{
    public partial class MailController : ControllerBase
    {
        /// <summary>
        ///    Returns lists of all mailboxes, aliases and groups for user.
        /// </summary>
        /// <param name="username" visible="false">User id</param>
        /// <returns>Mailboxes, aliases and groups list</returns>
        /// <short>Get mailboxes, aliases and groups list</short> 
        /// <category>Accounts</category>
        [Read("accounts")]
        public IEnumerable<MailAccountData> GetAccounts()
        {
            var accounts = AccountEngine.GetAccountInfoList();
            return accounts.ToAccountData();
        }

        /// <summary>
        ///    Returns the information about the account.
        /// </summary>
        /// <param name="email">Account email address</param>
        /// <returns>Account with specified email</returns>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        /// <exception cref="NullReferenceException">Exception happens when mailbox wasn't found by email.</exception>
        /// <short>Get account by email</short> 
        /// <category>Accounts</category>
        [Read(@"accounts/single")]
        public MailBoxData GetAccount(string email)
        {
            var account = AccountEngine.GetAccount(email);

            return account;
        }

        /// <summary>
        ///    Creates account using full information about mail servers.
        /// </summary>
        /// <param name="model">instance of AccountModel</param>
        /// <returns>Created account</returns>
        /// <exception cref="Exception">Exception contains text description of happened error.</exception>
        /// <short>Create account with custom mail servers.</short> 
        /// <category>Accounts</category>
        [Create("accounts")]
        public MailAccountData CreateAccount(AccountModel model)
        {
            var accountInfo = AccountEngine.TryCreateAccount(model, out LoginResult loginResult);

            if (accountInfo == null)
                throw new LoginException(loginResult);

            return accountInfo.ToAccountData().FirstOrDefault();
        }

        /// <summary>
        ///    Creates an account based on email and password.
        /// </summary>
        /// <param name="email">Account email in string format like: name@domain</param>
        /// <param name="password">Password as plain text.</param>
        /// <exception cref="ArgumentException">Exception happens when in parameters is invalid. Text description contains parameter name and text description.</exception>
        /// <exception cref="Exception">Exception contains text description of happened error.</exception>
        /// <returns>Created account</returns>
        /// <short>Create new account by email and password</short> 
        /// <category>Accounts</category>
        [Create(@"accounts/simple")]
        public MailAccountData CreateAccountSimple(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException(@"Empty email", "email");

            //Thread.CurrentThread.CurrentCulture = CurrentCulture;
            //Thread.CurrentThread.CurrentUICulture = CurrentCulture;

            string errorText = null;

            try
            {
                var account = AccountEngine.CreateAccountSimple(email, password, out List<LoginResult> loginResults);

                if (account != null)
                    return account.ToAccountData().FirstOrDefault();

                var i = 0;

                foreach (var loginResult in loginResults)
                {
                    errorText += string.Format("#{0}:<br>", ++i);

                    if (!loginResult.IngoingSuccess)
                    {
                        errorText += GetFormattedTextError(loginResult.IngoingException,
                            loginResult.Imap ? ServerType.Imap : ServerType.Pop3, false) + "<br>";
                    }

                    if (!loginResult.OutgoingSuccess)
                    {
                        errorText += GetFormattedTextError(loginResult.OutgoingException, ServerType.Smtp, false) +
                                     "<br>";
                    }
                }

            }
            catch (Exception ex)
            {
                //TODO: change AttachmentsUnknownError to common unknown error text
                errorText = GetFormattedTextError(ex, MailApiResource.AttachmentsUnknownError);
            }

            throw new Exception(errorText);
        }

        private static string GetFormattedTextError(Exception ex, ServerType mailServerType, bool timeoutFlag = true)
        {
            var headerText = string.Empty;
            var errorExplain = string.Empty;

            switch (mailServerType)
            {
                case ServerType.Imap:
                case ServerType.ImapOAuth:
                    headerText = MailApiResource.ImapResponse;
                    if (timeoutFlag)
                        errorExplain = MailApiResource.ImapConnectionTimeoutError;
                    break;
                case ServerType.Pop3:
                    headerText = MailApiResource.Pop3Response;
                    if (timeoutFlag)
                        errorExplain = MailApiResource.Pop3ConnectionTimeoutError;
                    break;
                case ServerType.Smtp:
                    headerText = MailApiResource.SmtRresponse;
                    if (timeoutFlag)
                        errorExplain = MailApiResource.SmtpConnectionTimeoutError;
                    break;
            }

            return GetFormattedTextError(ex, errorExplain, headerText);

        }

        //TODO: Remove HTML tags from response data
        private static string GetFormattedTextError(Exception ex, string errorExplain = "", string headerText = "")
        {
            if (!string.IsNullOrEmpty(headerText))
                headerText = string.Format("<span class=\"attempt_header\">{0}</span><br/>", headerText);

            if (string.IsNullOrEmpty(errorExplain))
                errorExplain = ex.InnerException == null ||
                                string.IsNullOrEmpty(ex.InnerException.Message)
                                    ? ex.Message
                                    : ex.InnerException.Message;

            var errorText = string.Format("{0}{1}",
                          headerText,
                          errorExplain);

            return errorText;
        }
    }
}