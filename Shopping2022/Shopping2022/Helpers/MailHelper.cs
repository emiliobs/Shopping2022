using MailKit.Net.Smtp;
using MimeKit;
using Shopping2022.Common;

namespace Shopping2022.Helpers
{
    public class MailHelper : IMailHelper
    {
        private readonly IConfiguration _configuration;

        public MailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public Response SendMail(string toName, string toEmail, string subject, string body)
        {
            try
            {
                string from = _configuration["Mail:From"];
                string name = _configuration["mail:Name"];
                string smtp = _configuration["mail:Smtp"];
                string port = _configuration["mail:Port"];
                string password = _configuration["Mail:Password"];

                MimeMessage message = new();
                message.From.Add(new MailboxAddress(name, from));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;
                BodyBuilder bodyBuilder = new()
                {
                    HtmlBody = body,
                };
                message.Body = bodyBuilder.ToMessageBody();

                using (SmtpClient client = new())
                {
                    client.Connect(smtp, int.Parse(port));
                    client.Authenticate(from, password);
                    _ = client.Send(message);
                    client.Disconnect(true);
                }

                return new Response { IsSuccess = true };
            }
            catch (Exception ex)
            {

                return new Response { IsSuccess = false, Message = ex.Message, Result = ex };
            }
        }
    }
}
