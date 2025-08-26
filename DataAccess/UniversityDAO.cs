using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{
    public class UniversityDAO : IGenericDAO<University>
    {
        private readonly AppDbContext _db;

        public UniversityDAO(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<University> GetAllQueryable(Expression<Func<University, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<University> query = _db.Universities;
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

        public async Task<University?> GetAsync(Expression<Func<University, bool>> predicate)
        {
            return await _db.Universities
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching University or null if none found
        }

        public async Task<bool> AddAsync(University entity)
        {
            _db.Universities.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(University entity)
        {
            _db.Universities.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(int UniversityId)
        {
            var University = await _db.Universities.FindAsync(UniversityId);
            if (University == null)
            {
                return false; // University not found
            }

            _db.Universities.Remove(University);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<University, bool>> predicate)
        {
            return await _db.Universities.AnyAsync(predicate);
        }
    }
}