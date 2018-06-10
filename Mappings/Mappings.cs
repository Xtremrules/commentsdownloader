using CommentsDownloader.DTO.Entities;
using CommentsDownloader.Models;
using CommentsDownloader.ViewModels;

namespace CommentsDownloader.Mappings
{
    public static class Mappings
    {
        public static CommentsRequest ToModel(this CommentsRequestCreate model)
        {
            return new CommentsRequest
            {
                RequestUrl = model.RequestUrl,
                Email = model.Email
            };
        }

        public static CommentsRequestViewModel ToViewModel(this CommentsRequest model)
        {
            return new CommentsRequestViewModel
            {
                RequestUrl = model.RequestUrl,
                Email = model.Email,
                MailSent = model.MailSent,
                CommentsFetched = model.CommentsFetched
            };
        }
    }
}
