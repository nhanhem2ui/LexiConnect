using BusinessObjects;
using System.Linq.Expressions;

namespace Services
{
    public class UserFollowerService : IGenericService<UserFollower>
    {
        private readonly IGenericService<UserFollower> _repo;

        public UserFollowerService(IGenericService<UserFollower> repo)
        {
            _repo = repo;
        }

        public IQueryable<UserFollower> GetAllQueryable(Expression<Func<UserFollower, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<UserFollower?> GetAsync(Expression<Func<UserFollower, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(UserFollower entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(UserFollower entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int UserFollowerId)
        {
            return await _repo.DeleteAsync(UserFollowerId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<UserFollower, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
