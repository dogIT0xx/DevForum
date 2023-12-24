using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;


namespace Blog.Services.MailService
{
    public class SendMailService : ISendMailService
    {
        private readonly MailSetting _mailSetting;
        private readonly ILogger<SendMailService> _logger;

        public SendMailService(IOptions<MailSetting> options, ILogger<SendMailService> logger)
        {
            _mailSetting = options.Value;
            _logger = logger;
            _logger.LogInformation("Create SendMailService");
        }

        public async Task SendEmailAsync(MailContent mailContent)
        {
            var email = new MimeMessage();
            email.Sender = new MailboxAddress(_mailSetting.DisplayName, _mailSetting.Mail);
            email.From.Add(new MailboxAddress(_mailSetting.DisplayName, _mailSetting.Mail));
            email.To.Add(MailboxAddress.Parse(mailContent.To));
            email.Subject = mailContent.Subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = mailContent.Body;
            email.Body = builder.ToMessageBody();

            using (var smtp = new SmtpClient())
            {
                try
                {
                    smtp.Connect(_mailSetting.Host, _mailSetting.Port, SecureSocketOptions.StartTls);
                    smtp.Authenticate(_mailSetting.Mail, _mailSetting.Password);
                    await smtp.SendAsync(email);
                }
                catch (Exception ex)
                {
                    string path = "Services/MailService/ErrorMailSave";
                    Directory.CreateDirectory(path);
                    var emailSaveFile = string.Format(@"{0}/{1}.eml", path, Guid.NewGuid());
                    await email.WriteToAsync(emailSaveFile);

                    _logger.LogInformation("Lỗi gửi mail, lưu tại - " + emailSaveFile);
                    _logger.LogInformation(ex.Message);
                }
                _logger.LogInformation("send mail to " + mailContent.To);
            }
        }
    }
}
