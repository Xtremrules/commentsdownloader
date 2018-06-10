
using CommentsDownloader.DTO.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommentsDownloader.Data
{
    public class CommentsDownloaderDbContext : DbContext
    {
        public CommentsDownloaderDbContext(DbContextOptions<CommentsDownloaderDbContext> options) : base(options)
        {

        }

        public CommentsDownloaderDbContext() { }

        public DbSet<CommentsRequest> CommentRequests { get; set; }
    }
}
