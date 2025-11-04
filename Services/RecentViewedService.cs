using BusinessObjects;
using Repositories;
using System.Linq.Expressions;

namespace Services
{
    public class RecentViewedService : IGenericService<RecentViewed>
    {
        private readonly IGenericRepository<RecentViewed> _repo;

        public RecentViewedService(IGenericRepository<RecentViewed> repo)
        {
            _repo = repo;
        }

        public IQueryable<RecentViewed> GetAllQueryable(Expression<Func<RecentViewed, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<RecentViewed?> GetAsync(Expression<Func<RecentViewed, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(RecentViewed entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(RecentViewed entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int RecentViewedId)
        {
            return await _repo.DeleteAsync(RecentViewedId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<RecentViewed, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
