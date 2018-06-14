using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommentsDownloader.DTO.Entities;
using CommentsDownloader.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CommentsDownloader.Services.HostedServices {
    public class YoutubeFetcher : CommentFetcher {
        private readonly string _endpoint;

        public YoutubeFetcher (ILogger<YoutubeFetcher> logger, IConfiguration config) : base (logger, config) {
            var apiKey = _config["Youtube:ApiKey"];
            _endpoint = $"https://www.googleapis.com/youtube/v3/commentThreads?key={apiKey}";
        }

        public override async Task<string> FetchComments (CommentsRequest request) {
            #region initialize
            var videoId = GetVideoId (request.RequestUrl);
            var query = BuildQuery (videoId);
            var response = await _httpClient.GetAsync (query);
            #endregion

            #region needed params
            var fileName = request.Id + ".csv";
            var currentResponse = new YouTubeCommentSet ();
            bool hasMore = false;
            string nextPageToken = "";
            var refinedComments = new List<RefinedYoutubeComment> ();
            #endregion

            do {
                if (hasMore) {
                    var newQuery = AddNextPageToken (query, nextPageToken);
                    response = await _httpClient.GetAsync (newQuery);
                }
                currentResponse = await response.Content.ReadAsAsync<YouTubeCommentSet> ();
                var snippets = currentResponse.Items.Select (c => c.Snippet)
                    .Select (d => d.TopLevelComment)
                    .Select (e => new RefinedYoutubeComment {
                        Username = e.Snippet.AuthorDisplayName,
                            Date = e.Snippet.PublishedAt,
                            Rating = e.Snippet.ViewerRating,
                            Comment = e.Snippet.TextOriginal,
                            Link = GetDirectCommentLink (videoId, e.Id)
                    });
                refinedComments.AddRange (snippets);
                nextPageToken = currentResponse.NextPageToken;
                hasMore = currentResponse.PageInfo.TotalResults == 100 && !string.IsNullOrEmpty (nextPageToken);
            } while (hasMore);

            var fileSaved = SaveToFile (fileName, refinedComments);
            _logger.LogInformation ($"fetching successfull for {request.RequestUrl}, {response.StatusCode}");
            return fileSaved ? fileName : "" ;
        }

        public bool SaveToFile (string fileName, List<RefinedYoutubeComment> comments) {
            var writer = new CsvWriter ();
            var fullFilePath = Path.Combine(_path, fileName);
            return writer.Write(comments, fileName, true);
        }

        public string GetVideoId (string url) {
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery (new Uri (url)?.Query);
            var videoId = query.GetValueOrDefault ("v");
            return videoId;
        }

        public string GetDirectCommentLink (string videoId, string commentId) {
            return $"https://www.youtube.com/watch?v={videoId}&lc={commentId}";
        }

        public string BuildQuery (string videoId) {
            return $"{_endpoint}&videoId={videoId}&part=snippet&maxResults=100";
        }

        public string AddNextPageToken (string url, string nextPageToken) {
            return $"{url}&pageToken={nextPageToken}";
        }

        private class YouTubeCommentSet {
            public string NextPageToken { get; set; }
            public PageInfo PageInfo { get; set; }
            public IList<Comment> Items { get; set; }
        }

        private class PageInfo {
            public int TotalResults { get; set; }
        }

        private class Comment {
            public string Id { get; set; }
            public ParentSnippet Snippet { get; set; }

        }

        private class ParentSnippet {
            public string VideoId { get; set; }
            public TopLevelComment TopLevelComment { get; set; }
        }

        private class TopLevelComment {
            public string Id { get; set; }
            public ChildSnippet Snippet { get; set; }
        }

        private class ChildSnippet {
            public string AuthorDisplayName { get; set; }
            public DateTime PublishedAt { get; set; }
            public string ViewerRating { get; set; }
            public string TextOriginal { get; set; }
        }

        public class RefinedYoutubeComment {
            public string Username { get; set; }
            public DateTime Date { get; set; }
            public string Rating { get; set; }
            public string Comment { get; set; }
            public string Link { get; set; }
        }
    }
}