using System;

namespace CommentsDownloader.Models {
    public class RefinedComment {
        public string Username { get; set; }
        public DateTime Date { get; set; }
        public string Rating { get; set; }
        public string Comment { get; set; }
        public string Link { get; set; }
    }
}