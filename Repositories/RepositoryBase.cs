using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ChatBot.Database;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.EntityFrameworkCore;

namespace ChatBot.Repositories
{
    public abstract class RepositoryBase<T> : IBaseRepository<T> where T : class
    {
        private readonly BankDbContext _dbContext;

        public RepositoryBase(BankDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<T> GetById(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public IQueryable<T> FindQueryable(Expression<Func<T, bool>> expression,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            var query = _dbContext.Set<T>().Where(expression);
            return orderBy != null ? orderBy(query) : query;
        }

        public Task<List<T>> FindListAsync(Expression<Func<T, bool>>? expression, Func<IQueryable<T>,
            IOrderedQueryable<T>>? orderBy = null)
        {
            var query = expression != null ? _dbContext.Set<T>().Where(expression) : _dbContext.Set<T>();
            return orderBy != null
                ? orderBy(query).ToListAsync()
            : query.ToListAsync();
        }

        public async Task<List<T>> GetByPageAsync(Expression<Func<T, bool>> predicate, int selectedPage, int maxNumberPerItemsPage)
        {
            return await _dbContext.Set<T>().Where(predicate)
                   .Skip((selectedPage - 1) * maxNumberPerItemsPage)
                   .Take(maxNumberPerItemsPage)
                   .ToListAsync();
        }

        public Task<List<T>> FindAllAsync() 
        {
            return _dbContext.Set<T>().ToListAsync();
        }

        public Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> expression, string includeProperties) 
        {
            var query = _dbContext.Set<T>().AsQueryable();

            query = includeProperties.Split(new char[] { ',' },
                StringSplitOptions.RemoveEmptyEntries).Aggregate(query, (current, includeProperty)
                => current.Include(includeProperty));

            return query.SingleOrDefaultAsync(expression);
        }

        public T Add(T entity) 
        {
            return _dbContext.Set<T>().Add(entity).Entity;
        }

        public void Update(T entity) 
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public void UpdateRange(IEnumerable<T> entities) 
        {
            _dbContext.Set<T>().UpdateRange(entities);
        }

        public void Delete(T entity) 
        {
            _dbContext.Set<T>().Remove(entity);
        }
    }
}

