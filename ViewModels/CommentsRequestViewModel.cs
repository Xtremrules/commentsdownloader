namespace CommentsDownloader.ViewModels
{
    public class CommentsRequestViewModel
    {
        public string RequestUrl { get; set; }
        public string Email { get; set; }
        public bool CommentsFetched { get; set; }
        public bool MailSent { get; set; }
    }
}
