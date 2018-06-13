using System.Net.Http;
using System.Threading.Tasks;
using CommentsDownloader.DTO.Entities;
using CommentsDownloader.Extensions;
using Microsoft.Extensions.Logging;

namespace CommentsDownloader.Services.HostedServices {
    public class YoutubeFetcher : CommentFetcher {
        public YoutubeFetcher (ILogger<YoutubeFetcher> logger) : base(logger) {
        }
    }
}
