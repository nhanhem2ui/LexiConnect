using BusinessObjects;
using DataAccess;
using System.Linq.Expressions;

namespace Repositories
{
    public class CountryRepository : IGenericRepository<Country>
    {
        private readonly IGenericDAO<Country> _dao;

        public CountryRepository(IGenericDAO<Country> dao)
        {
            _dao = dao;
        }

        public IQueryable<Country> GetAllQueryable(Expression<Func<Country, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _dao.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<Country?> GetAsync(Expression<Func<Country, bool>> predicate)
        {
            return await _dao.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(Country entity)
        {
            return await _dao.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(Country entity)
        {
            return await _dao.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int CountryId)
        {
            return await _dao.DeleteAsync(CountryId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Country, bool>> predicate)
        {
            return await _dao.ExistsAsync(predicate);
        }
    }
}
