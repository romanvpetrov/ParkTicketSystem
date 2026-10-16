using System.Net;
using TicketSystem.Domain.Orders;

namespace TicketSystem.Services.Shopping;

public sealed class TicketEmailService
{
    private readonly string _senderEmail;

    public TicketEmailService(string senderEmail)
    {
        if (string.IsNullOrWhiteSpace(senderEmail))
        {
            throw new ArgumentException("A sender email is required.", nameof(senderEmail));
        }

        _senderEmail = senderEmail;
    }

    public TicketEmail Create(Ticket ticket)
    {
        return new TicketEmail(
            _senderEmail,
            ticket.Email,
            $"Your ticket for {ticket.VisitDate:MMMM d, yyyy}",
            CreatePlainText(ticket),
            CreateHtml(ticket));
    }

    private static string CreatePlainText(Ticket ticket)
    {
        return $"""
            Park Ticket System

            Hello {ticket.FirstName} {ticket.LastName},

            Your ticket is ready.

            Product: {ticket.ProductId}
            Park: {ticket.ParkUniqueName}
            Visit date: {ticket.VisitDate:MMMM d, yyyy}
            Ticket ID: {ticket.Id}

            Please keep this email for your visit.
            """;
    }

    private static string CreateHtml(Ticket ticket)
    {
        var firstName = WebUtility.HtmlEncode(ticket.FirstName);
        var lastName = WebUtility.HtmlEncode(ticket.LastName);
        var productId = WebUtility.HtmlEncode(ticket.ProductId);
        var parkUniqueName = WebUtility.HtmlEncode(ticket.ParkUniqueName);

        return $"""
            <!doctype html>
            <html lang="en">
            <body style="margin:0;background:#f4f7fa;font-family:Arial,sans-serif;color:#17212b;">
              <div style="max-width:600px;margin:0 auto;padding:32px 16px;">
                <div style="background:#06324a;color:#ffffff;padding:24px;border-radius:8px 8px 0 0;">
                  <div style="font-size:13px;font-weight:bold;text-transform:uppercase;color:#ffe48a;">Park Ticket System</div>
                  <h1 style="margin:8px 0 0;font-size:28px;">Your ticket is ready</h1>
                </div>
                <div style="background:#ffffff;border:1px solid #d8e0e8;border-top:0;padding:24px;border-radius:0 0 8px 8px;">
                  <p style="margin-top:0;">Hello {firstName} {lastName},</p>
                  <p>Keep this email available when you arrive at the park.</p>
                  <table role="presentation" style="width:100%;border-collapse:collapse;margin:24px 0;">
                    <tr><td style="padding:10px 0;color:#667085;">Product</td><td style="padding:10px 0;text-align:right;font-weight:bold;">{productId}</td></tr>
                    <tr><td style="padding:10px 0;border-top:1px solid #d8e0e8;color:#667085;">Park</td><td style="padding:10px 0;border-top:1px solid #d8e0e8;text-align:right;font-weight:bold;">{parkUniqueName}</td></tr>
                    <tr><td style="padding:10px 0;border-top:1px solid #d8e0e8;color:#667085;">Visit date</td><td style="padding:10px 0;border-top:1px solid #d8e0e8;text-align:right;font-weight:bold;">{ticket.VisitDate:MMMM d, yyyy}</td></tr>
                  </table>
                  <div style="padding:16px;background:#eaf6f6;border-radius:6px;word-break:break-all;">
                    <div style="margin-bottom:5px;font-size:12px;font-weight:bold;text-transform:uppercase;color:#667085;">Ticket ID</div>
                    <strong>{ticket.Id}</strong>
                  </div>
                </div>
              </div>
            </body>
            </html>
            """;
    }
}
