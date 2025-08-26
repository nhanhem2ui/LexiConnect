using DataAccess;
using System.Linq.Expressions;

namespace Repositories
{
    public class CourseRepository : IGenericRepository<Course>
    {
        private readonly IGenericDAO<Course> _dao;

        public CourseRepository(IGenericDAO<Course> dao)
        {
            _dao = dao;
        }

        public IQueryable<Course> GetAllQueryable(Expression<Func<Course, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<Course?> GetAsync(Expression<Func<Course, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(Course entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(Course entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int CourseId)
        {
            return await _dao.DeleteAsync(CourseId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Course, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
