using System.Threading.Tasks;
using CommentsDownloader.DTO.Entities;

namespace CommentsDownloader.Services
{
    public interface ICommentFetcher
    {
        Task FetchComments(CommentsRequest request);
    }
}