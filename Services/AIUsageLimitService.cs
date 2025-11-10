using BusinessObjects;
using Repositories;
using System.Linq.Expressions;

namespace Services
{
    public class AIUsageLimitService : IGenericService<AIUsageLimit>
    {
        private readonly IGenericRepository<AIUsageLimit> _repo;

        public AIUsageLimitService(IGenericRepository<AIUsageLimit> repo)
        {
            _repo = repo;
        }

        public IQueryable<AIUsageLimit> GetAllQueryable(Expression<Func<AIUsageLimit, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<AIUsageLimit?> GetAsync(Expression<Func<AIUsageLimit, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(AIUsageLimit entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(AIUsageLimit entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int AIUsageLimitId)
        {
            return await _repo.DeleteAsync(AIUsageLimitId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<AIUsageLimit, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
