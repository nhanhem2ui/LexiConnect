using BusinessObjects;
using System.Linq.Expressions;

namespace Services
{
    public class CountryService : IGenericService<Country>
    {
        private readonly IGenericService<Country> _repo;

        public CountryService(IGenericService<Country> repo)
        {
            _repo = repo;
        }

        public IQueryable<Country> GetAllQueryable(Expression<Func<Country, bool>>? predicate = null, bool asNoTracking = true)
        {
            return _repo.GetAllQueryable(predicate, asNoTracking);
        }

        public async Task<Country?> GetAsync(Expression<Func<Country, bool>> predicate)
        {
            return await _repo.GetAsync(predicate);
        }

        public async Task<bool> AddAsync(Country entity)
        {
            return await _repo.AddAsync(entity);
        }

        public async Task<bool> UpdateAsync(Country entity)
        {
            return await _repo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(int CountryId)
        {
            return await _repo.DeleteAsync(CountryId);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Country, bool>> predicate)
        {
            return await _repo.ExistsAsync(predicate);
        }
    }
}
