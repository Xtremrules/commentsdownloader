using CommentsDownloader.DTO.Interfaces;

namespace CommentsDownloader.ViewModels
{
    public class CommentsRequestCreate : ICommentsRequest
    {
        public string RequestUrl { get; set; }
        public string Email { get; set; }
    }
}
