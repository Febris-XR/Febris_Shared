// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.EnumLibrary;
using Febris.ModelLibrary.Models.EmailModels;
using Febris.ModelLibrary.Models.MarketingModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Febris.SharedServices
{
    // Email transport shapes (TestEmailViewModel, HostEmailProperties, EmailModel) moved to
    // Febris.ModelLibrary.Models.EmailModels per the "models live in FebrisModelLibrary" rule.
    // EmailType enum moved to Febris.EnumLibrary per the "all enums live in FebrisEnumLibrary" rule.

    public class EmailService : IEmailSender
    {
        private IConfiguration _config;

        #region unused
        //private IHostingEnvironment _env;

        //public EmailService(IConfiguration config,
        //    IHostingEnvironment env
        //    )
        //{
        //    _config = config;
        //    HostEmailProperties = new HostEmailProperties()
        //    {
        //        Host = _config.GetValue<string>("EmailSender:Host"),
        //        Port = _config.GetValue<int>("EmailSender:Port"),
        //        EnableSSL = _config.GetValue<bool>("EmailSender:EnableSSL"),
        //        Sender = _config.GetValue<string>("EmailSender:Sender"),
        //        Password = _config.GetValue<string>("EmailSender:Password"),
        //        SenderName = _config.GetValue<string>("EmailSender:SenderName")
        //    };
        //    _env = env;
        //}

        //public EmailService()
        //{

        //    HostEmailProperties = new HostEmailProperties()
        //    {
        //        Host = _config.GetValue<string>("EmailSender:Host"),
        //        Port = _config.GetValue<int>("EmailSender:Port"),
        //        EnableSSL = _config.GetValue<bool>("EmailSender:EnableSSL"),
        //        Sender = _config.GetValue<string>("EmailSender:Sender"),
        //        Password = _config.GetValue<string>("EmailSender:Password"),
        //        SenderName = _config.GetValue<string>("EmailSender:SenderName")
        //    };
        //}
        #endregion

        public EmailService(IConfiguration config)
        {
            _config = config;
            HostEmailProperties = new HostEmailProperties()
            {
                Host = _config.GetValue<string>("EmailSender:Host"),
                Port = _config.GetValue<int>("EmailSender:Port"),
                EnableSSL = _config.GetValue<bool>("EmailSender:EnableSSL"),
                Sender = _config.GetValue<string>("EmailSender:Sender"),
                Password = _config.GetValue<string>("EmailSender:Password"),
                SenderName = _config.GetValue<string>("EmailSender:SenderName"),
                // Absent key binds to false, which is what MailKit 2.10.1 did before the NET8
                // bump to 4.16.0 flipped the default. See HostEmailProperties for the full note.
                CheckCertificateRevocation = _config.GetValue<bool>("EmailSender:CheckCertificateRevocation")
            };
            ReturnUrl = _config.GetValue<string>("Branding:UnsubscribeBaseUrl") ?? string.Empty;
        }

        // Branding and topology a node operator must be able to set. Every default is empty
        // rather than a Febris-hosted URL: a self-hosted node must not beacon to infrastructure
        // its operator does not control, so an unset value degrades to "no logo" / "no link"
        // rather than to somebody else's host.
        private string BrandingLogoUrl => _config?.GetValue<string>("Branding:LogoUrl") ?? string.Empty;
        private string BrandingSchedulingUrl => _config?.GetValue<string>("Branding:SchedulingUrl") ?? string.Empty;

        #region Email Builder
        public HostEmailProperties HostEmailProperties { get; set; }
        public EmailModel EmailModel { get; set; }
        public List<EmailType> EmailTypeList { get; set; }
        public EmailType EmailType { get; set; }
        public string ReturnUrl { get; set; }
        #endregion

        public async Task<bool> SendEmail()
        {
            bool sent = false;
            try
            {
                MimeMessage message = new MimeMessage();

                MailboxAddress from = new MailboxAddress(HostEmailProperties.SenderName, HostEmailProperties.Sender);
                message.From.Add(from);

                MailboxAddress to = new MailboxAddress(EmailModel.RecipientName, EmailModel.RecipientEmailAddress);
                message.To.Add(to);


                if (string.IsNullOrEmpty(EmailModel.Subject))
                {
                    EmailModel.Subject = SetSubject(EmailType);
                }
                if (string.IsNullOrEmpty(EmailModel.Message))
                {
                    EmailModel.Message = SetMessage(EmailType);
                }

                message.Subject = EmailModel.Subject;

                message.Body = new TextPart("html")
                {
                    Text = EmailModel.Message
                };

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    // NOT the commented line below. That accepts ANY certificate and would mask a
                    // real man-in-the-middle. The actual failure was revocation, not the chain:
                    // MailKit 4.16 checks revocation by default where 2.10.1 did not, and rejects a
                    // valid certificate whenever the responder is unreachable. Chain and host name
                    // are still fully validated here.
                    //// For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    //client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.CheckCertificateRevocation = HostEmailProperties.CheckCertificateRevocation;

                    client.Connect(HostEmailProperties.Host, HostEmailProperties.Port, HostEmailProperties.EnableSSL);//MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable);

                    // Since we don't have an OAuth2 token, disable the XOAUTH2 authentication mechanism.
                    //client.AuthenticationMechanisms.Remove("XOAUTH2");

                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate(HostEmailProperties.Sender, HostEmailProperties.Password);

                    client.Send(message);
                    client.Disconnect(true);

                }
                sent = true;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }

            return sent;
        }

        public async Task<bool> SendEmail(string subject, string body)
        {
            bool sent = false;
            try
            {
                MimeMessage message = new MimeMessage();

                MailboxAddress from = new MailboxAddress(HostEmailProperties.SenderName, HostEmailProperties.Sender);
                message.From.Add(from);

                MailboxAddress to = new MailboxAddress(EmailModel.RecipientName, EmailModel.RecipientEmailAddress);
                message.To.Add(to);

                message.Subject = subject;

                message.Body = new TextPart("html")
                {
                    Text = body
                };

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    // NOT the commented line below. That accepts ANY certificate and would mask a
                    // real man-in-the-middle. The actual failure was revocation, not the chain:
                    // MailKit 4.16 checks revocation by default where 2.10.1 did not, and rejects a
                    // valid certificate whenever the responder is unreachable. Chain and host name
                    // are still fully validated here.
                    //// For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    //client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.CheckCertificateRevocation = HostEmailProperties.CheckCertificateRevocation;

                    client.Connect(HostEmailProperties.Host, HostEmailProperties.Port, HostEmailProperties.EnableSSL);//MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable);

                    // Since we don't have an OAuth2 token, disable the XOAUTH2 authentication mechanism.
                    //client.AuthenticationMechanisms.Remove("XOAUTH2");

                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate(HostEmailProperties.Sender, HostEmailProperties.Password);

                    client.Send(message);
                    client.Disconnect(true);

                }
                sent = true;

            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }



            return sent;
        }

        private string SetSubject(EmailType emailType)
        {
            string subject = string.Empty;

            switch (emailType)
            {
                #region Marketing                
                #region First Contact
                case EmailType.FirstContact:
                    subject = "Glad to meet you";
                    break;
                #endregion
                #region Question
                case EmailType.Question:
                    subject = "We see you asked a question";
                    break;
                #endregion
                #region Price Request
                case EmailType.PricingRequest:
                    subject = "Febris pricing information";
                    break;
                #endregion
                #region Demo Request
                case EmailType.DemoRequest:
                    subject = "We look forward to showing you what we have";
                    break;
                #endregion
                #region Newsletter Signup
                case EmailType.NewsletterSignup:
                    subject = "We look forward to sending periodic updates";
                    break;
                #endregion
                #region Newsletter removal
                case EmailType.NewsletterRemoval:
                    subject = "We are sorry to see you go";
                    break;
                #endregion
                #region Custom Content Development
                case EmailType.CustomContentDevelopment:
                    subject = "Curriculum Development";
                    break;
                #endregion
                #region Company Application
                case EmailType.CompanyRegistration:
                    subject = "Company Registration";
                    break;
                #endregion
                #region Content Developer Application
                case EmailType.ContentDeveloperApplication:
                    subject = "Content Developer Application";
                    break;
                #endregion
                #region Accreditation body Application
                case EmailType.AccreditationBodyApplication:
                    subject = "Accreditation Body Application";
                    break;
                #endregion
                #endregion

                #region Account
                #region Welcome
                case EmailType.Welcome:
                    subject = "Welcome to Febris";
                    break;
                #endregion
                #region Generic updates
                case EmailType.UserUpdated:
                    subject = "Your account has been updated";
                    break;
                #endregion
                #region Password
                #region Password Reset
                case EmailType.PasswordReset:
                    subject = "Password reset";
                    break;
                #endregion
                #region Forgot password
                case EmailType.ForgotPassword:
                    subject = "Password reset";
                    break;
                #endregion
                #region Password Changed
                case EmailType.PasswordChanged:
                    subject = "Password changed";
                    break;
                #endregion
                #endregion
                #region Email
                #region Email Verifications
                case EmailType.EmailVerification:
                    subject = "Please verify your email address";
                    break;
                #endregion
                #region Email Changed
                case EmailType.EmailAddressChanged:
                    subject = "Email address changed";
                    break;
                #endregion
                #endregion
                #region Pending notifications
                case EmailType.PendingItemNotification:
                    subject = "You have pending notifications";
                    break;
                #endregion
                #region Linking verification
                case EmailType.LinkVerification:
                    subject = "You have pending notifications";
                    break;
                #endregion
                #region New Statement Submission
                case EmailType.StatementSubmission:
                    subject = "New submission";
                    break;
                #endregion
                #endregion

                #region To Admin only
                #region New Purchase
                case EmailType.Purchase:
                    subject = "New purchase";
                    break;
                #endregion
                #endregion

                #region Developer self-signup outcome
                case EmailType.DeveloperApproved:
                    subject = "Your Febris developer account has been approved";
                    break;
                case EmailType.DeveloperRejected:
                    subject = "Update on your Febris developer application";
                    break;
                case EmailType.DeveloperInvite:
                    subject = "You've been invited to join a Febris developer team";
                    break;
                case EmailType.AccountActivation:
                    subject = "Welcome to Febris -- set your password to finish setup";
                    break;
                case EmailType.NodeUserInvite:
                    subject = "You have been invited to create an account";
                    break;
                #endregion

            }


            return subject;
        }

        /// <summary>
        /// Need to set a specific data readout 
        /// </summary>
        /// 
        /// {0} Title
        /// {1} Subtitle
        /// {2} LetterHead
        /// {3} SubLetterhead
        /// {4} UserName
        /// {5} Messagebody
        /// {6} image
        /// {7} Special
        /// {8} Unsubscribe
        /// <returns></returns>
        private string SetMessage(EmailType emailType)
        {
            //var webRoot = _env.WebRootPath; //get wwwroot Folder 
            //var pathToFile = _env.WebRootPath
            //          + Path.DirectorySeparatorChar.ToString()                      
            //          + "EmailTemplates"
            //          + Path.DirectorySeparatorChar.ToString()
            //          + "febrisbasic.html";

            string pathToTemplate = "wwwroot/EmailTemplates/febrisbasic.html";
            //string pathToTemplate = "EmailTemplates/febrisbasic.html";
            string body = string.Empty;
            BodyBuilder builder = new BodyBuilder();

            string title = string.Empty;
            string subTitle = string.Empty;
            string letterHead = string.Empty;
            string subletterHead = string.Empty;
            string userName = string.Empty;
            string messagebody = string.Empty;
            string imagePath = string.Empty;
            string specialInsert = string.Empty;
            string unsubscribeUrl = string.Empty;
            string unsubscribeHyperlink = string.Empty;


            userName = EmailModel.RecipientName;
            unsubscribeUrl = ReturnUrl + EmailModel.RecipientUUID.ToString();
            unsubscribeHyperlink = "<a href=\"" + unsubscribeUrl + "\">Unsubscribe</a>";


            ////add a button creation for use in special insert
            //specialInsert = "<hr/> <button type=\"button\" title=\"Confirm Account Registration\" style=\"background: #64a19d\">" +
            //    "<a href = \"{6}\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
            //    "Click Me " +
            //    "</a></button>";
            //if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
            //{
            //    specialInsert = "<hr/> <button type=\"button\" title=\"Confirm Account Registration\" style=\"background: #64a19d\"><a href = \""+
            //    EmailModel.SpecialHyperlink
            //    +"\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
            //    "Link " +
            //    "</a></button>";
            //}



            switch (emailType)
            {
                #region Marketing                
                #region First Contact
                case EmailType.FirstContact:
                    title = "Great to meet you!";
                    subTitle = " ";
                    letterHead = "Thank you for connecting!";
                    subletterHead = " ";
                    messagebody = "We look forward to working with you in the future! ";
                    //link to demo signup
                    imagePath = BrandingLogoUrl;
                    EmailModel.SpecialHyperlink = BrandingSchedulingUrl;
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        specialInsert = "<hr/> " +
                        "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                        "<tr>" +
                        "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                        "<a href=\"" +
                        EmailModel.SpecialHyperlink +
                        "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                        "Shedule Demo" +
                        "</a>" +
                        "</td>" +
                        "</tr>" +
                        "</table>";

                        //specialInsert = "<hr/> " +
                        //    "<button type=\"button\" title=\"Schedule a Demo\" style=\"background: #64a19d\">" +
                        //    "<a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"Shedule Demo" +
                        //"</a>" +
                        //"</button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #region Question
                case EmailType.Question:
                    title = "Questions and Answers";
                    subTitle = " ";
                    letterHead = "We see you have asked a question";
                    subletterHead = " ";
                    messagebody = "We have received your question and a Febris representative will be in touch shortly.";
                    //link to demo signup
                    imagePath = BrandingLogoUrl;
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        //specialInsert = "<hr/> <button type=\"button\" title=\"Schedule a Demo\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"Shedule Demo" +
                        //"</a></button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #region Demo Request
                case EmailType.DemoRequest:
                    title = "Demonstration signing up";
                    subTitle = " ";
                    letterHead = "If you have not already, please select a time that works for you!";
                    subletterHead = " ";
                    messagebody = "We look forward to telling you about the products Febris builds and how they can help your organization. If there are any aspects of the Febris system that intrest you please let us know so we can focus on those topics.";
                    imagePath = BrandingLogoUrl;
                    //specialInsert = "<hr/> <button type=\"button\" title=\"Confirm Account Registration\" style=\"background: #64a19d\"><a href = \"{6}\" style = \"font-size:22px; padding: 10px; color: #ffffff\">Set up a demo date!</a></button>";
                    EmailModel.SpecialHyperlink = BrandingSchedulingUrl;
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        specialInsert = "<hr/> " +
                   "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                   "<tr>" +
                   "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                   "<a href=\"" +
                   EmailModel.SpecialHyperlink +
                   "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                   "Shedule Demo" +
                   "</a>" +
                   "</td>" +
                   "</tr>" +
                   "</table>";
                        //specialInsert = "<hr/> " +
                        //    "<button type=\"button\" title=\"Pending Items\" style=\"background: #64a19d\">" +
                        //    "<a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"Shedule Demo" +
                        //"</a>" +
                        //"</button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #region Newsletter Signup
                case EmailType.NewsletterSignup:
                    title = "Updates";
                    subTitle = " ";
                    letterHead = "We look forward to keeping you up-to-date with our progress.";
                    subletterHead = " ";
                    messagebody = "You just signed up to receive periodic updates from Febris. We look forward to sending you updates. " +
                        "If you would like to have demonstration, please use the link provided.";
                    imagePath = BrandingLogoUrl;
                    //specialInsert = "<hr/> <button type=\"button\" title=\"Confirm Account Registration\" style=\"background: #64a19d\"><a href = \"{6}\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> Click Me </a></button>";
                    EmailModel.SpecialHyperlink = BrandingSchedulingUrl;
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        specialInsert = "<hr/> " +
                   "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                   "<tr>" +
                   "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                   "<a href=\"" +
                   EmailModel.SpecialHyperlink +
                   "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                   "Shedule Demo" +
                   "</a>" +
                   "</td>" +
                   "</tr>" +
                   "</table>";
                        //specialInsert = "<hr/> <button type=\"button\" title=\"Pending Items\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"Shedule Demo" +
                        //"</a></button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #region Newsletter removal
                case EmailType.NewsletterRemoval:
                    title = "Unsubscribe";
                    subTitle = " ";
                    letterHead = "We are sorry to see you go";
                    subletterHead = " ";
                    messagebody = "You will no longer receive periodic updates from us. If you would ever like to start receiving them again please let us know.";
                    imagePath = BrandingLogoUrl;
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        specialInsert = "<hr/> " +
                   "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                   "<tr>" +
                   "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                   "<a href=\"" +
                   EmailModel.SpecialHyperlink +
                   "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                   "Shedule Demo" +
                   "</a>" +
                   "</td>" +
                   "</tr>" +
                   "</table>";
                        //specialInsert = "<hr/> <button type=\"button\" title=\"Schedule a Demo\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"A Button is here" +
                        //"</a></button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #region Custom Content Development
                case EmailType.CustomContentDevelopment:
                    title = "Content Development";
                    subTitle = "Initial Screening";
                    letterHead = "Let's talk about your plans";
                    subletterHead = " ";
                    messagebody = "We look forward to speaking about your content plans.";
                    imagePath = BrandingLogoUrl;
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        specialInsert = "<hr/> " +
                   "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                   "<tr>" +
                   "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                   "<a href=\"" +
                   EmailModel.SpecialHyperlink +
                   "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                   "Shedule Time" +
                   "</a>" +
                   "</td>" +
                   "</tr>" +
                   "</table>";
                        //specialInsert = "<hr/> <button type=\"button\" title=\"Schedule a Demo\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"Schedule Time" +
                        //"</a></button>";
                    }
                    //specialInsert = "<hr/> <button type=\"button\" title=\"Confirm Account Registration\" style=\"background: #64a19d\"><a href = \"{6}\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> Click Me </a></button>";
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #region Company Application
                case EmailType.CompanyRegistration:
                    title = "Company Registration";
                    subTitle = "Initial Screening";
                    letterHead = "Planning your companies deployment of the Febris system";
                    subletterHead = " ";
                    messagebody = "Febris offers multiple options for deployment and personalization. " +
                        "We look forward to speaking to your team about deployments and your desired specifications. " +
                        "A Febris representative will be in touch with you shortly.";
                    imagePath = BrandingLogoUrl;
                    //specialInsert = "<hr/> <button type=\"button\" title=\"Confirm Account Registration\" style=\"background: #64a19d\"><a href = \"{6}\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> Click Me </a></button>";
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        specialInsert = "<hr/> " +
                   "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                   "<tr>" +
                   "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                   "<a href=\"" +
                   EmailModel.SpecialHyperlink +
                   "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                   "Shedule Time" +
                   "</a>" +
                   "</td>" +
                   "</tr>" +
                   "</table>";
                        //specialInsert = "<hr/> <button type=\"button\" title=\"Schedule a Demo\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"Shedule Time" +
                        //"</a></button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #region Content Developer Application
                case EmailType.ContentDeveloperApplication:
                    title = "Content Developer";
                    subTitle = "Initial Screening";
                    letterHead = "We look forward to providing you the power of the Febris infrastructure";
                    subletterHead = " ";
                    messagebody = "The process of becoming an approved seller and developer is not automatic. " +
                        "Febris manually verifies each developer. " +
                        "A Febris representative will be in touch with you shortly. ";
                    imagePath = BrandingLogoUrl;
                    //specialInsert = "<hr/> <button type=\"button\" title=\"Confirm Account Registration\" style=\"background: #64a19d\"><a href = \"{6}\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> Click Me </a></button>";
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        specialInsert = "<hr/> " +
                   "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                   "<tr>" +
                   "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                   "<a href=\"" +
                   EmailModel.SpecialHyperlink +
                   "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                   "Shedule Time" +
                   "</a>" +
                   "</td>" +
                   "</tr>" +
                   "</table>";
                        //specialInsert = "<hr/> <button type=\"button\" title=\"Schedule a Demo\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"Shedule Time" +
                        //"</a></button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #region Accreditation body Application
                case EmailType.AccreditationBodyApplication:
                    title = "Accreditation Body";
                    subTitle = "Initial Screening";
                    letterHead = "We look forward to your ";
                    subletterHead = " ";
                    messagebody = "The process of becoming an approved accreditation body is not automatic. " +
                        "Febris manually verifies each accreditation body. " +
                        "A Febris representative will be in touch with you shortly. ";
                    imagePath = BrandingLogoUrl;
                    //specialInsert = "<hr/> <button type=\"button\" title=\"Confirm Account Registration\" style=\"background: #64a19d\"><a href = \"{6}\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> Click Me </a></button>";
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        specialInsert = "<hr/> " +
                   "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                   "<tr>" +
                   "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                   "<a href=\"" +
                   EmailModel.SpecialHyperlink +
                   "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                   "Shedule Time" +
                   "</a>" +
                   "</td>" +
                   "</tr>" +
                   "</table>";
                        //specialInsert = "<hr/> <button type=\"button\" title=\"Schedule a Demo\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"Shedule Time" +
                        //"</a></button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #region Pricing info Request
                case EmailType.PricingRequest:
                    title = "Pricing";
                    subTitle = " ";
                    letterHead = "We look forward to telling you about our offerings";
                    subletterHead = " ";
                    messagebody = "Due to the customized nature of our products, pricing is dependent on your requirements. A Febris representative will be in touch with you shortly.";
                    imagePath = BrandingLogoUrl;
                    //specialInsert = "<hr/> <button type=\"button\" title=\"Confirm Account Registration\" style=\"background: #64a19d\"><a href = \"{6}\" style = \"font-size:22px; padding: 10px; color: #ffffff\">Set up a demo date!</a></button>";
                    //EmailModel.SpecialHyperlink = BrandingSchedulingUrl;
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        specialInsert = "<hr/> " +
                   "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                   "<tr>" +
                   "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                   "<a href=\"" +
                   EmailModel.SpecialHyperlink +
                   "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                   "Shedule Demo" +
                   "</a>" +
                   "</td>" +
                   "</tr>" +
                   "</table>";
                        //specialInsert = "<hr/> <button type=\"button\" title=\"Pending Items\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"Shedule Demo" +
                        //"</a></button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #endregion

                #region Account
                #region Welcome
                case EmailType.Welcome:
                    title = "Welcome to Febris";
                    subTitle = "Your new account has been created";
                    letterHead = "New account creation";
                    subletterHead = " ";
                    messagebody = "Now that you have a new account, you will need to reset your user password. Please click \"Forgot Password\" to gain initial access to your new Febris account.";
                    imagePath = BrandingLogoUrl;
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        //specialInsert = "<hr/> <button type=\"button\" title=\"Schedule a Demo\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"Shedule Demo" +
                        //"</a></button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #region Generic updates
                case EmailType.UserUpdated:
                    title = "Account Updated";
                    subTitle = " ";
                    letterHead = "Your account has been updated";
                    subletterHead = " ";
                    messagebody = "Your account has been updated. If you did not expect this action please contact Febris support.";
                    imagePath = BrandingLogoUrl;
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        //specialInsert = "<hr/> <button type=\"button\" title=\"Schedule a Demo\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"Shedule Demo" +
                        //"</a></button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #region Password
                #region Password Reset
                case EmailType.PasswordReset:
                    title = "Password Reset";
                    subTitle = " ";
                    letterHead = "Your password has been reset";
                    subletterHead = " ";
                    messagebody = "If you did not expect your password to be reset please contact Febris support.";
                    imagePath = BrandingLogoUrl;

                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        //specialInsert = "<hr/> <button type=\"button\" title=\"Schedule a Demo\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"Shedule Demo" +
                        //"</a></button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #region Forgot password
                case EmailType.ForgotPassword:
                    title = "Forgotten Password";
                    subTitle = " ";
                    letterHead = "Don't worry,";
                    subletterHead = "we have you covered";
                    messagebody = "Please follow the link bellow to reset your password.";
                    imagePath = BrandingLogoUrl;
                    //specialInsert = "<hr/> <button type=\"button\" title=\"Confirm Account Registration\" style=\"background: #64a19d\"><a href = \"{6}\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> Click Me </a></button>";
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        specialInsert = "<hr/> " +
                   "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                   "<tr>" +
                   "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                   "<a href=\"" +
                   EmailModel.SpecialHyperlink +
                   "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                   "Password Reset" +
                   "</a>" +
                   "</td>" +
                   "</tr>" +
                   "</table>";
                        //specialInsert = "<hr/> <button type=\"button\" title=\"Pending Items\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"See Pending Items" +
                        //"</a></button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #region Password Changed
                case EmailType.PasswordChanged:
                    title = "Password Changed";
                    subTitle = " ";
                    letterHead = "Your password has been changed";
                    subletterHead = " ";
                    messagebody = "Your password has been changed. If you did not expect your password to be reset please contact Febris support.";
                    imagePath = BrandingLogoUrl;
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        //specialInsert = "<hr/> <button type=\"button\" title=\"Schedule a Demo\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"Shedule Demo" +
                        //"</a></button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #endregion
                #region Email
                #region Email Verifications
                case EmailType.EmailVerification:
                    title = "Email Verification";
                    subTitle = " ";
                    letterHead = "Please verify your email address";
                    subletterHead = " ";
                    messagebody = "";
                    imagePath = BrandingLogoUrl;
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        specialInsert = "<hr/> " +
                   "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                   "<tr>" +
                   "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                   "<a href=\"" +
                   EmailModel.SpecialHyperlink +
                   "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                   "Verify Your Email" +
                   "</a>" +
                   "</td>" +
                   "</tr>" +
                   "</table>";
                        //specialInsert = "<hr/> <button type=\"button\" title=\"Email Varification\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"Verify Your Email" +
                        //"</a></button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #region Email Changed
                case EmailType.EmailAddressChanged:
                    title = "Email Address Updated";
                    subTitle = " ";
                    letterHead = "Your email address was changed";
                    subletterHead = " ";
                    messagebody = " Your Email Address was changed. If you did not request this action, please contact Febris support. ";
                    imagePath = BrandingLogoUrl;
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        specialInsert = "<hr/> " +
                   "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                   "<tr>" +
                   "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                   "<a href=\"" +
                   EmailModel.SpecialHyperlink +
                   "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                   "Verify Your Email" +
                   "</a>" +
                   "</td>" +
                   "</tr>" +
                   "</table>";
                        //specialInsert = "<hr/> <button type=\"button\" title=\"Email Varification\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"Verify Your Email" +
                        //"</a></button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #endregion
                #region Pending notifications
                case EmailType.PendingItemNotification:
                    title = "Pending Items";
                    subTitle = " ";
                    letterHead = "You have pending items";
                    subletterHead = " ";
                    messagebody = "There are pending items on your account. Please visit your account to learn more.";
                    imagePath = BrandingLogoUrl;
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        specialInsert = "<hr/> " +
                   "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                   "<tr>" +
                   "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                   "<a href=\"" +
                   EmailModel.SpecialHyperlink +
                   "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                   "See Pending Items" +
                   "</a>" +
                   "</td>" +
                   "</tr>" +
                   "</table>";
                        //specialInsert = "<hr/> <button type=\"button\" title=\"Pending Items\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"See Pending Items" +
                        //"</a></button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #region Linking verification
                case EmailType.LinkVerification:
                    title = "Email Verification";
                    subTitle = " ";
                    letterHead = "Please verify your email address";
                    subletterHead = " ";
                    messagebody = " ";
                    imagePath = BrandingLogoUrl;
                    specialInsert = "<hr/> <button type=\"button\" title=\"Confirm Account Registration\" style=\"background: #64a19d\"><a href = \"{6}\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> Click Me </a></button>";
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        specialInsert = "<hr/> " +
                   "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                   "<tr>" +
                   "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                   "<a href=\"" +
                   EmailModel.SpecialHyperlink +
                   "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                   "See Pending Items" +
                   "</a>" +
                   "</td>" +
                   "</tr>" +
                   "</table>";

                        //specialInsert = "<hr/> <button type=\"button\" title=\"Pending Items\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"See Pending Items" +
                        //"</a></button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #region New Statement Submission
                case EmailType.StatementSubmission:
                    title = "New submisson";
                    subTitle = " ";
                    letterHead = "You have new results submitted!";
                    subletterHead = " ";
                    messagebody = "Check out your latest results submitted to your Febris account!";
                    imagePath = BrandingLogoUrl;
                    //specialInsert = "<hr/> <button type=\"button\" title=\"Confirm Account Registration\" style=\"background: #64a19d\"><a href = \"{6}\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> Click Me </a></button>";
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        specialInsert = "<hr/> " +
                  "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                  "<tr>" +
                  "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                  "<a href=\"" +
                  EmailModel.SpecialHyperlink +
                  "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                  "Go To Site" +
                  "</a>" +
                  "</td>" +
                  "</tr>" +
                  "</table>";
                        //specialInsert = "<hr/> <button type=\"button\" title=\"Pending Items\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"Got To Site" +
                        //"</a></button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #endregion

                #region To Admin only
                #region New Purchase
                case EmailType.Purchase:
                    title = "New Purchase";
                    subTitle = " ";
                    letterHead = "Your account has new purchases.";
                    subletterHead = " ";
                    messagebody = "Please vist your account to see your current purshases";
                    imagePath = BrandingLogoUrl;
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        specialInsert = "<hr/> " +
                  "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                  "<tr>" +
                  "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                  "<a href=\"" +
                  EmailModel.SpecialHyperlink +
                  "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                  "Go To Site" +
                  "</a>" +
                  "</td>" +
                  "</tr>" +
                  "</table>";
                        //specialInsert = "<hr/> <button type=\"button\" title=\"Pending Items\" style=\"background: #64a19d\"><a href = \"" +
                        //EmailModel.SpecialHyperlink
                        //+ "\" style = \"font-size:22px; padding: 10px; color: #ffffff\"> " +
                        //"Go To Site" +
                        //"</a></button>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
                #endregion

                #region Developer self-signup outcome
                // Approval -- sent after a Febris admin clears
                // PendingSelfSignUp=true on the new org. SpecialHyperlink
                // is the SSO login URL so the new admin can sign in.
                case EmailType.DeveloperApproved:
                    title = "Developer Account Approved";
                    subTitle = " ";
                    letterHead = "Welcome to Febris!";
                    subletterHead = " ";
                    messagebody = "Your Febris developer account has been approved. You can now sign in and start setting up your organization.";
                    imagePath = BrandingLogoUrl;
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink))
                    {
                        specialInsert = "<hr/> " +
                  "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                  "<tr>" +
                  "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                  "<a href=\"" +
                  EmailModel.SpecialHyperlink +
                  "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                  "Sign In" +
                  "</a>" +
                  "</td>" +
                  "</tr>" +
                  "</table>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;

                // Rejection -- sent after a Febris admin rejects a pending
                // self-signup. The reason (if any) was captured by the BLL
                // in SignUpRejectionOutcome.Reason; callers put it into
                // EmailModel.Message before send. No CTA button.
                case EmailType.DeveloperRejected:
                    title = "Developer Application Update";
                    subTitle = " ";
                    letterHead = "We were unable to approve your application";
                    subletterHead = " ";
                    messagebody = string.IsNullOrEmpty(EmailModel.Message)
                        ? "After reviewing your application we were unable to approve it at this time. If you believe this was in error, please reach out to the Febris team."
                        : ("After reviewing your application we were unable to approve it at this time. Note from our team: " + EmailModel.Message);
                    imagePath = BrandingLogoUrl;
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;

                // Account activation -- admin-created accounts. The user
                // hasn't set a password yet; this email is the only way
                // they can set their initial password and confirm their
                // email. SpecialHyperlink = SetPassword page URL with
                // userId + reset token; Message = (optional) inviter /
                // org name for personalization.
                case EmailType.AccountActivation:
                    title = "Welcome to Febris";
                    subTitle = " ";
                    letterHead = string.IsNullOrEmpty(EmailModel.Message)
                        ? "An account has been created for you"
                        : ("Your " + EmailModel.Message + " account is ready");
                    subletterHead = " ";
                    messagebody = "Click the button below to set your password and finish setting up your account. The link will expire in 24 hours -- ask the admin who created your account to resend if it lapses.";
                    imagePath = BrandingLogoUrl;
                    // FIX (SCBA-B4): only build the href when SpecialHyperlink is an absolute http/https URL. The set-password link is always meant to be http(s), so this leaves real links untouched while blocking javascript:/data: injection into the anchor.
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink)
                        && Uri.TryCreate(EmailModel.SpecialHyperlink, UriKind.Absolute, out Uri activationLinkUri)
                        && (activationLinkUri.Scheme == Uri.UriSchemeHttp || activationLinkUri.Scheme == Uri.UriSchemeHttps))
                    {
                        specialInsert = "<hr/> " +
                  "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                  "<tr>" +
                  "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                  "<a href=\"" +
                  EmailModel.SpecialHyperlink +
                  "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                  "Set Your Password" +
                  "</a>" +
                  "</td>" +
                  "</tr>" +
                  "</table>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;

                // Invite to an existing org. SpecialHyperlink = accept-link
                // carrying the token; Message field = inviter's name (set
                // by the BLL before send) so the email reads "X invited
                // you to join Y on Febris".
                case EmailType.DeveloperInvite:
                    title = "Team invitation";
                    subTitle = " ";
                    letterHead = string.IsNullOrEmpty(EmailModel.Message)
                        ? "You've been invited to join a Febris developer team"
                        : (EmailModel.Message + " invited you to join their Febris developer team");
                    subletterHead = " ";
                    messagebody = "Click the button below to accept the invitation and create your account. The link expires after seven days.";
                    imagePath = BrandingLogoUrl;
                    // FIX (SCBA-B4): only build the href when SpecialHyperlink is an absolute http/https URL. The accept-invite link is always meant to be http(s), so this leaves real links untouched while blocking javascript:/data: injection into the anchor.
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink)
                        && Uri.TryCreate(EmailModel.SpecialHyperlink, UriKind.Absolute, out Uri inviteLinkUri)
                        && (inviteLinkUri.Scheme == Uri.UriSchemeHttp || inviteLinkUri.Scheme == Uri.UriSchemeHttps))
                    {
                        specialInsert = "<hr/> " +
                  "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                  "<tr>" +
                  "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                  "<a href=\"" +
                  EmailModel.SpecialHyperlink +
                  "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                  "Accept Invitation" +
                  "</a>" +
                  "</td>" +
                  "</tr>" +
                  "</table>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;

                // User-node invitation. SpecialHyperlink = the accept-link carrying the token.
                //
                // Says NOTHING about Febris teams, developers, marketplaces or any other central
                // concept: the recipient is a learner or teacher at whoever runs this node, and has
                // no relationship with any of that. That is why this does not reuse DeveloperInvite
                // despite the near-identical mechanics.
                //
                // Reads correctly with Message NULL, which is the only shape it can arrive in: the
                // IEmailSender adapter (SendEmailAsync) sets SpecialHyperlink and leaves Message
                // unset, so a letterhead that depended on an inviter name would render blank.
                case EmailType.NodeUserInvite:
                    title = "You have been invited";
                    subTitle = " ";
                    letterHead = string.IsNullOrEmpty(EmailModel.Message)
                        ? "An account is waiting to be set up for you"
                        : (EmailModel.Message + " has invited you to create an account");
                    subletterHead = " ";
                    messagebody = "Click the button below to choose a password and finish creating your account. "
                        + "You will be asked to confirm the email address this invitation was sent to. "
                        + "The link can be used once, and expires. If it has lapsed, ask the person who invited you to send a new one. "
                        + "If you were not expecting this, you can ignore it and no account will be created.";
                    imagePath = BrandingLogoUrl;
                    // Same SCBA-B4 guard as the two links above: build the href ONLY for an absolute
                    // http/https URL, so javascript:/data: cannot be injected into the anchor.
                    if (!string.IsNullOrEmpty(EmailModel.SpecialHyperlink)
                        && Uri.TryCreate(EmailModel.SpecialHyperlink, UriKind.Absolute, out Uri nodeInviteLinkUri)
                        && (nodeInviteLinkUri.Scheme == Uri.UriSchemeHttp || nodeInviteLinkUri.Scheme == Uri.UriSchemeHttps))
                    {
                        specialInsert = "<hr/> " +
                  "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: auto;\">" +
                  "<tr>" +
                  "<td style=\"font-family: sans-serif; font-size: 14px; vertical-align: top; background-color: #64a19d; border-radius: 0px; text-align: center;\" valign=\"top\" bgcolor=\"#3498db\" align=\"center\">" +
                  "<a href=\"" +
                  EmailModel.SpecialHyperlink +
                  "\" target=\"_blank\" style=\"display: inline-block; color: #ffffff; background-color: #64a19d; border: solid 1px #64a19d; border-radius: 5px; box-sizing: border-box; cursor: pointer; text-decoration: none; font-size: 14px; font-weight: bold; margin: 0; padding: 10px 25px; text-transform: capitalize; border-color: #64a19d;\">" +
                  "Create Your Account" +
                  "</a>" +
                  "</td>" +
                  "</tr>" +
                  "</table>";
                    }
                    using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                    {
                        builder.HtmlBody = SourceReader.ReadToEnd();
                    }
                    break;
                #endregion
            }


            //maybe this should be moved
            // FIX (SCBA-B4): HTML-encode userName before it lands in the {4} placeholder. RecipientName is user-controlled plain text with no intended markup, so encoding closes an HTML/XSS injection without changing rendered output for real names.
            // NOTE (SCBA-B4): messagebody and specialInsert intentionally carry HTML built by this method (tables, hr, anchor button). HtmlEncode on them would emit literal tags and break the rendered email, a functionality/output change. Deferred per do-not-change-functionality.
            body = string.Format(builder.HtmlBody,
                title,
                subTitle,
                letterHead,
                subletterHead,
                System.Net.WebUtility.HtmlEncode(userName),
                messagebody,
                imagePath,
                specialInsert,
                unsubscribeHyperlink
                );



            return body;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            bool sent = false;
            try
            {
                EmailType inputEmailType = (EmailType)Enum.Parse(typeof(EmailType), subject);

                EmailService emailService = new EmailService(_config)
                {
                    EmailType = inputEmailType,
                    EmailModel = new EmailModel()
                    {
                        RecipientName = email,
                        RecipientEmailAddress = email,
                        //Subject = subject,
                        SpecialHyperlink = htmlMessage,
                        //Message = htmlMessage
                    }
                };
                sent = emailService.SendEmail().Result;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
            return Task.CompletedTask;
        }

        public Task SendCampaignEmailAsync(string email, string subject, string htmlBody)
        {
            bool sent = false;
            try
            {

                //can make the subject input work
                //EmailType inputEmailType = (EmailType)Enum.Parse(typeof(EmailType), subject);

                EmailService emailService = new EmailService(_config)
                {
                    EmailType = EmailType.CampaignMessage,
                    EmailModel = new EmailModel()
                    {
                        RecipientName = email,
                        RecipientEmailAddress = email,
                        Message = htmlBody,
                        Subject = subject,
                        //SpecialHyperlink = unsubscribeLink,
                    }
                };
                sent = emailService.SendEmail(subject, htmlBody).Result;
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
            return Task.CompletedTask;
        }
    }

    public class EmailEngine
    {
        private IConfiguration _config;
        public EmailEngine(IConfiguration config)
        {
            _config = config;
            ImageURL = _config.GetValue<string>("ApiPaths:MarketingAPI");
            ImageURL = ImageURL + "Widget/EmailCampaignImageLoader?Path=";
        }
        public const string HyperlinkFormat = "<a href=\"{0}\">{1}</a>";
        public const string ImageFormat = "";
        /// <summary>
        /// These table cells will need to be updated to route to proper image loaders in marketing API
        /// 
        /// 0-image
        /// 1-Title
        /// 2-Body
        /// 3-Hyperlink
        /// </summary>
        public const string TableCellLeft_Image = "<tr>" +
                                                 "<td align=\"left\" valign=\"middle\" style=\"padding:10px;\" width=\"50%\">" +
                                                     "<img src = \"{0}\" style=\"display:block\" width=\"100%\">" +
                                                     "<p style=\"color:rgb(0,0,0,0.5)\"><i>{1}</i></p>" +
                                                 "</td>" +
                                                 "<td width = \"50%\" align=\"left\" valign=\"middle\" style=\"color:#525252; font-family:Arial, Helvetica, sans-serif; padding:10px;\">" +
                                                     "<div style = \"font-size:16px;\">" +
                                                         "<b>{2}</b>" +
                                                     "</div>" +
                                                     "<div style=\"font-size:12px;\">" +
                                                         "<b>{3}</b>{4}" +
                                                     "</div>" +
                                                 "</td>" +
                                             "</tr>";
        #region from partial view
        //public const string TableCellLeft_Image = "<tr>" +
        //                                            "<td align=\"left\" valign=\"middle\" style=\"padding:10px;\" width=\"50%\">" +
        //                                                "<img src = \"@Url.Action(\"CampaignEmailMessageImageLoader\",\"Widget\",new {path={0}})\" style=\"display:block\" width=\"100%\">" +
        //                                            "</td>" +
        //                                            "<td width = \"50%\" align=\"left\" valign=\"middle\" style=\"color:#525252; font-family:Arial, Helvetica, sans-serif; padding:10px;\">" +
        //                                                "<div style = \"font-size:16px;\">" +
        //                                                    "<b>{1}</b>" +
        //                                                "</div>" +
        //                                                "<div style=\"font-size:12px;\">" +
        //                                                    "<b>{2}</b>{3}" +
        //                                                "</div>" +
        //                                            "</td>" +
        //                                        "</tr>";
        #endregion
        /// <summary>
        /// 0- Title
        /// 1- Body
        /// 2- hyperlink
        /// 3- image
        /// </summary>
        public const string TableCellRight_Image = "<tr>" +
                                                        "<td width=\"50%\" align=\"left\" valign=\"middle\" style=\"color:#525252; font-family:Arial, Helvetica, sans-serif; padding:10px;\">" +
                                                            "<div style = \"font-size:16px;\" >" +
                                                                "<b> {0} </b>" +
                                                            "</div>" +
                                                            "<div style=\"font-size:12px;\">" +
                                                                "<b>{1}</b>{2}" +
                                                            "</div>" +
                                                        "</td>" +
                                                        "<td align = \"left\" valign=\"middle\" style=\"padding:10px;\" width=\"40%\">" +
                                                            "<img src = \"{3}\" style=\"display:block\" width=\"100%\">" +
                                                            "<p style=\"color:rgb(0,0,0,0.5)\"><i>{4}</i></p>" +
                                                        "</td>" +
                                                    "</tr>";
        #region from partial view
        //public const string TableCellRight_Image = "<tr>" +
        //                                                "<td width=\"50%\" align=\"left\" valign=\"middle\" style=\"color:#525252; font-family:Arial, Helvetica, sans-serif; padding:10px;\">" +
        //                                                    "<div style = \"font-size:16px;\" >" +
        //                                                        "<b> {0} </b>" +
        //                                                    "</div>" +
        //                                                    "<div style=\"font-size:12px;\">" +
        //                                                        "<b>{1}</b>{2}"+
        //                                                    "</div>"+
        //                                                "</td>"+
        //                                                "<td align = \"left\" valign=\"middle\" style=\"padding:10px;\" width=\"40%\">"+
        //                                                    "<img src = \"@Url.Action(\"CampaignEmailMessageImageLoader\",\"Widget\",new {path={3}})\" style=\"display:block\" width=\"100%\">"+
        //                                                "</td>"+
        //                                            "</tr>";
        #endregion
        /// <summary>
        /// 0-Title
        /// 1-Body
        /// 2-Hyperlink
        /// </summary>
        public const string TableCell_NoImage = "<tr>" +
                                            "<td width=\"100%\" align=\"left\" valign=\"middle\" colspan=\"2\" style=\"color:#525252; font-family:Arial, Helvetica, sans-serif; padding:10px;\">" +
                                                "<div style = \"font-size:16px;\" >" +
                                                    "<b>{0}</b>" +
                                                "</div>" +
                                                "<div style=\"font-size:12px;\">" +
                                                     "<b>{1}</b>{2}" +
                                                "</div>" +
                                                "</td>" +
                                            "</tr>";
        #region from partial view
        //public const string TableCell_NoImage = "<tr>" +
        //                                    "<td width=\"100%\" align=\"left\" valign=\"middle\" colspan=\"2\" style=\"color:#525252; font-family:Arial, Helvetica, sans-serif; padding:10px;\">" +
        //                                        "<div style = \"font-size:16px;\" >"+
        //                                            "<b>{0}</b>"+
        //                                        "</div>"+
        //                                        "<div style=\"font-size:12px;\">"+
        //                                             "<b>{1}</b>{2}" +
        //                                        "</div>"+
        //                                        "</td>"+
        //                                    "</tr>";
        #endregion
        //public string UnsubscribeHyperLink = "<a href=\"" + unsubscribeUrl + "\">Unsubscribe</a>";
        public const string UnsubscribeHyperLink = "<a href=\"{0}\">Unsubscribe</a>";
        public const string pathToTemplate = "wwwroot/EmailTemplates/campaigntemplate.html";
        public const string HeaderImage = "<img src =\"{0}\" style=\"display:block\" width=\"100%\">";
        public const string HeaderImageCaption = "<p style=\"color:rgb(0,0,0,0.5)\"><i>{0}</i></p>";
        //public const string ImageCaption = "<p><i>{0}c</i></p>";
        //public const string HeaderImage = "<img src =\"@Url.Action(\" CampaignEmailMessageImageLoader\",\"Widget\",new {path={0}})\" style=\"display:block\" width=\"100%\">";
        public static string ImageURL = string.Empty;


        public async Task<string> EmailBuilder(FullEmailBuilderViewModel input)
        {
            string output = string.Empty;
            string body = string.Empty;

            try
            {

                //Breakdown each section
                for (var i = 0; input.EmailSectionViewModelList.Count() > i; i++)
                {
                    string tempBody = string.Empty;
                    string tempHyperlink = string.Empty;
                    if (!input.EmailSectionViewModelList[i].IncludeImage)
                    {
                        //tempBody = TableCell_NoImage;
                        if (!string.IsNullOrEmpty(input.EmailSectionViewModelList[i].Hyperlink))
                        {
                            tempHyperlink = HyperlinkFormat;
                            tempHyperlink = string.Format(tempHyperlink, input.EmailSectionViewModelList[i].Hyperlink, "Read More...");
                            tempBody = string.Format(TableCell_NoImage, input.EmailSectionViewModelList[i].Title, input.EmailSectionViewModelList[i].Body, tempHyperlink);
                        }
                        else
                        {
                            //tempHyperlink=string.Format(tempHyperlink, string.Empty, string.Empty);
                            tempBody = string.Format(TableCell_NoImage, input.EmailSectionViewModelList[i].Title, input.EmailSectionViewModelList[i].Body, string.Empty);
                        }
                        body += tempBody;
                    }
                    else if (i % 2 == 0)
                    {
                        //tempBody = TableCellRight_Image;                        
                        //tempBody = tempBody.Replace("{0}", input.EmailSectionViewModelList[i].Title);
                        //tempBody = tempBody.Replace("{1}", input.EmailSectionViewModelList[i].Body);

                        //No idea why but the string.Format does not work with with setup. So I am having to use a workaround.
                        tempBody = tempBody = TableCellRight_Image;
                        input.EmailSectionViewModelList[i].ImagePath = ImageURL + input.EmailSectionViewModelList[i].ImagePath;
                        tempBody = tempBody.Replace("{3}", input.EmailSectionViewModelList[i].ImagePath);
                        tempBody = tempBody.Replace("{0}", input.EmailSectionViewModelList[i].Title);
                        tempBody = tempBody.Replace("{1}", input.EmailSectionViewModelList[i].Body);

                        if (!string.IsNullOrEmpty(input.EmailSectionViewModelList[i].Hyperlink))
                        {
                            tempHyperlink = HyperlinkFormat;
                            tempHyperlink = string.Format(tempHyperlink, input.EmailSectionViewModelList[i].Hyperlink, "Read More...");
                            tempBody = tempBody.Replace("{2}", tempHyperlink);


                            //tempBody = string.Format(TableCellRight_Image, input.EmailSectionViewModelList[i].Title, input.EmailSectionViewModelList[i].Body, tempHyperlink, input.EmailSectionViewModelList[i].ImagePath);


                            //tempHyperlink.Replace("{0}", input.EmailSectionViewModelList[i].Hyperlink);
                            //tempHyperlink.Replace("{1}", "Read More...");
                            //tempBody = tempBody.Replace("{2}", tempHyperlink);
                        }
                        else
                        {
                            tempBody = tempBody.Replace("{2}", string.Empty);


                            //tempBody = tempBody.Replace("{2}", "");
                            //tempBody = string.Format(TableCellRight_Image, input.EmailSectionViewModelList[i].Title, input.EmailSectionViewModelList[i].Body, string.Empty, input.EmailSectionViewModelList[i].ImagePath);
                        }
                        if (!string.IsNullOrEmpty(input.EmailSectionViewModelList[i].ImageCaption))
                        {
                            tempBody = tempBody.Replace("{4}", input.EmailSectionViewModelList[i].ImageCaption);
                        }
                        else
                        {
                            tempBody = tempBody.Replace("{4}", string.Empty);
                        }
                        body += tempBody;
                    }
                    else
                    {
                        //tempBody = TableCellLeft_Image;
                        //tempBody = tempBody.Replace("{0}", input.EmailSectionViewModelList[i].Title);
                        //tempBody = tempBody.Replace("{1}", input.EmailSectionViewModelList[i].Body);
                        //if (!string.IsNullOrEmpty(input.EmailSectionViewModelList[i].Hyperlink))
                        //{
                        //    tempHyperlink = HyperlinkFormat;
                        //    tempHyperlink.Replace("{0}", input.EmailSectionViewModelList[i].Hyperlink);
                        //    tempHyperlink.Replace("{1}", "Read More...");
                        //    tempBody = tempBody.Replace("{2}", tempHyperlink);
                        //}
                        //else
                        //{
                        //    tempBody = tempBody.Replace("{2}", "");
                        //}

                        //tempBody = tempBody.Replace("{3}", input.EmailSectionViewModelList[i].Body);
                        // body += tempBody;

                        //No idea why but the string.Format does not work with with setup. So I am having to use a workaround.
                        tempBody = TableCellLeft_Image;
                        string updatedHeaderPath = ImageURL + input.EmailSectionViewModelList[i].ImagePath;
                        tempBody = tempBody.Replace("{0}", updatedHeaderPath);
                        tempBody = tempBody.Replace("{2}", input.EmailSectionViewModelList[i].Title);
                        tempBody = tempBody.Replace("{3}", input.EmailSectionViewModelList[i].Body);

                        if (!string.IsNullOrEmpty(input.EmailSectionViewModelList[i].Hyperlink))
                        {

                            tempHyperlink = HyperlinkFormat;
                            tempHyperlink = string.Format(tempHyperlink, input.EmailSectionViewModelList[i].Hyperlink, "Read More...");
                            tempBody = tempBody.Replace("{4}", tempHyperlink);
                            //tempBody = string.Format(TableCellLeft_Image, input.EmailSectionViewModelList[i].ImagePath, input.EmailSectionViewModelList[i].Title, input.EmailSectionViewModelList[i].Body, tempHyperlink);                         
                        }
                        else
                        {
                            tempBody = tempBody.Replace("{3}", string.Empty);
                            //tempBody = string.Format(TableCellLeft_Image, input.EmailSectionViewModelList[i].ImagePath, input.EmailSectionViewModelList[i].Title, input.EmailSectionViewModelList[i].Body, string.Empty);
                        }
                        if (!string.IsNullOrEmpty(input.EmailSectionViewModelList[i].ImageCaption))
                        {
                            tempBody = tempBody.Replace("{1}", input.EmailSectionViewModelList[i].ImageCaption);
                        }
                        else
                        {
                            tempBody = tempBody.Replace("{1}", string.Empty);
                        }
                        body += tempBody;
                    }

                }


                //Get Email template
                BodyBuilder builder = new BodyBuilder();
                using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
                {
                    builder.HtmlBody = SourceReader.ReadToEnd();
                }

                //input.EmailCampaignMessage.HeaderImage = ImageURL + input.EmailCampaignMessage.HeaderImage;
                string modifiedHeaderImagePath = ImageURL + input.EmailCampaignMessage.HeaderImage;
                string headerImage = string.Format(HeaderImage, modifiedHeaderImagePath);
                string headerImageCaption = string.Format(HeaderImageCaption, input.EmailCampaignMessage.HeaderImageCaption);



                //put together 
                output = string.Format(builder.HtmlBody,
                    input.EmailCampaignMessage.Subject,
                    input.EmailCampaignMessage.Subject,
                    headerImage,
                    headerImageCaption,
                    body,
                    UnsubscribeHyperLink
                    );
            }
            catch (Exception ex)
            {
                Febris.SharedServices.FebrisLog.Error(ex);
                Febris.SharedServices.FebrisLog.Error(ex);
                throw;
            }
            return output;
        }

        //public async Task<string> AddUnsubscribeHyperlink(string body, Lead input)
        //{
        //    string output = string.Empty;
        //    try
        //    {
        //        //Get Email template
        //        BodyBuilder builder = new BodyBuilder();
        //        using (StreamReader SourceReader = System.IO.File.OpenText(pathToTemplate))
        //        {
        //            builder.HtmlBody = SourceReader.ReadToEnd();
        //        }

        //        //put together                 
        //        output=string.Format(body, input.UUID.ToString());
        //    }
        //    catch (Exception ex)
        //    {
        //        Febris.SharedServices.FebrisLog.Error(ex);
        //        throw;
        //    }
        //    return output;
        //}

    }
}
