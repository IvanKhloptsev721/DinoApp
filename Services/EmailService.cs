// using MimeKit;
// using MailKit.Net.Smtp;
// using MailKit.Security;
// using Microsoft.Extensions.Options;

namespace DinoApp.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task SendVerificationCodeAsync(string toEmail, string code);
}

// Временно отключаем EmailService
public class EmailService : IEmailService
{
    public Task SendEmailAsync(string toEmail, string subject, string body)
    {
        // Временно отключено
        Console.WriteLine($"[ОТЛАДКА] Email должен быть отправлен на: {toEmail}");
        Console.WriteLine($"Тема: {subject}");
        Console.WriteLine($"Тело: {body}");
        return Task.CompletedTask;
    }

    public Task SendVerificationCodeAsync(string toEmail, string code)
    {
        // Временно отключено
        Console.WriteLine($"[ОТЛАДКА] Код верификации для {toEmail}: {code}");
        return Task.CompletedTask;
    }
}

// Оригинальный EmailService закомментирован оставлен для лучших времён
/*
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };
            email.Body = builder.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation($"Email sent to {toEmail}: {subject}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to {toEmail}");
            throw;
        }
    }

    public async Task SendVerificationCodeAsync(string toEmail, string code)
    {
        var subject = "DinoMir - Код подтверждения";
        var body = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <div style='background: linear-gradient(135deg, #2c5a1e, #1e3a2f); padding: 30px; text-align: center; color: white; border-radius: 10px 10px 0 0;'>
                <h1 style='margin: 0;'>🦕 DinoMir</h1>
                <p style='margin: 10px 0 0; opacity: 0.9;'>Код подтверждения входа</p>
            </div>
            <div style='background: white; padding: 40px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 12px rgba(0,0,0,0.1);'>
                <p>Здравствуйте!</p>
                <p>Вы запросили вход в аккаунт DinoMir. Используйте следующий код для подтверждения:</p>
                <div style='background: #f5f5f5; padding: 20px; text-align: center; border-radius: 10px; margin: 30px 0;'>
                    <span style='font-size: 36px; font-weight: bold; letter-spacing: 10px; color: #2c5a1e;'>{code}</span>
                </div>
                <p style='color: #666; font-size: 14px;'>Код действителен в течение 5 минут. Если вы не запрашивали вход, просто проигнорируйте это письмо.</p>
                <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                <p style='color: #999; font-size: 12px;'>Это автоматическое сообщение, пожалуйста, не отвечайте на него.</p>
            </div>
        </div>";

        await SendEmailAsync(toEmail, subject, body);
    }
}

public class EmailSettings
{
    public string SmtpServer { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "DinoMir";
}
*/