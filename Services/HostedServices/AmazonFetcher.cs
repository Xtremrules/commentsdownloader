using System.Net.Http;
using System.Threading.Tasks;
using CommentsDownloader.DTO.Entities;
using CommentsDownloader.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CommentsDownloader.Services.HostedServices {
        public class AmazonFetcher : CommentFetcher {
            public AmazonFetcher (ILogger<AmazonFetcher> logger, IConfiguration config) : base (logger, config) { }
            }
        }