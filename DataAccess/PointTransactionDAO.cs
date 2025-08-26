using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{
    public class PointTransactionDAO : IGenericDAO<PointTransaction>
    {
        private readonly AppDbContext _db;

        public PointTransactionDAO(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<PointTransaction> GetAllQueryable(Expression<Func<PointTransaction, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<PointTransaction> query = _db.PointTransactions;
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

        public async Task<PointTransaction?> GetAsync(Expression<Func<PointTransaction, bool>> predicate)
        {
            return await _db.PointTransactions
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching PointTransaction or null if none found
        }

        public async Task<bool> AddAsync(PointTransaction entity)
        {
            _db.PointTransactions.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(PointTransaction entity)
        {
            _db.PointTransactions.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(int PointTransactionId)
        {
            var PointTransaction = await _db.PointTransactions.FindAsync(PointTransactionId);
            if (PointTransaction == null)
            {
                return false; // PointTransaction not found
            }

            _db.PointTransactions.Remove(PointTransaction);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<PointTransaction, bool>> predicate)
        {
            return await _db.PointTransactions.AnyAsync(predicate);
        }
    }
}
