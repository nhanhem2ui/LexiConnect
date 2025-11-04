using BusinessObjects;
using System.Linq.Expressions;

namespace Services
{
    public class UniversityService : IGenericService<University>
    {
        private readonly IGenericService<University> _repo;

        public UniversityService(IGenericService<University> repo)
        {
            _repo = repo;
        }

        public IQueryable<University> GetAllQueryable(Expression<Func<University, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<University?> GetAsync(Expression<Func<University, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(University entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(University entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int UniversityId)
        {
            return await _repo.DeleteAsync(UniversityId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<University, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
