namespace Blog.Services.MailService
{
    public interface ISendMailService
    {
        Task SendEmailAsync(MailContent mailContent);
    }
}
