using System;

namespace CommentsDownloader.ViewModels
{
    public class CommentsRequestViewModel
    {
        public Guid Id { get; set; }
        public string RequestUrl { get; set; }
        public string Email { get; set; }
        public bool CommentsFetched { get; set; }
        public bool MailSent { get; set; }
        public string FileName { get; set; }
    }
}
