using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CommentsDownloader.Data.Interfaces;
using CommentsDownloader.DTO.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CommentsDownloader.Data
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity 
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<Repository<TEntity>> _logger;

        public Repository(IUnitOfWork unitOfWork, ILogger<Repository<TEntity>> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        protected virtual IQueryable<TEntity> GetQueryable(
            System.Linq.Expressions.Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null,
            int? skip = null,
            int? take = null)
        {
            includeProperties = includeProperties ?? string.Empty;
            IQueryable<TEntity> query = _unitOfWork.Context.Set<TEntity>();

            if (filter != null) query = query.Where(filter);

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null) query = orderBy(query);

            if (skip.HasValue) query = query.Skip(skip.Value);

            if (take.HasValue) query = query.Take(take.Value);

            return query;
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null,
            int? skip = null,
            int? take = null)
        {
            return await GetQueryable(null, orderBy, includeProperties, skip, take).ToListAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        string includeProperties = null,
        int? skip = null,
        int? take = null)
        {
            return await GetQueryable(filter, orderBy, includeProperties, skip, take).ToListAsync();
        }

        public virtual async Task<TEntity> GetOneAsync(
            Expression<Func<TEntity, bool>> filter = null,
            string includeProperties = null)
        {
            return await GetQueryable(filter, null, includeProperties).SingleOrDefaultAsync();
        }

        public virtual async Task<TEntity> GetFirstAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null)
        {
            return await GetQueryable(filter, orderBy, includeProperties).FirstOrDefaultAsync();
        }

        public virtual Task<TEntity> GetByIdAsync(Guid id)
        {
            return _unitOfWork.Context.Set<TEntity>().FindAsync(id);
        }

        public virtual Task<int> GetCountAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            return GetQueryable(filter).CountAsync();
        }

        public virtual Task<bool> GetExistsAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            return GetQueryable(filter).AnyAsync();
        }

        public virtual void Create(TEntity entity, string createdBy = null)
        {
            entity.CreatedDate = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            _unitOfWork.Context.Set<TEntity>().Add(entity);
        }

        public virtual void Update(TEntity entity, string modifiedBy = null)
        {
            entity.ModifiedDate = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;
            _unitOfWork.Context.Set<TEntity>().Attach(entity);
            _unitOfWork.Context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Delete(Guid id, string deletedBy = null)
        {
            TEntity entity = _unitOfWork.Context.Set<TEntity>().Find(id);
            if(entity != null) Delete(entity, deletedBy);
        }

        public virtual void Delete(TEntity entity, string deletedBy = null)
        {
            var dbSet = _unitOfWork.Context.Set<TEntity>();
            entity.Deleted = DateTime.UtcNow;
            entity.ModifiedBy = deletedBy;
            if (_unitOfWork.Context.Entry(entity).State == EntityState.Modified)
            {
                dbSet.Attach(entity);
            }
            _unitOfWork.Context.Entry(entity).State = EntityState.Modified;
        }

        public virtual async Task SaveAsync()
        {
            await _unitOfWork.Commit();
        }
    }
}
