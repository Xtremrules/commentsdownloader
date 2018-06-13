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
    public class BackgroundReviewWorker : BackgroundService
    {
        private readonly ILogger<BackgroundReviewWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Func<string, ICommentFetcher> _fetcherFactory;

        public BackgroundReviewWorker(
            ILogger<BackgroundReviewWorker> logger,
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            Func<string, ICommentFetcher> fetcherFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _fetcherFactory = fetcherFactory;
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
            _logger.LogInformation("Background Review Fetcher started");
            token.Register(() => _logger.LogDebug($"Shutting down Background Review Fetcher."));

            while (!token.IsCancellationRequested)
            {
                var messageContent = new Message();
                try
                {
                    //Let's wait for a message to appear in the queue
                    //If the token gets canceled, then we'll stop waiting
                    //since an OperationCanceledException will be thrown
                    _logger.LogInformation("waiting for new requests");
                    using(var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<CommentsDownloaderDbContext>();
                        await FetchAndSendRequests(dbContext);
                    }
                }
                catch (OperationCanceledException)
                {
                    //We need to terminate the delivery, so we'll just break the while loop
                    _logger.LogInformation("background review fetcher is down.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"failed to fetch reviews, {ex.Message}");
                }
                await Task.Delay(10000, token);
            }
            _logger.LogInformation("Background Review fetcher stopped");
        }
        
        private async Task FetchAndSendRequests(CommentsDownloaderDbContext context)
        {
            var requests = context.CommentRequests.Where(cr => !cr.CommentsFetched).ToList();
            if(requests.Any()){
                foreach(var request in requests)
                {
                    var host = new Uri(request.RequestUrl)?.Host;
                    string fileName = "";
                    switch(host)
                    {
                        case AppConstants.YoutubeHost:
                            _logger.LogInformation("Hello Youtube");
                            fileName = await _fetcherFactory(AppConstants.Youtube).FetchComments(request);
                            break;
                        case AppConstants.AmazonHost:
                            _logger.LogInformation("Hello Amazon");
                            fileName = await _fetcherFactory(AppConstants.Amazon).FetchComments(request);
                            break;
                        default:
                            _logger.LogInformation("We haven't implemented that yet");
                            break;
                    }
                    request.CommentsFetched = true;
                    request.ModifiedBy = "System";
                    request.TempFileDirectory = fileName;
                    request.ModifiedDate = DateTime.UtcNow;
                    context.Attach(request);
                    context.Entry(request).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
