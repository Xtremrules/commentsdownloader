using System.Threading.Tasks;
using CommentsDownloader.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommentsDownloader.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        public CommentsDownloaderDbContext Context { get; }

        public UnitOfWork(CommentsDownloaderDbContext context)
        {
            Context = context;
        }
        public async Task Commit()
        {
            await Context.SaveChangesAsync();
        }
 
        public void Dispose()
        {
           Context.Dispose();   
        }
    }
}