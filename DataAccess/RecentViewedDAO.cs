using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{
    public class RecentViewedDAO : IGenericDAO<RecentViewed>
    {

        private readonly AppDbContext _db;

        public RecentViewedDAO(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<RecentViewed> GetAllQueryable(Expression<Func<RecentViewed, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<RecentViewed> query = _db.RecentVieweds;
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

        public async Task<RecentViewed?> GetAsync(Expression<Func<RecentViewed, bool>> predicate)
        {
            return await _db.RecentVieweds
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching RecentViewed or null if none found
        }

        public async Task<bool> AddAsync(RecentViewed entity)
        {
            _db.RecentVieweds.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(RecentViewed entity)
        {
            _db.RecentVieweds.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(int RecentViewedId)
        {
            var RecentViewed = await _db.RecentVieweds.FindAsync(RecentViewedId);
            if (RecentViewed == null)
            {
                return false; // RecentViewed not found
            }

            _db.RecentVieweds.Remove(RecentViewed);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<RecentViewed, bool>> predicate)
        {
            return await _db.RecentVieweds.AnyAsync(predicate);
        }
    }
}