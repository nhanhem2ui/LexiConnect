using BusinessObjects;
using System.Linq.Expressions;

namespace Services
{
    public class MajorService : IGenericService<Major>
    {
        private readonly IGenericService<Major> _repo;

        public MajorService(IGenericService<Major> repo)
        {
            _repo = repo;
        }

        public IQueryable<Major> GetAllQueryable(Expression<Func<Major, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<Major?> GetAsync(Expression<Func<Major, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(Major entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(Major entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int MajorId)
        {
            return await _repo.DeleteAsync(MajorId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Major, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
