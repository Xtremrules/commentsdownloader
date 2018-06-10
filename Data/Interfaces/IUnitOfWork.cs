using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CommentsDownloader.Data.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        CommentsDownloaderDbContext Context { get;  }
        Task Commit();
    }
}
