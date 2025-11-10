using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{
    public class AIUsageLimitDAO : IGenericDAO<AIUsageLimit>
    {
        private readonly AppDbContext _db;

        public AIUsageLimitDAO(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<AIUsageLimit> GetAllQueryable(Expression<Func<AIUsageLimit, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<AIUsageLimit> query = _db.AIUsageLimits;
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

        public async Task<AIUsageLimit?> GetAsync(Expression<Func<AIUsageLimit, bool>> predicate)
        {
            return await _db.AIUsageLimits
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching AIUsageLimit or null if none found
        }

        public async Task<bool> AddAsync(AIUsageLimit entity)
        {
            _db.AIUsageLimits.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(AIUsageLimit entity)
        {
            _db.AIUsageLimits.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(int AIUsageLimitId)
        {
            var AIUsageLimit = await _db.AIUsageLimits.FindAsync(AIUsageLimitId);
            if (AIUsageLimit == null)
            {
                return false; // AIUsageLimit not found
            }

            _db.AIUsageLimits.Remove(AIUsageLimit);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<AIUsageLimit, bool>> predicate)
        {
            return await _db.AIUsageLimits.AnyAsync(predicate);
        }
    }
}