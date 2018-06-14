using System.IO;

namespace CommentsDownloader.Models
{
    public class AppConstants
    {
        public const string Youtube = "youtube";
        public const string Amazon = "amazon";
        public const string YoutubeHost = "www.youtube.com";
        public const string AmazonHost = "www.amazon.com";
        public static string TempFileDirectory { get => Path.Combine(Directory.GetCurrentDirectory(), "TempFolder"); }
    }
}
