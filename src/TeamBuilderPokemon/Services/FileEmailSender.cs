using Microsoft.AspNetCore.Identity.UI.Services;

namespace TeamBuilderPokemon.Services;

/// <summary>
/// "Envia" e-mail salvando um .html na pasta App_Data/emails. Serve pra testar a confirmacao
/// de cadastro sem conta em provedor nenhum; pra valer, troque por SendGrid/SMTP.
/// </summary>
public class FileEmailSender : IEmailSender
{
    private readonly string _folder;
    private readonly ILogger<FileEmailSender> _logger;

    public FileEmailSender(IWebHostEnvironment env, ILogger<FileEmailSender> logger)
    {
        _folder = Path.Combine(env.ContentRootPath, "App_Data", "emails");
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Directory.CreateDirectory(_folder);
        var safeEmail = string.Concat(email.Split(Path.GetInvalidFileNameChars()));
        var file = Path.Combine(_folder, $"{DateTime.UtcNow:yyyyMMdd-HHmmss}-{safeEmail}.html");
        var body = $"<p><strong>Para:</strong> {System.Net.WebUtility.HtmlEncode(email)}</p>"
            + $"<p><strong>Assunto:</strong> {System.Net.WebUtility.HtmlEncode(subject)}</p><hr />{htmlMessage}";
        await File.WriteAllTextAsync(file, body);
        _logger.LogInformation("E-mail de teste salvo em {File}", file);
    }
}
