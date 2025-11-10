using BusinessObjects;
using Repositories;
using System.Linq.Expressions;

namespace Services
{
    public class UserFollowCourseService : IGenericService<UserFollowCourse>
    {
        private readonly IGenericRepository<UserFollowCourse> _repo;

        public UserFollowCourseService(IGenericRepository<UserFollowCourse> repo)
        {
            _repo = repo;
        }

        public IQueryable<UserFollowCourse> GetAllQueryable(Expression<Func<UserFollowCourse, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<UserFollowCourse?> GetAsync(Expression<Func<UserFollowCourse, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(UserFollowCourse entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(UserFollowCourse entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int UserFollowCourseId)
        {
            return await _repo.DeleteAsync(UserFollowCourseId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<UserFollowCourse, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
