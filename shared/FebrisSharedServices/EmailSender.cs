// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: Apache-2.0
using Febris.ModelLibrary.Models.EmailModels;
using Febris.EnumLibrary;
//using Febris.ModelLibrary.Models.DataModels;
//using Febris.ModelLibrary.Models.UserModels;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity.UI.Services;
//using Microsoft.Extensions.Configuration;
//using MimeKit;
//using System;
//using System.Collections.Generic;
//using MailKit.Net.Smtp;
//using System.Text;
//using System.Threading.Tasks;
//using System.Net.Mail;
//using Microsoft.AspNetCore.Identity;

//namespace Febris.SharedServices
//{
//    //***********************************************************************************************************************************
//    //May need the Iconfiguraation
//    //***********************************************************************************************************************************       
//    //public class EmailProperties
//    //{
//    //    public string Host { get; set; }
//    //    public int Port { get; set; }
//    //    public bool EnableSSL { get; set; }
//    //    public string Sender { get; set; }
//    //    public string Password { get; set; }
//    //    public string SenderName { get; set; }
//    //}
//    //public class EmailModel
//    //{
//    //    public string ToEmail { get; set; }
//    //    public string Subject { get; set; }
//    //    public string Body { get; set; }
//    //    public List<IFormFile> Attachments { get; set; }
//    //}
//    //public enum EmailType
//    //{
//    //    EmailVerification,
//    //    LinkVerification,
//    //    StatementSubmission,
//    //}


//    public class EmailSender : IEmailSender //,ISmsSender
//    {       
//        private IConfiguration _configuration;
//        private string Host;
//        private int Port;
//        private bool EnableSSL;
//        private string Sender;
//        private string Password;
//        private string SenderName;

//        public EmailSender(IConfiguration iConfiguration)
//        {
//            _configuration = iConfiguration;
//        }                
//        public EmailSender(string host, int port, bool enableSSL, string sender, string password, string senderName)
//        {
//            this.Host = host;
//            this.Port = port;
//            this.EnableSSL = enableSSL;
//            this.Sender = sender;
//            this.Password = password;
//            this.SenderName = senderName;
//        }       
//        public EmailSender()
//        {

//        }
                
//        public Task SendEmailAsync(string email, string subject, string htmlMessage)
//        {
//            //may have to put code in here?
//            try
//            {
//                var emailStatus = SendEmail(email, subject, htmlMessage);
//                //no idea if this will work properly
//                return Task.CompletedTask;
//            }
//            catch
//            {
//                throw new NotImplementedException();
//            }

//        }
                
//        public string SendEmail(IConfiguration config, string emailReceiver, string subject, string htmlMessage)
//        {
//            string Host = config.GetValue<string>("EmailSender:Host");
//            int Port = config.GetValue<int>("EmailSender:Port");
//            bool EnableSSL = config.GetValue<bool>("EmailSender:EnableSSL");
//            string Sender = config.GetValue<string>("EmailSender:Sender");
//            string Password = config.GetValue<string>("EmailSender:Password");
//            string SenderName = config.GetValue<string>("EmailSender:SenderName");

//            try
//            {
//                MimeMessage message = new MimeMessage();

//                MailboxAddress from = new MailboxAddress(SenderName, Sender);
//                message.From.Add(from);

//                MailboxAddress to = new MailboxAddress(emailReceiver, emailReceiver);
//                message.To.Add(to);

//                message.Subject = subject;

//                message.Body = new TextPart("html")
//                {
//                    Text = htmlMessage
//                };

//                using (var client = new MailKit.Net.Smtp.SmtpClient())
//                {
//                    //// For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
//                    //client.ServerCertificateValidationCallback = (s, c, h, e) => true;

//                    client.Connect(Host, Port, EnableSSL);//MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable);

//                    // Since we don't have an OAuth2 token, disable the XOAUTH2 authentication mechanism.
//                    //client.AuthenticationMechanisms.Remove("XOAUTH2");

//                    // Note: only needed if the SMTP server requires authentication
//                    client.Authenticate(Sender, Password);

//                    client.Send(message);
//                    client.Disconnect(true);
//                }
//                return "success";
//                //return client.SendMailAsync(new MailMessage(sender, email, subject, htmlMessage)
//                //{ IsBodyHtml = true });
//            }
//            catch (Exception ex)
//            {
//                //throw new InvalidOperationException(ex.Message);
//                return "failed";
//            }
//        }
        
//        public string SendEmail(string emailReceiver, string subject, string htmlMessage)
//        {

//            try
//            {
//                MimeMessage message = new MimeMessage();

//                MailboxAddress from = new MailboxAddress(SenderName, Sender);
//                message.From.Add(from);

//                MailboxAddress to = new MailboxAddress(emailReceiver, emailReceiver);
//                message.To.Add(to);

//                message.Subject = subject;

//                message.Body = new TextPart("html")
//                {
//                    Text = htmlMessage
//                };

//                using (var client = new MailKit.Net.Smtp.SmtpClient())
//                {
//                    //// For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
//                    //client.ServerCertificateValidationCallback = (s, c, h, e) => true;

//                    client.Connect(Host, Port, EnableSSL);//MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable);

//                    // Since we don't have an OAuth2 token, disable the XOAUTH2 authentication mechanism.
//                    //client.AuthenticationMechanisms.Remove("XOAUTH2");

//                    // Note: only needed if the SMTP server requires authentication
//                    client.Authenticate(Sender, Password);

//                    client.Send(message);
//                    client.Disconnect(true);
//                }
//                return "success";
//                //return client.SendMailAsync(new MailMessage(sender, email, subject, htmlMessage)
//                //{ IsBodyHtml = true });
//            }
//            catch (Exception ex)
//            {
//                //throw new InvalidOperationException(ex.Message);
//                return "failed";
//            }
//        }                
//    }  
        
//    public class EmailHandler
//    {       
//        private IConfiguration _config;
//        private EmailSender _emailSender;
//        private readonly UserManager<ApplicationUser> _userManager;

//        public EmailHandler(IConfiguration config)
//        {
//            _config = config;            
//            _emailSender = new EmailSender(_config);
//        }

//        public EmailHandler(IConfiguration config, UserManager<ApplicationUser> userManager) : this(config)
//        {
//            _config = config;
//            _emailSender = new EmailSender(_config);
//            _userManager = userManager;

//        }

//        public void SendEmail(ProfessionalSettings input, EmailType emailType)
//        {
//            //pull email address
//            EmailModel email = new EmailModel
//            {
//                RecipientEmailAddress = input.EmailAddress
//            };


//            switch (emailType)
//            {
//                case EmailType.EmailVerification:
//                    break;
//                case EmailType.LinkVerification:
//                    PendingLinkVerification(input.EmailAddress);
//                    break;
//                case EmailType.StatementSubmission:

//                    break;
//            }
//        }

//        public void SendEmail(ApplicationUser input, EmailType emailType)
//        {
//            switch (emailType)
//            {
//                case EmailType.EmailVerification:

//                    break;
//                case EmailType.LinkVerification:
//                    PendingLinkVerification(input.UserName);
//                    break;
//                case EmailType.StatementSubmission:

//                    break;
//            }
//        }
        
//        private async Task PendingLinkVerification(string emailAddress)
//        {
//            var email = emailAddress;
//            string subject = "You have pending requests";
//            string htmlMessage = "An Institution or Location has requested to add you to their system. " +
//                "When added to their system they will be able to see your past educational records conducted on the Febris system." +
//                "Please go to your dashboard and either accept or reject their request.";
//            var emailStatus = _emailSender.SendEmail(_config,
//                email,
//                subject,
//                htmlMessage);

//            if (emailStatus != "success")
//            {
//                //enter into log
//            }
//        }

//        private async Task StatementVerification(string emailAddress)
//        {
//            throw new NotImplementedException();
//            //var email = emailAddress;
//            //string subject = "You have pending requests";
//            //string htmlMessage = "A Provider or Location has requested to add you to their system. " +
//            //    "When added to their system they will be able to see your past educational records conducted on the Febris system." +
//            //    "Please go to your dashboard and either accept or reject their request.";
//            //var emailStatus = _emailSender.SendEmail(_config,
//            //    email,
//            //    subject,
//            //    htmlMessage);

//            //if (emailStatus != "success")
//            //{
//            //    //enter into log
//            //}
//        }

//    }
//}

