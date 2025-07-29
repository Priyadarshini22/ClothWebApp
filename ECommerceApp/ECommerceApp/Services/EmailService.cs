using ECommerceApp.Repository;
using ECommerceApp.Services;

public class EmailService
{
    private readonly IEmailRepository  _emailRepository;

    public EmailService(IEmailRepository emailRepository)
    {
        _emailRepository = emailRepository;
    }

    public async Task SendOrderConfirmationAsync(string email, string orderNumber)
    {
        string subject = $"Order Confirmation - #{orderNumber}";
        string body = $"<h3>Thank you for your order #{orderNumber}</h3><p>We are processing your order.</p>";

        await _emailRepository.SendEmailAsync(email, subject, body, isBodyHtml: true);
    }

    // Add other email types like password reset, promo, etc.
}
