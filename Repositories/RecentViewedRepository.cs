using BusinessObjects;
using DataAccess;
using System.Linq.Expressions;

namespace Repositories
{
    public class RecentViewedRepository : IGenericRepository<RecentViewed>
    {
        private readonly IGenericDAO<RecentViewed> _dao;

        public RecentViewedRepository(IGenericDAO<RecentViewed> dao)
        {
            _dao = dao;
        }

        public IQueryable<RecentViewed> GetAllQueryable(Expression<Func<RecentViewed, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<RecentViewed?> GetAsync(Expression<Func<RecentViewed, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(RecentViewed entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(RecentViewed entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int RecentViewedId)
        {
            return await _dao.DeleteAsync(RecentViewedId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<RecentViewed, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
