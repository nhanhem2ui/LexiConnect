using BusinessObjects;
using DataAccess;
using System.Linq.Expressions;

namespace Repositories
{
    public class AIUsageLimitRepository : IGenericRepository<AIUsageLimit>
    {
        private readonly IGenericDAO<AIUsageLimit> _dao;

        public AIUsageLimitRepository(IGenericDAO<AIUsageLimit> dao)
        {
            _dao = dao;
        }

        public IQueryable<AIUsageLimit> GetAllQueryable(Expression<Func<AIUsageLimit, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<AIUsageLimit?> GetAsync(Expression<Func<AIUsageLimit, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(AIUsageLimit entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(AIUsageLimit entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int AIUsageLimitId)
        {
            return await _dao.DeleteAsync(AIUsageLimitId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<AIUsageLimit, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
