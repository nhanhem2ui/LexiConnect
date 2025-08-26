using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{
    public class CountryDAO : IGenericDAO<Country>
    {
        private readonly AppDbContext _db;

        public CountryDAO(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<Country> GetAllQueryable(Expression<Func<Country, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<Country> query = _db.Countries;
            if (asNoTracking)
            {
                query = query.AsNoTracking(); // Use AsNoTracking for read-only queries
            }
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            return query;
        }

        public async Task<Country?> GetAsync(Expression<Func<Country, bool>> predicate)
        {
            return await _db.Countries
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching Country or null if none found
        }

        public async Task<bool> AddAsync(Country entity)
        {
            _db.Countries.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(Country entity)
        {
            _db.Countries.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(int CountryId)
        {
            var Country = await _db.Countries.FindAsync(CountryId);
            if (Country == null)
            {
                return false; // Country not found
            }

            _db.Countries.Remove(Country);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<Country, bool>> predicate)
        {
            return await _db.Countries.AnyAsync(predicate);
        }
    }
}
