using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess
{
    public class PaymentRecordDAO : IGenericDAO<PaymentRecord>
    {

        private readonly AppDbContext _db;

        public PaymentRecordDAO(AppDbContext db)
        {
            _db = db;
        }

        public IQueryable<PaymentRecord> GetAllQueryable(Expression<Func<PaymentRecord, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<PaymentRecord> query = _db.PaymentRecords;
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

        public async Task<PaymentRecord?> GetAsync(Expression<Func<PaymentRecord, bool>> predicate)
        {
            return await _db.PaymentRecords
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching PaymentRecord or null if none found
        }

        public async Task<bool> AddAsync(PaymentRecord entity)
        {
            _db.PaymentRecords.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(PaymentRecord entity)
        {
            _db.PaymentRecords.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(int PaymentRecordId)
        {
            var PaymentRecord = await _db.PaymentRecords.FindAsync(PaymentRecordId);
            if (PaymentRecord == null)
            {
                return false; // PaymentRecord not found
            }

            _db.PaymentRecords.Remove(PaymentRecord);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<PaymentRecord, bool>> predicate)
        {
            return await _db.PaymentRecords.AnyAsync(predicate);
        }
    }
}