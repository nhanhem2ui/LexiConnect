using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{
    public class MajorDAO : IGenericDAO<Major>
    {
        private readonly AppDbContext _db;

        public MajorDAO(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<Major> GetAllQueryable(Expression<Func<Major, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<Major> query = _db.Majors;
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

        public async Task<Major?> GetAsync(Expression<Func<Major, bool>> predicate)
        {
            return await _db.Majors
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching Major or null if none found
        }

        public async Task<bool> AddAsync(Major entity)
        {
            _db.Majors.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(Major entity)
        {
            _db.Majors.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(int MajorId)
        {
            var Major = await _db.Majors.FindAsync(MajorId);
            if (Major == null)
            {
                return false; // Major not found
            }

            _db.Majors.Remove(Major);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<Major, bool>> predicate)
        {
            return await _db.Majors.AnyAsync(predicate);
        }
    }
}
