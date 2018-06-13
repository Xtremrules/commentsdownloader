using System.Threading.Tasks;
using CommentsDownloader.DTO.Entities;

namespace CommentsDownloader.Services
{
    public interface ICommentFetcher
    {
        Task<string> FetchComments(CommentsRequest request);
    }
}