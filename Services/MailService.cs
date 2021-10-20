using DisneyAPI.Interfaces;
using DisneyAPI.ViewModel.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace DisneyAPI.Services
{
    public class MailService : IMailService
    {
        private readonly ISendGridClient _sendGridClient;

        public MailService(ISendGridClient sendGridClient)
        {
            _sendGridClient = sendGridClient;
        }

        public async Task SendEmail(MailServiceResVM sendMail)
        {
            try
            {
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress("ignacio_pancaldo@live.com", sendMail.Title),
                    Subject = sendMail.Title
                };
                msg.AddContent(MimeType.Text, sendMail.Body);
                msg.AddTo(new EmailAddress(sendMail.Email, sendMail.Username));
                var response = await _sendGridClient.SendEmailAsync(msg).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}

