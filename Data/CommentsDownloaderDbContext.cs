
using Microsoft.EntityFrameworkCore;

namespace CommentsDownloader.Data
{
    public class CommentsDownloaderDbContext : DbContext
    {
        public CommentsDownloaderDbContext(DbContextOptions<CommentsDownloaderDbContext> options) : base(options)
        {

        }

        public CommentsDownloaderDbContext() { }
        // public DbSet<Post> Posts { get; set; }
    }
}
