using BusinessObjects;
using DataAccess;
using System.Linq.Expressions;

namespace Repositories
{
    public class UserFollowCourseRepository : IGenericRepository<UserFollowCourse>
    {
        private readonly IGenericDAO<UserFollowCourse> _dao;

        public UserFollowCourseRepository(IGenericDAO<UserFollowCourse> dao)
        {
            _dao = dao;
        }

        public IQueryable<UserFollowCourse> GetAllQueryable(Expression<Func<UserFollowCourse, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<UserFollowCourse?> GetAsync(Expression<Func<UserFollowCourse, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(UserFollowCourse entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(UserFollowCourse entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int UserFollowCourseId)
        {
            return await _dao.DeleteAsync(UserFollowCourseId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<UserFollowCourse, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
