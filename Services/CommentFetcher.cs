using System.Net.Http;
using System.Threading.Tasks;
using CommentsDownloader.DTO.Entities;
using CommentsDownloader.Extensions;
using Microsoft.Extensions.Logging;

namespace CommentsDownloader.Services {
    public abstract class CommentFetcher : ICommentFetcher {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CommentFetcher> _logger;

        public CommentFetcher (ILogger<CommentFetcher> logger) {
            _httpClient = new HttpClient ();
            _logger = logger;
        }
        public virtual async Task<string> FetchComments (CommentsRequest request) {
            var response = await _httpClient.GetAsync (request.RequestUrl);
            var fileName = request.Id + ".txt";
            await response.Content.ReadAsFileAsync($@"C:\\TempFile\\{fileName}", true);
            _logger.LogInformation ($"fetching successfull for {request.RequestUrl}, {response.StatusCode}");
            return fileName;
        }
    }
}
