using System.Net.Http;
using System.Threading.Tasks;
using CommentsDownloader.DTO.Entities;
using CommentsDownloader.Extensions;
using Microsoft.Extensions.Logging;

namespace CommentsDownloader.Services.HostedServices
{
    public class AmazonFetcher : ICommentFetcher
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AmazonFetcher> _logger;

        public AmazonFetcher(ILogger<AmazonFetcher> logger)
        {
            _httpClient = new HttpClient();
            _logger = logger;
        }
        public async Task FetchComments(CommentsRequest request)
        {
            var response = await _httpClient.GetAsync(request.RequestUrl);
            await response.Content.ReadAsFileAsync($@"C:\\TempFile\\{request.Id}.txt", true);
            _logger.LogInformation($"fetching successfull for {request.RequestUrl}, {response.StatusCode}");
            ;
        }
    }
}
