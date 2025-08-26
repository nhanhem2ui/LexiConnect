using BusinessObjects;
using DataAccess;
using System.Linq.Expressions;

namespace Repositories
{
    public class UniversityRepository : IGenericRepository<University>
    {
        private readonly IGenericDAO<University> _dao;

        public UniversityRepository(IGenericDAO<University> dao)
        {
            _dao = dao;
        }

        public IQueryable<University> GetAllQueryable(Expression<Func<University, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<University?> GetAsync(Expression<Func<University, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(University entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(University entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int UniversityId)
        {
            return await _dao.DeleteAsync(UniversityId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<University, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
