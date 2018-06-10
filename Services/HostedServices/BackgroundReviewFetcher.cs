using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace CommentsDownloader.Services.HostedServices
{
    public class BackgroundReviewFetcher : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
