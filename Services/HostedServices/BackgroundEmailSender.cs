using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CommentsDownloader.Data;
using CommentsDownloader.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommentsDownloader.Services.HostedServices
{
    public class BackgroundEmailSender : BackgroundService, IMailService
    {
        private readonly ILogger<BackgroundEmailSender> _logger;
        private readonly SmtpClient _smtpClient;
        private readonly BufferBlock<Message> _messageQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public BackgroundEmailSender(ILogger<BackgroundEmailSender> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _smtpClient = CreateSmtpClient(configuration);
            _messageQueue = new BufferBlock<Message>();
            _serviceScopeFactory = serviceScopeFactory;
        }

        private SmtpClient CreateSmtpClient(IConfiguration configuration)
        {
            return new SmtpClient
            {
                Host = configuration["Smtp:Host"],
                Port = configuration.GetValue<int>("Smtp:Port"),
                EnableSsl = configuration.GetValue<bool>("Smtp:Ssl"),
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(
                                    userName: configuration["Smtp:Username"],
                                    password: configuration["Smtp:Password"]
                )
            };
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            _logger.LogInformation("Background Mail Sender started");
            token.Register(() => _logger.LogDebug($"Shutting down Background Mail Sender."));

            while (!token.IsCancellationRequested)
            {
                var messageContent = new Message();
                try
                {
                    //Let's wait for a message to appear in the queue
                    //If the token gets canceled, then we'll stop waiting
                    //since an OperationCanceledException will be thrown
                    _logger.LogInformation("waiting for new messages");
                    using(var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<CommentsDownloaderDbContext>();
                        await FetchAndSendRequest(dbContext);
                    }
                }
                catch (OperationCanceledException)
                {
                    //We need to terminate the delivery, so we'll just break the while loop
                    _logger.LogInformation("background mail sender is down.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"failed to send email, {ex.Message}");
                }
                await Task.Delay(10000, token);
            }
            _logger.LogInformation("Background Mail Sender stopped");
        }

        private async Task SendTestMail(Message messageContent)
        {
            var message = new MailMessage(
                from: "ade@ade.com",
                to: "mubarakadeimam@icloud.com"
            )
            {
                Subject = messageContent.Subject,
                Body = messageContent.Body,
                IsBodyHtml = true    
            };

            await _smtpClient.SendMailAsync(message);
        }

        public void SendMail(Message message)
        {
            bool posted = _messageQueue.Post(message);
            if(!posted)
            {
                throw new InvalidOperationException("Mail server no longer accepting mails");
            }
        }

        private async Task FetchAndSendRequest(CommentsDownloaderDbContext context)
        {
            var request = await context.CommentRequests.AsQueryable().FirstOrDefaultAsync(cr => !cr.CommentsFetched);
            if(request != null)
            {
                var commentData = FetchData(request.RequestUrl);
                var message = new Message
                {
                    Body = request.RequestUrl,
                    Subject = request.Email
                };
                await SendTestMail(message);
                request.CommentsFetched = true;
                context.Attach(request);
                context.Entry(request).State = EntityState.Modified;
                await context.SaveChangesAsync();
                _logger.LogDebug($"just sent another mail: {message.Subject}, {context.CommentRequests.Count()}");
            }
        }

        private async Task FetchData(string requestUrl)
        {
            var response = new HttpResponseMessage();
            using(var httpClient = new HttpClient())
            {
                var stream = await httpClient.GetAsync(requestUrl);
                //response = stream.();
            };
        }
    }
}
