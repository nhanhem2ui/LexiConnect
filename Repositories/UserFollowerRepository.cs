using BusinessObjects;
using DataAccess;
using System.Linq.Expressions;

namespace Repositories
{
    public class UserFollowerRepository : IGenericRepository<UserFollower>
    {
        private readonly IGenericDAO<UserFollower> _dao;

        public UserFollowerRepository(IGenericDAO<UserFollower> dao)
        {
            _dao = dao;
        }

        public IQueryable<UserFollower> GetAllQueryable(Expression<Func<UserFollower, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<UserFollower?> GetAsync(Expression<Func<UserFollower, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(UserFollower entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(UserFollower entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int UserFollowerId)
        {
            return await _dao.DeleteAsync(UserFollowerId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<UserFollower, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
