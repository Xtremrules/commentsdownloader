using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using CommentsDownloader.Data;
using CommentsDownloader.DTO.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommentsDownloader.Services.HostedServices {
    public class BackgroundEMailSender : BackgroundService {
        private readonly ILogger<BackgroundEMailSender> _logger;
        private readonly SmtpClient _smtpClient;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public BackgroundEMailSender (
            ILogger<BackgroundEMailSender> logger,
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory) {
            _logger = logger;
            _smtpClient = CreateSmtpClient (configuration);
            _serviceScopeFactory = serviceScopeFactory;
        }

        private SmtpClient CreateSmtpClient (IConfiguration configuration) {
            return new SmtpClient {
                Host = configuration["Smtp:Host"],
                    Port = configuration.GetValue<int> ("Smtp:Port"),
                    EnableSsl = configuration.GetValue<bool> ("Smtp:Ssl"),
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential (
                        userName: configuration["Smtp:Username"],
                        password : configuration["Smtp:Password"]
                    )
            };
        }

        protected override async Task ExecuteAsync (CancellationToken token) {
            _logger.LogInformation ("Background Email Sender started");
            token.Register (() => _logger.LogDebug ($"Shutting down Background Email Sender."));

            while (!token.IsCancellationRequested) {
                try {
                    //Let's wait for a message to appear in the queue
                    //If the token gets canceled, then we'll stop waiting
                    //since an OperationCanceledException will be thrown
                    _logger.LogInformation ("waiting for new requests");
                    using (var scope = _serviceScopeFactory.CreateScope ()) {
                        var dbContext = scope.ServiceProvider.GetRequiredService<CommentsDownloaderDbContext> ();
                        await FetchAndSendEmail (dbContext);
                    }
                } catch (OperationCanceledException) {
                    //We need to terminate the delivery, so we'll just break the while loop
                    _logger.LogInformation ("background email sender is down.");
                    break;
                } catch (Exception ex) {
                    _logger.LogWarning ($"failed to fetch email, {ex.Message}");
                }
                await Task.Delay (10000, token);
            }
            _logger.LogInformation ("Background Email Sender stopped");
        }

        private async Task FetchAndSendEmail(CommentsDownloaderDbContext context) {
            var messages = context.CommentRequests.Where(
                cr => cr.CommentsFetched && !cr.MailSent).ToList();
            if (messages.Any()) {
                foreach(var message in messages) {
                    _logger.LogInformation ($"sending mail for {message.TempFileDirectory}");
                    message.MailSent = await SendMail(message);
                    message.ModifiedBy = "System";
                    message.ModifiedDate = DateTime.UtcNow;
                    context.Attach(message);
                    context.Entry(message).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                }
            }
        }

        private async Task<bool> SendMail(CommentsRequest request)
        {
            return await Task.FromResult(true);
        } 
    }
}