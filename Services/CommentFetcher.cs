using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CommentsDownloader.DTO.Entities;
using CommentsDownloader.Extensions;
using CommentsDownloader.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CommentsDownloader.Services {
    public abstract class CommentFetcher : ICommentFetcher {
        protected readonly HttpClient _httpClient;
        protected readonly ILogger<CommentFetcher> _logger;
        protected readonly IConfiguration _config;
        protected readonly string _path;

        public CommentFetcher (ILogger<CommentFetcher> logger,
            IConfiguration config) {
            _httpClient = new HttpClient ();
            _logger = logger;
            _config = config;
            _path = AppConstants.TempFileDirectory;
        }
        public virtual async Task<string> FetchComments (CommentsRequest request) {
            var response = await _httpClient.GetAsync (request.RequestUrl);
            var fileName = request.Id + "csv";
            var fullFilePath = Path.Combine (_path, fileName);
            await response.Content.ReadAsFileAsync (fullFilePath, true);
            _logger.LogInformation ($"fetching successfull for {request.RequestUrl}, {response.StatusCode}");
            return fullFilePath;
        }
    }
}