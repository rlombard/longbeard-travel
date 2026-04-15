using AI.Forged.TourOps.Application.Models.EmailIntegrations;
using MimeKit;
using MimeKit.Utils;

namespace AI.Forged.TourOps.Infrastructure.Email;

internal static class EmailMimeFactory
{
    public static MimeMessage Create(EmailProviderSendRequest request)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(request.FromAddress));

        foreach (var address in request.ToAddresses)
        {
            message.To.Add(MailboxAddress.Parse(address));
        }

        foreach (var address in request.CcAddresses)
        {
            message.Cc.Add(MailboxAddress.Parse(address));
        }

        foreach (var address in request.BccAddresses)
        {
            message.Bcc.Add(MailboxAddress.Parse(address));
        }

        if (!string.IsNullOrWhiteSpace(request.ReplyToAddress))
        {
            message.ReplyTo.Add(MailboxAddress.Parse(request.ReplyToAddress));
        }

        message.Subject = request.Subject;
        message.MessageId = MimeUtils.GenerateMessageId();

        var bodyBuilder = new BodyBuilder
        {
            TextBody = request.BodyText,
            HtmlBody = request.BodyHtml
        };

        foreach (var attachment in request.Attachments)
        {
            bodyBuilder.Attachments.Add(
                attachment.FileName,
                Convert.FromBase64String(attachment.ContentBase64),
                ContentType.Parse(attachment.ContentType));
        }

        message.Body = bodyBuilder.ToMessageBody();
        return message;
    }
}
