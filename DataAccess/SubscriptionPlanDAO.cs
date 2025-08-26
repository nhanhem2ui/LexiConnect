using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{
    public class SubscriptionPlanDAO : IGenericDAO<SubscriptionPlan>
    {
        private readonly AppDbContext _db;

        public SubscriptionPlanDAO(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<SubscriptionPlan> GetAllQueryable(Expression<Func<SubscriptionPlan, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<SubscriptionPlan> query = _db.SubscriptionPlans;
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

        public async Task<SubscriptionPlan?> GetAsync(Expression<Func<SubscriptionPlan, bool>> predicate)
        {
            return await _db.SubscriptionPlans
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching SubscriptionPlan or null if none found
        }

        public async Task<bool> AddAsync(SubscriptionPlan entity)
        {
            _db.SubscriptionPlans.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(SubscriptionPlan entity)
        {
            _db.SubscriptionPlans.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(int SubscriptionPlanId)
        {
            var SubscriptionPlan = await _db.SubscriptionPlans.FindAsync(SubscriptionPlanId);
            if (SubscriptionPlan == null)
            {
                return false; // SubscriptionPlan not found
            }

            _db.SubscriptionPlans.Remove(SubscriptionPlan);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<SubscriptionPlan, bool>> predicate)
        {
            return await _db.SubscriptionPlans.AnyAsync(predicate);
        }
    }
}