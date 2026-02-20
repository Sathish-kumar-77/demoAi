using System.Net;
using System.Net.Mail;

namespace UpiFraudApi.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public void SendFraudAlert(string toEmail, string subject, string body)
    {
        var smtpSection = _config.GetSection("Smtp");
        var host = smtpSection["Host"];
        var user = smtpSection["User"];
        var pass = smtpSection["Pass"];
        var from = smtpSection["From"];
        var port = int.TryParse(smtpSection["Port"], out var parsedPort) ? parsedPort : 587;

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass) || string.IsNullOrWhiteSpace(from))
        {
            return;
        }

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(user, pass),
            EnableSsl = true
        };

        var message = new MailMessage(from, toEmail, subject, body);
        client.Send(message);
    }
}
