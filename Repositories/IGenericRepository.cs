using System.Linq.Expressions;

namespace Repositories
{
    public interface IGenericRepository<T>
    {
        // AsNoTracking when readonly to improve performance
        // Just ToListAsync() in the controller
        IQueryable<T> GetAllQueryable(Expression<Func<T, bool>>? predicate = null, bool asNoTracking = false);
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate);
        // Using bool to indicate success or failure of the operation
        Task<bool> AddAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int Id);
        // Check if an entity exists
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    }
}
