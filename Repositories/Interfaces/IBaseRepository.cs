using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ChatBot.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T> GetById(int id);
        IQueryable<T> FindQueryable(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
        Task<List<T>> FindListAsync(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
        Task<List<T>> FindAllAsync();
        Task<List<T>> GetByPageAsync(Expression<Func<T, bool>> predicate, int selectedPage, int maxNumberPerItemsPage);
        Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> expression, string includeProperties);
        T Add(T entity);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Delete(T entity);
    }
}

