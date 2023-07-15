namespace IdaWebApplicationTemplate.Services
{
    public interface IEmailService
    {
        public Task<bool> SendEmailAsync(string title, string content, string recipient);
    }
}