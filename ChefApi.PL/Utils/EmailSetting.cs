﻿using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity;

namespace ChefApi.PL.Utils
{
    public class EmailSetting : IEmailSender
    {
        public  Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("noqta.technical@gmail.com", "tnqd zlvb vmnm tqbx")
            };

            return  client.SendMailAsync(
                new MailMessage(from: "noqta.technical@gmail.com",
                                to: email,
                                subject,
                                htmlMessage
                                )
                { IsBodyHtml = true });
        
    }
    }
}
