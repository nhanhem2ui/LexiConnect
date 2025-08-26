using BusinessObjects;
using DataAccess;
using System.Linq.Expressions;

namespace Repositories
{
    public class MajorRepository : IGenericRepository<Major>
    {
        private readonly IGenericDAO<Major> _dao;

        public MajorRepository(IGenericDAO<Major> dao)
        {
            _dao = dao;
        }

        public IQueryable<Major> GetAllQueryable(Expression<Func<Major, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<Major?> GetAsync(Expression<Func<Major, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(Major entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(Major entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int MajorId)
        {
            return await _dao.DeleteAsync(MajorId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Major, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
