using System.Linq.Expressions;

namespace Services
{
    public class CourseService : IGenericService<Course>
    {
        private readonly IGenericService<Course> _repo;

        public CourseService(IGenericService<Course> repo)
        {
            _repo = repo;
        }

        public IQueryable<Course> GetAllQueryable(Expression<Func<Course, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<Course?> GetAsync(Expression<Func<Course, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(Course entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(Course entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int CourseId)
        {
            return await _repo.DeleteAsync(CourseId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Course, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
